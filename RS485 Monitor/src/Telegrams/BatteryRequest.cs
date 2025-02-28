using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information
/// of BatteryStatus
/// </summary>
public class BatteryRequest : BaseTelegram
{
    /// <summary>
    /// Internal logging object
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    /// <summary>
    /// Required size of PDU data
    /// </summary>
    private const byte PDU_LENGTH = 1;


    private const byte SOURCE = (byte)Units.ECU;
    private const byte DESTINATION = (byte)Units.BATTERY;
    public const UInt16 TELEGRAM_ID = DESTINATION << 8 | SOURCE;
    #endregion

    /// <summary>
    /// Create a new Battery Status object based on a received telegram
    /// </summary>
    /// <param name="t">Raw telegram</param>
    /// <exception cref="ArgumentException">Raw data has an unexpected length</exception>
    public BatteryRequest(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != PDU_LENGTH)
        {
            throw new ArgumentException($"Unexpected size of {t.PDU.Length}");
        }
        if (t.Source != SOURCE || t.Destination != DESTINATION)
        {
            throw new ArgumentException("Not a BatteryResponse telegram");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        return "Battery Request";
    }
}
