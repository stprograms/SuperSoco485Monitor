using NLog;

/// <summary>
/// Simple class printing the telegram to the log.
///
/// It will print the telegrams in the following format:
/// - offset to the last telegram in milliseconds
/// - Raw data of the telegram
/// - Detailed string representation of the telegram
/// </summary>
public class LogPrinter : IUserVisualizable
{
    /// <summary>
    /// class logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    /// <summary>
    /// Timestamp of the last printed telegram.
    ///
    /// If no telegram was printed yet, it is null.
    /// </summary>
    private DateTime? lastTelegramTimestamp = null;

    /// <summary>
    /// Flush output
    /// </summary>
    public void Flush()
    {
        // Nothing to be done
    }

    /// <summary>
    /// Print the telegram using the detailed string
    /// </summary>
    /// <param name="tg"></param>
    public void PrintTelegram(BaseTelegram tg)
    {
        var offset = lastTelegramTimestamp.HasValue ? tg.TimeStamp - lastTelegramTimestamp.Value : TimeSpan.Zero;
        lastTelegramTimestamp = tg.TimeStamp;

        log.Info($"[{offset.TotalMilliseconds,5:N0} ms] {tg.ToStringDetailed()}");
    }
}
