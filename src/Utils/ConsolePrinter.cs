public class ConsolePrinter
{
    private const double PRINT_INTERVAL = 200;

    private class TelegramInfo
    {
        public uint Count { get; set; }
        public BaseTelegram Telegram { get; set; }

        public TelegramInfo(BaseTelegram t)
        {
            Count = 1;
            Telegram = t;
        }
    };

    private Dictionary<UInt16, TelegramInfo> telegrams;

    private System.Timers.Timer timer;


    private enum PrintState
    {
        EMPTY,
        PRINT_REQUIRED,
        PRINTING
    };
    PrintState state;


    public ConsolePrinter()
    {
        telegrams = new();
        state = PrintState.EMPTY;
        timer = new System.Timers.Timer(PRINT_INTERVAL)
        {
            AutoReset = false
        };
        timer.Elapsed += (o, e) =>
        {
            if (state == PrintState.PRINT_REQUIRED)
            {
                PrintScreen();
            }
            this.timer.Start();
        };

        // start timer
        timer.Start();

        PrintHeader();
    }

    ~ConsolePrinter()
    {
        Console.CursorVisible = true;
    }

    public void PrintTelegram(BaseTelegram tg)
    {
        UInt16 key = (UInt16)((tg.Source << 8) + tg.Destination);

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

        // Update state
        if (state == PrintState.EMPTY)
        {
            state = PrintState.PRINT_REQUIRED;
        }

        // Start timer
        if (timer.Enabled == false)
        {
            PrintScreen();
            timer.Start();
        }
    }

    public void FlushAndStopTimer()
    {
        if (state == PrintState.PRINT_REQUIRED)
        {
            PrintScreen();
        }

        timer.Stop();
    }

    private void PrintScreen()
    {
        if (state == PrintState.PRINT_REQUIRED)
        {
            state = PrintState.PRINTING;

            //PrintHeader();
            try
            {
                Console.SetCursorPosition(0, 2);
                for (var i = 0; i < telegrams.Count; ++i)
                {
                    Console.WriteLine(new String(' ', Console.BufferWidth));
                }
                Console.SetCursorPosition(0, 2);
            }catch (System.IO.IOException)
            {
                // For VSCode debugger -> Print new line
                Console.WriteLine();
            }

            foreach (var telegram in telegrams)
            {
                BaseTelegram t = telegram.Value.Telegram;
                //char changed = telegram.Value.changed ? '*' : ' ';
                Console.WriteLine($"({telegram.Value.Count:D3}) {t.ToStringDetailed()}");

            }

            state = PrintState.EMPTY;
        }


    }
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
            Console.WriteLine();
        }
    }
}