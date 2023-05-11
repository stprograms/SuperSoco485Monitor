/// <summary>
/// Player for replaying telegrams
/// </summary>
public class TelegramPlayer
{
    /// <summary>
    /// Default interval between telegrams in ms
    /// </summary>
    private const double DEFAULT_INTERVAL = 5;

    /// <summary>
    /// List of telegrams
    /// </summary>
    public List<BaseTelegram> Telegrams { get; }

    /// <summary>
    /// Enumerator of the Telegram list
    /// </summary>
    private List<BaseTelegram>.Enumerator it;

    /// <summary>
    /// Timer used for emitting data
    /// </summary>
    private System.Timers.Timer timer;

    /// <summary>
    /// Semaphore used for signalling playback finished
    /// </summary>
    private SemaphoreSlim finishedSem = new SemaphoreSlim(0, 1);

    /// <summary>
    /// Event called for emitting telegrams
    /// </summary>
    public event EventHandler<TelegramParser.TelegramArgs>? TelegramEmitted = null;

    /// <summary>
    /// Create a new TelegramPlayer
    /// </summary>
    /// <param name="interval">optional interval to use for playback</param>
    public TelegramPlayer(double? interval = null)
    {
        this.Telegrams = new();
        it = Telegrams.GetEnumerator();

        // Use given interval or default
        double ival = DEFAULT_INTERVAL;
        if (interval != null)
        {
            ival = (double)interval;
        }

        // initialze the timer
        timer = new(ival) { AutoReset = false };
        timer.Elapsed += (o, e) =>
        {
            // Timer elapsed callback
            var tel = it.Current;
            if (tel != null && TelegramEmitted != null)
            {
                TelegramEmitted.Invoke(this, new TelegramParser.TelegramArgs(tel));
            }

            // Got to next entry or emit finished semaphore
            if (it.MoveNext())
            {
                timer.Start();
            }
            else
            {
                finishedSem.Release();
            }
        };
    }

    /// <summary>
    /// Add the telegram to the list
    /// </summary>
    /// <param name="t">Telegram to add</param>
    public void AddTelegram(BaseTelegram t)
    {
        Telegrams.Add(t);
    }

    /// <summary>
    /// Replay the telegrams asynchronously.
    /// </summary>
    /// <returns>Task</returns>
    /// <exception cref="InvalidOperationException">No eventhandler added to
    /// TelegramEmitted</exception>
    public Task ReplayTelegramsAsync()
    {
        return Task.Run(async () =>
        {
            if (TelegramEmitted == null)
            {
                throw new InvalidOperationException("No eventHandler for " +
                "TelegramReceived registered");
            }
            timer.Start();

            // Get current enumerator
            it = Telegrams.GetEnumerator();

            if (it.MoveNext())
            {
                // wait until the finished semaphore has been set
                // (only if data is available in the enumerator)
                await finishedSem.WaitAsync();
            }

        });
    }
}