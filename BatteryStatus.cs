using NLog;

/// <summary>
/// Specialization of the BaseTelegram class that extracts the information 
/// of BatteryStatus
/// </summary>
public class BatteryStatus : BaseTelegram
{
    /// <summary>
    /// Internal logging object
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Constants
    /// <summary>
    /// Required size of PDU data
    /// </summary>
    private const byte TELEGRAM_SIZE = 10;
    /// <summary>
    /// Position of battery voltage in PDU
    /// </summary>
    private const byte POS_VOLTAGE = 0;
    /// <summary>
    /// position of State of Charge in PDU
    /// </summary>
    private const byte POS_SOC = 1;
    /// <summary>
    /// Position of temperature in PDU
    /// </summary>
    private const byte POS_TEMP = 2;
    /// <summary>
    /// Position of charge / discharge current in PDU
    /// </summary>
    private const byte POS_CHARGE = 3;
    /// <summary>
    /// Positiion of high byte of number of charging cycles in PDU
    /// </summary>
    private const byte POS_CYCLE_H = 6;
    /// <summary>
    /// Position of low byte of number of charging cycles in PDU
    /// </summary>
    private const byte POS_CYCLE_L = 7;
    /// <summary>
    /// Position of charging information in PDU
    /// </summary>
    private const byte POS_CHARGING = 9;
    #endregion

    #region Properties
    /// <summary>
    /// Current Battery Voltage in Volts
    /// </summary>
    public byte Voltage { get => PDU[POS_VOLTAGE]; }
    /// <summary>
    /// Current State of Charge in Percent
    /// </summary>
    public byte SoC { get => PDU[POS_SOC]; }
    /// <summary>
    /// Current temperature in degree Celcius
    /// </summary>
    public byte Temperature { get => PDU[POS_TEMP]; }
    /// <summary>
    /// Current Charge or Discharge in Amps
    /// </summary>
    public double Charge
    {
        get
        {
            double val = PDU[POS_CHARGE];
            if (val >= 100) val /= 10.0;
            return val;
        }
    }
    /// <summary>
    /// Total number of charging cycles
    /// </summary>
    public UInt16 Cycles { get => (UInt16)((PDU[POS_CYCLE_H] << 8) + PDU[POS_CYCLE_L]); }
    /// <summary>
    /// Battery is charging
    /// </summary>
    public bool Charging
    {
        get
        {
            bool val = false;
            switch (PDU[POS_CHARGING])
            {
                case 0:
                    val = false;
                    break;

                case 1:
                    val = true;
                    break;

                default:
                    log.Trace($"Charging: 0x{PDU[POS_CHARGING]:X2}");
                    break;
            }
            return val;
        }
    }
    #endregion

    /// <summary>
    /// Create a new Battery Status object based on a received telegram
    /// </summary>
    /// <param name="t">Raw telegram</param>
    /// <exception cref="ArgumentException">Raw data has an unexpected length</exception>
    public BatteryStatus(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != TELEGRAM_SIZE)
        {
            throw new ArgumentException($"Unexpected size of {t.PDU.Length}");
        }
    }

    /// <summary>
    /// Get a string representation of the telegram
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        log.Debug(base.ToString());
        return $"Battery Status: {Voltage}V, {SoC}%, {Temperature}°C, {Charge} Amp, {Cycles}x, Charging: {Charging}";
    }
}