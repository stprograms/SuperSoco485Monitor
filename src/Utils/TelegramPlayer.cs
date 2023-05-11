public class TelegramPlayer
{
    /// <summary>
    /// Default interval between telegrams in ms
    /// </summary>
    private const double DEFAULT_INTERVAL = 5;

    public List<BaseTelegram> Telegrams { get; }

    private int position = -1;
    private double interval = DEFAULT_INTERVAL;

    private System.Timers.Timer timer;

    private SemaphoreSlim signal = new SemaphoreSlim(0, 1);

    public event EventHandler<TelegramParser.TelegramArgs>? TelegramReceived = null;

    public event EventHandler? PlaybackFinished = null;

    public TelegramPlayer(double? interval = null)
    {
        this.Telegrams = new();

        if (interval != null)
        {
            this.interval = (double)interval;
        }

        timer = new(this.interval) { AutoReset = false };
        timer.Elapsed += (o, e) =>
        {
            // Timer elapsed callback
            TelegramReceived?.Invoke(this, new TelegramParser.TelegramArgs(Telegrams[++position]));
            if (position >= Telegrams.Count - 1)
            {
                signal.Release();
                PlaybackFinished?.Invoke(this, new EventArgs());
            }
            else
            {
                timer.Start();
            }
        };
    }

    public void AddTelegram(BaseTelegram t)
    {
        Telegrams.Add(t);
    }


    public Task ReplayTelegramsAsync()
    {
        return Task.Run(async () =>
        {
            if (TelegramReceived == null)
            {
                throw new InvalidOperationException("No eventHandler for TelegramReceived registered");
            }
            timer.Start();
            await signal.WaitAsync();
        });
    }
}