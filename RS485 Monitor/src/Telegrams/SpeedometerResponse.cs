using CommandLine;
using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information
/// of the SpeedometerRequest
/// </summary>
public class SpeedometerResponse : BaseTelegram
{
    /// <summary>
    /// Internal class logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants

    /// <summary>
    /// Destination of the telegram
    /// </summary>
    private const byte DESTINATION = (byte)Units.ECU;
    /// <summary>
    /// Source of the telegram
    /// </summary>
    private const byte SOURCE = (byte)Units.SPEEDOMETER;

    /// <summary>
    /// Calculated telegram ID
    /// </summary>
    public const UInt16 TELEGRAM_ID = DESTINATION << 8 | SOURCE;

    /// <summary>
    /// Length of the required raw data
    /// </summary>
    public const byte RAW_DATA_LEN = 1;
    #endregion


    /// <summary>
    /// Create a new GSMStatus telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Unexpected size</exception>
    public SpeedometerResponse(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != RAW_DATA_LEN)
        {
            throw new ArgumentException("Unexpected size");
        }
        if (t.Source != SOURCE || t.Destination != DESTINATION)
        {
            throw new ArgumentException("Not a SpeedometerResponse telegram");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        return $"Speedometer Response";
    }
}
