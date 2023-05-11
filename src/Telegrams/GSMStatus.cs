using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information 
/// of GSM / GPS Status
/// </summary>
public class GSMStatus : BaseTelegram
{
/// <summary>
/// Internal class logger{
/// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    /// <summary>
    /// Position of hour (Localtime)
    /// </summary>
    private const byte POS_HOUR = 4;
    /// <summary>
    /// Position of minute
    /// </summary>
    private const byte POS_MINUTE = 5;

/// <summary>
/// LEngth of the required raw data
/// </summary>
    public const byte RAW_DATA_LEN = 14;
    #endregion

    #region Properties
    /// <summary>
    /// Current hour (localtime)
    /// </summary>
    public byte Hour { get => PDU[POS_HOUR]; }
    /// <summary>
    /// Current minute (localtime)
    /// </summary>
    public byte Minutes { get => PDU[POS_MINUTE]; }

    #endregion

    /// <summary>
    /// Create a new GSMStatus telegram with the information of BaseTelegram
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