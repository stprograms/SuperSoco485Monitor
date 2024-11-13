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

    public const byte RAW_DATA_LEN = 10;
    #endregion

    /// <summary>
    /// Possible parking states
    /// </summary>
    public enum ParkStatus
    {
        PARKING_ON = 0x02,
        PARKING_OFF = 0x01,
    }

    #region Properties
    public byte Mode { get => PDU[POS_PDU]; }
    /// <summary>
    /// Current in Ampere
    /// </summary>
    public float Current { get => (float)((ushort)((PDU[POS_CURRENT_H] << 8) + PDU[POS_CURRENT_L]) * 0.1); }
    /// <summary>
    /// Current speed in km/h
    /// </summary>
    public float Speed { get => (float) ((ushort)((PDU[POS_SPEED_H] << 8) + PDU[POS_SPEED_L]) * 0.028); }
    /// <summary>
    /// Temperature in °C
    /// </summary>
    public sbyte Temperature { get => (sbyte)PDU[POS_TEMP]; }
    public ParkStatus Parking { get => (ParkStatus)PDU[POS_PARKING]; }
    public bool IsParking { get => Parking == ParkStatus.PARKING_ON; }

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
        return $"ECU Status: Mode {Mode}, {Current}A, {Speed}km/h, {Temperature}°C, Parking: {IsParking}";
    }
}