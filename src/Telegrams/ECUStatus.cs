using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information 
/// of ECUStatus
/// </summary>
public class ECUStatus : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    private const byte POS_PDU = 0;
    private const byte POS_CURRENT_H = 1;
    private const byte POS_CURRENT_L = 2;
    private const byte POS_SPEED_H = 3;
    private const byte POS_SPEED_L = 4;
    private const byte POS_TEMP = 5;
    private const byte POS_PARKING = 8;

    private const byte PARKING_ON = 2;
    private const byte PARKING_OFF = 1;
    public const byte RAW_DATA_LEN = 10;
    #endregion

    #region Properties
    public byte Mode { get => PDU[POS_PDU]; }
    public UInt16 Current { get => (UInt16)((PDU[POS_CURRENT_H] << 8) + PDU[POS_CURRENT_L]); }
    public UInt16 Speed { get => (UInt16)((PDU[POS_SPEED_H] << 8) + PDU[POS_SPEED_L]); }
    public byte Temperature { get => PDU[POS_TEMP]; }
    public bool Parking { get => PDU[POS_PARKING] == PARKING_ON; }

    #endregion

    /// <summary>
    /// Create a new ECUStatus Telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Unexpected size</exception>
    public ECUStatus(BaseTelegram t)
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
        return $"ECU Status: Mode {Mode}, {Current}mA, {Speed}km/h, {Temperature}°C, Parking: {Parking}";
    }
}