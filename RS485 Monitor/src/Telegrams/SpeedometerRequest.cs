using CommandLine;
using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information
/// of the SpeedometerRequest
/// </summary>
public class SpeedometerRequest : BaseTelegram
{
    /// <summary>
    /// Internal class logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    /// <summary>
    /// Position of SOC in the PDU
    /// </summary>
    private const byte POS_SOC = 0;
    /// <summary>
    /// Position of current in the PDU
    /// </summary>
    private const byte POS_CTRL_CURRENT = 1;
    /// <summary>
    /// Position of speed in the PDU
    /// </summary>
    private const byte POS_SPEED = 2;
    /// <summary>
    /// Position of temperature level in the PDU
    /// </summary>
    private const byte POS_TEMP_LEVEL = 3;
    /// <summary>
    /// Position of hour (Localtime) in the PDU
    /// </summary>
    private const byte POS_HOUR = 4;
    /// <summary>
    /// Position of minute in the PDU
    /// </summary>
    private const byte POS_MINUTE = 5;
    /// <summary>
    /// Position of error code high Byte in the PDU
    /// </summary>
    private const byte POS_ERR_1 = 6;
    /// <summary>
    /// Position of error code low Byte in the PDU
    /// </summary>
    private const byte POS_ERR_2 = 7;
    /// <summary>
    /// Position of vehicle state in the PDU
    /// </summary>
    private const byte POS_VEHICLE_STATE = 8;
    /// <summary>
    /// Position of gear in the PDU
    /// </summary>
    private const byte POS_GEAR = 9;
    /// <summary>
    /// Position of speed high Byte in the PDU
    /// </summary>
    private const byte POS_SPEED_H = 10;
    /// <summary>
    /// Position of speed low Byte in the PDU
    /// </summary>
    private const byte POS_SPEED_L = 11;
    /// <summary>
    /// Position of range in the PDU
    /// </summary>
    private const byte POS_RANGE = 13;

    /// <summary>
    /// Destination of the telegram
    /// </summary>
    private const byte DESTINATION = (byte)Units.SPEEDOMETER;
    /// <summary>
    /// Source of the telegram
    /// </summary>
    private const byte SOURCE = (byte)Units.ECU;

    /// <summary>
    /// Calculated telegram ID
    /// </summary>
    public const UInt16 TELEGRAM_ID = DESTINATION << 8 | SOURCE;

    /// <summary>
    /// Length of the required raw data
    /// </summary>
    public const byte RAW_DATA_LEN = 14;
    #endregion

    #region Properties

    /// <summary>
    /// State of Charge in %
    /// </summary>
    public byte Soc { get => PDU[POS_SOC]; }
    /// <summary>
    /// Current received from the engine controller in ampere
    /// </summary>
    public double CtrlCurrent { get => PDU[POS_CTRL_CURRENT] * 2.5; }
    /// <summary>
    /// Speed in km/h
    /// </summary>
    public byte Speed { get => PDU[POS_SPEED]; }
    /// <summary>
    /// Temperature level
    /// </summary>
    public byte TempLevel { get => PDU[POS_TEMP_LEVEL]; }
    /// <summary>
    /// Current hour (localtime)
    /// </summary>
    public byte Hour { get => PDU[POS_HOUR]; }
    /// <summary>
    /// Current minute (localtime)
    /// </summary>
    public byte Minutes { get => PDU[POS_MINUTE]; }
    public ErrorCode ErrorValue {
        get => new ErrorCode(PDU[POS_ERR_1] << 8 | PDU[POS_ERR_2]);
    }

    /// <summary>
    /// Current state of the vehicle
    /// </summary>
    public VehicleState State
    {
        get
        {
            if (Enum.IsDefined(typeof(VehicleState), (Int32)PDU[POS_VEHICLE_STATE]))
            {
                return (VehicleState)PDU[POS_VEHICLE_STATE];
            }
            return VehicleState.UNKNOWN;
        }
    }
    /// <summary>
    /// Currently selected gear
    /// </summary>
    public byte Gear { get => PDU[POS_GEAR]; }
    /// <summary>
    /// Current speed
    /// </summary>
    public UInt16 SpeedCtrlValue { get => (UInt16)(PDU[POS_SPEED_H] << 8 | PDU[POS_SPEED_L]); }
    /// <summary>
    /// Remaining range in km
    /// </summary>
    public byte Range { get => PDU[POS_RANGE]; }

    #endregion

    /// <summary>
    /// Different states of the vehicle
    /// </summary>
    public enum VehicleState
    {
        /// <summary>
        /// Vehicle is charging
        /// </summary>
        CHARGING = 4,
        /// <summary>
        /// Vehicle is parking
        /// </summary>
        PARKING = 1,
        /// <summary>
        /// Vehicle is active
        /// </summary>
        ACTIVE = 0,
        /// <summary>
        /// Vehicle has an unknown state
        /// </summary>
        UNKNOWN = 0xFF
    }

    /// <summary>
    /// Create a new GSMStatus telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Unexpected size</exception>
    public SpeedometerRequest(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != RAW_DATA_LEN)
        {
            throw new ArgumentException("Unexpected size");
        }
        if (t.Source != SOURCE || t.Destination != DESTINATION)
        {
            throw new ArgumentException("Not a SpeedometerRequest telegram");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        return $"Speedometer Request: Soc {Soc}%, Current {CtrlCurrent}A, " +
                $"Speed {Speed}km/h, TempLvl {TempLevel}, Time {Hour:d2}:{Minutes:d2}, " +
                $"Error {ErrorValue.State}, Vehicle State {State}, Gear {Gear}, " +
                $"Speed Ctrl {SpeedCtrlValue}, Range {Range}km";
    }
}
