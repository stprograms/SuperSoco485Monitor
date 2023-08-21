using System.Reflection.Metadata.Ecma335;
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

    /// <summary>
    /// Possible activities of the battery
    /// </summary>
    public enum BatteryActivity
    {
        /// <summary>
        /// Battery is currently charging
        /// </summary>
        CHARGING = 0x01,

        /// <summary>
        /// Battery is currently discharging
        /// </summary>
        DISCHARGING = 0x04
    }

    public enum VBreakerStatus
    {
        /// <summary>
        /// BMS has stopped charging
        /// </summary>
        OK                      = 0,
        /// <summary>
        /// BMS has stopped charging
        /// </summary>
        BMS_STOPPED_CHARGE      = 1,
        /// <summary>
        /// Too high charge current
        /// </summary>
        HIGH_CURRENT_CHARGE     = 2,
        /// <summary>
        /// Too high discharge current
        /// </summary>
        HIGH_CURRENT_DISCHARGE  = 4
    }

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
    private const byte POS_CYCLE_H = 4;
    /// <summary>
    /// Position of low byte of number of charging cycles in PDU
    /// </summary>
    private const byte POS_CYCLE_L = 5;
    /// <summary>
    /// Position of VBreaker information
    /// </summary>
    private const byte POS_VBREAKER= 8;
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
    public sbyte Temperature { get => (sbyte)PDU[POS_TEMP]; }
    /// <summary>
    /// Current VBreaker status
    /// </summary>
    public VBreakerStatus VBreaker{ get => (VBreakerStatus)PDU[POS_VBREAKER]; }
    /// <summary>
    /// Current Charge or Discharge in Amps
    /// </summary>
    public sbyte Charge { get => (sbyte)PDU[POS_CHARGE]; }
    /// <summary>
    /// Total number of charging cycles
    /// </summary>
    public ushort Cycles { get => (ushort)((PDU[POS_CYCLE_H] << 8) | PDU[POS_CYCLE_L]); }
    /// <summary>
    /// current battery activity
    /// </summary>
    public BatteryActivity Activity
    {
        get => (BatteryActivity)PDU[POS_CHARGING];
    }

    /// <summary>
    /// Is Battery charging
    /// </summary>
    public bool Charging
    {
        get => Activity == BatteryActivity.CHARGING;
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
        log.Trace(base.ToString());
        return $"Battery Status: {Voltage}V, {SoC}%, {Temperature}°C, {Charge} Amp, {Cycles}x, VBreaker: {VBreaker}, Charging: {Charging}";
    }
}