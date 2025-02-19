using NLog;

/// <summary>
/// Simple class printing the telegram to the log
/// </summary>
public class LogPrinter : IUserVisualizable
{

    /// <summary>
    /// class logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

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
        log.Info(tg.ToStringDetailed());
    }
}