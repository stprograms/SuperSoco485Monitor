/// <summary>
/// Specialized printer and display of data on the console
/// </summary>
public class ConsolePrinter
{
    /// <summary>
    /// Interval / refresh timer in milliseconds the data will be refreshed
    /// </summary>
    private const double PRINT_INTERVAL = 200;

    /// <summary>
    /// Internal class used for storing information per telegram
    /// </summary>
    private class TelegramInfo
    {
        /// <summary>
        /// Number of updates of the telegram (type)
        /// </summary>
        public uint Count { get; set; }
        /// <summary>
        /// Reference to the telegram
        /// </summary>
        public BaseTelegram Telegram { get; set; }

        /// <summary>
        /// Create new telegram info based on the given BaseTelegram
        /// </summary>
        /// <param name="t"></param>
        public TelegramInfo(BaseTelegram t)
        {
            Count = 1;
            Telegram = t;
        }
    };

    /// <summary>
    /// Dictionary holding the telegrams
    /// </summary>
    private readonly Dictionary<UInt16, TelegramInfo> telegrams;

    /// <summary>
    /// Used timer for refreshing the screen
    /// </summary>
    private readonly System.Timers.Timer refreshTimer;

    /// <summary>
    /// Different states for printing the data on the screen
    /// </summary>
    private enum PrintState
    {
        /// <summary>
        /// No data to print
        /// </summary>
        EMPTY,
        /// <summary>
        /// Printing of the data is necessary
        /// </summary>
        PRINT_REQUIRED
    };

    /// <summary>
    /// current print state
    /// </summary>
    private PrintState state;

    /// <summary>
    /// Create a new console printer
    /// </summary>
    public ConsolePrinter()
    {
        telegrams = new();
        state = PrintState.EMPTY;
        refreshTimer = new System.Timers.Timer(PRINT_INTERVAL)
        {
            AutoReset = false
        };

        // Periodic handler
        refreshTimer.Elapsed += (o, e) =>
        {
            // Print the screen if required
            if (state == PrintState.PRINT_REQUIRED)
            {
                PrintScreen();
            }

            // Restart the timer after printing
            if (o != null)
            {
                ((System.Timers.Timer)o).Start();
            }
        };

        // start timer
        refreshTimer.Start();

        // Print the header on the screen
        PrintHeader();
    }

    /// <summary>
    /// Make the cursor visible again
    /// </summary>
    ~ConsolePrinter()
    {
        Console.CursorVisible = true;
    }

    /// <summary>
    /// Add the telegram to the internal dictionary to be printed the next cycle
    /// </summary>
    /// <param name="tg">telegram to print</param>
    public void PrintTelegram(BaseTelegram tg)
    {
        // Generate key
        UInt16 key = (UInt16)((tg.Source << 8) + tg.Destination);

        lock (telegrams)
        {

            if (!telegrams.ContainsKey(key))
            {
                // New telegram
                telegrams[key] = new TelegramInfo(tg);
            }
            else
            {
                // Update telegram
                if (!telegrams[key].Telegram.Equals(tg))
                {
                    telegrams[key].Telegram = tg;
                }
                // UPdate entry
                telegrams[key].Count++;
            }
        }

        // Update state
        if (state == PrintState.EMPTY)
        {
            state = PrintState.PRINT_REQUIRED;
        }

        // Start timer if not enabled
        if (refreshTimer.Enabled == false)
        {
            PrintScreen();
            refreshTimer.Start();
        }
    }

    /// <summary>
    /// Print the screen if print is required and stop the refresh timer
    /// afterwards
    /// </summary>
    public void FlushAndStopTimer()
    {
        if (state == PrintState.PRINT_REQUIRED)
        {
            PrintScreen();
        }

        refreshTimer.Stop();
    }

    /// <summary>
    /// Print the data on the screen (without the header)
    /// </summary>
    private void PrintScreen()
    {
        if (state == PrintState.PRINT_REQUIRED)
        {
            lock (telegrams)
            {
                try
                {
                    // Set cursor to the first data line after the header and clear
                    // the screen for the number of telegrams in the dictionary
                    Console.SetCursorPosition(0, 2);
                    for (var i = 0; i < telegrams.Count; ++i)
                    {
                        Console.WriteLine(new String(' ', Console.BufferWidth));
                    }
                    Console.SetCursorPosition(0, 2);
                }
                catch (System.IO.IOException)
                {
                    // For VSCode debugger -> Print new line
                    Console.WriteLine();
                }

                // Print the telegrams
                foreach (var telegram in telegrams)
                {
                    BaseTelegram t = telegram.Value.Telegram;
                    Console.WriteLine($"({telegram.Value.Count:D3}) {t.ToStringDetailed()}");
                }
            }

            // Set print state to empty
            state = PrintState.EMPTY;
        }
    }

    /// <summary>
    /// Clear the screen and print the header
    /// </summary>
    private void PrintHeader()
    {
        try
        {
            Console.CursorVisible = false;
            Console.Clear();
            Console.WriteLine("(Count) Raw Data -> Parsed Data");
            Console.WriteLine("-----------------------------------");
        }
        catch (System.IO.IOException)
        {
            // VS Code debugger
            Console.WriteLine();
        }
    }
}