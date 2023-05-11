using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information 
/// of ECUStatus
/// </summary>
public class GSMStatus : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    private const byte POS_HOUR = 4;
    private const byte POS_MINUTE = 5;

    public const byte RAW_DATA_LEN = 14;
    #endregion

    #region Properties
    public byte Hour { get => PDU[POS_HOUR]; }
    public byte Minutes { get => PDU[POS_MINUTE]; }

    #endregion

    /// <summary>
    /// Create a new ECUStatus Telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Unexpected size</exception>
    public GSMStatus(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != RAW_DATA_LEN)
        {
            throw new ArgumentException("Unexpected size");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        return $"GSM Status: Time {Hour:d2}:{Minutes:d2}";
    }
}