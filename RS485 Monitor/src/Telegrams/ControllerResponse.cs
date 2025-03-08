using System.Reflection.Metadata;
using NLog;
using NLog.Config;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information
/// of ControllerResponse
/// </summary>
public class ControllerResponse : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();


    #region Constants
    private const byte POS_GEAR = 0;
    private const byte POS_CURRENT_H = 1;
    private const byte POS_CURRENT_L = 2;
    private const byte POS_SPEED_H = 3;
    private const byte POS_SPEED_L = 4;
    private const byte POS_TEMP = 5;
    private const byte POS_ERROR_CODE = 6;
    private const byte POS_PARKING = 8;

    public const byte RAW_DATA_LEN = 10;

    public const byte SOURCE = (byte) Units.ENGINE_CONTROLLER;
    public const byte DESTINATION = (byte) Units.ECU;
    public const UInt16 TELEGRAM_ID = DESTINATION << 8 | SOURCE;
    #endregion

    /// <summary>
    /// Possible parking states
    /// </summary>
    public enum ParkStatus
    {
        PARKING_ON = 0x02,
        PARKING_OFF = 0x01,
        /// <summary>
        /// Invalid / unknown parking state
        /// </summary>
        PARKING_UNKNOWN = 0xFF
    }

    #region Properties
    public byte Gear { get => PDU[POS_GEAR]; }
    /// <summary>
    /// Current in Ampere
    /// </summary>
    public float Current { get => (float)((ushort)((PDU[POS_CURRENT_H] << 8) + PDU[POS_CURRENT_L]) * 0.1); }
    /// <summary>
    /// Current speed in km/h
    /// </summary>
    public float Speed { get => (float) ((ushort)((PDU[POS_SPEED_H] << 8) + PDU[POS_SPEED_L]) * 0.028); }
    /// <summary>
    /// Simple error code extraction
    /// </summary>
    /// <todo>Define error codes</todo>
    public byte ErrorCode { get => PDU[POS_ERROR_CODE]; }
    /// <summary>
    /// Temperature in °C
    /// </summary>
    public sbyte Temperature { get => (sbyte)PDU[POS_TEMP]; }
    /// <summary>
    /// Status of parking
    /// </summary>
    public ParkStatus Parking {
        get {
            ParkStatus status = ParkStatus.PARKING_UNKNOWN;
            if (Enum.IsDefined(typeof(ParkStatus), (int)PDU[POS_PARKING]))
            {
                status = (ParkStatus)PDU[POS_PARKING];
            }
            return status;
        }
    }
    /// <summary>
    /// Simple property if vehicle is parking
    /// </summary>
    public bool IsParking { get => Parking == ParkStatus.PARKING_ON; }
    #endregion

    /// <summary>
    /// Create a new ControllerResponse Telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Thrown when the size of the PDU is
    /// unexpected or the telegram is not a ControllerResponse telegram</exception>
    public ControllerResponse(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != RAW_DATA_LEN)
        {
            throw new ArgumentException("Unexpected size");
        }

        if (t.Source != SOURCE || t.Destination != DESTINATION)
        {
            throw new ArgumentException("Not a ControllerResponse telegram");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        string current_invariant = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Current}");
        return $"Controller Response: Mode {Gear}, {current_invariant}A, {Speed}km/h, {Temperature}°C, Parking: {Parking}";
    }
}
