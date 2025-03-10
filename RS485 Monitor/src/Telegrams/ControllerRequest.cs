using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information
/// of ControllerResponse
/// </summary>
public class ControllerRequest : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();


    #region Constants
    private const byte POS_CHARGE_STATE = 1;
    private const byte RAW_DATA_LEN = 2;

    public const byte SOURCE = (byte)Units.ECU;
    public const byte DESTINATION = (byte)Units.ENGINE_CONTROLLER;
    public const UInt16 TELEGRAM_ID = DESTINATION << 8 | SOURCE;
    #endregion

    /// <summary>
    /// Possible parking states
    /// </summary>
    public enum ChargingState
    {
        CHARGING_ON = 0x01,
        CHARGING_OFF = 0x00,
        /// <summary>
        /// Invalid / unknown parking state
        /// </summary>
        CHARGING_UNKNOWN = 0xFF
    }

    #region Properties
    public ChargingState Charge
    {
        get
        {
            if (Enum.IsDefined(typeof(ChargingState), (int)PDU[POS_CHARGE_STATE]))
            {
                return (ChargingState)PDU[POS_CHARGE_STATE];
            }
            else
            {
                return ChargingState.CHARGING_UNKNOWN;
            }
        }
    }
    /// <summary>
    /// Simple property if vehicle is parking
    /// </summary>
    public bool IsCharging { get => Charge == ChargingState.CHARGING_ON; }
    #endregion

    /// <summary>
    /// Create a new ControllerResponse Telegram with the information of BaseTelegram
    /// </summary>
    /// <param name="t">BaseTelegram that holds the raw data</param>
    /// <exception cref="ArgumentException">Thrown when the size of the PDU is
    /// unexpected or the telegram is not a ControllerResponse telegram</exception>
    public ControllerRequest(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != RAW_DATA_LEN)
        {
            throw new ArgumentException("Unexpected size");
        }

        if (t.Source != SOURCE || t.Destination != DESTINATION)
        {
            throw new ArgumentException("Not a ControllerRequest telegram");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Trace(base.ToString());
        return $"Controller Request: {Charge}";
    }
}
