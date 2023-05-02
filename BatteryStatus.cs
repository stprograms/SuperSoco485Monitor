using NLog;

public class BatteryStatus : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Properties
    /// <summary>
    /// Current Battery Voltage
    /// </summary>
    public byte Voltage { get; }
    /// <summary>
    /// Current State of Charge in Percent
    /// </summary>
    public byte SoC { get; }
    /// <summary>
    /// Current temperature in degree Celcius
    /// </summary>
    public byte Temperature { get; }
    /// <summary>
    /// Current Charge or Discharge in Amps
    /// </summary>
    public byte Charge { get; }
    /// <summary>
    /// Total number of charging cycles
    /// </summary>
    public UInt16 Cycles { get; }
    /// <summary>
    /// Battery is charging
    /// </summary>
    public bool Charging { get; }
    #endregion

    /// <summary>
    /// Create a new Battery Status object based on a received telegram
    /// </summary>
    /// <param name="t">Raw telegram</param>
    /// <exception cref="ArgumentException"></exception>
    public BatteryStatus(BaseTelegram t)
    {
        if (t.PDU.Length != 10)
        {
            throw new ArgumentException("Unexpected size");
        }

        Voltage = t.PDU[0];
        SoC = t.PDU[1];
        Temperature = t.PDU[2];
        Charge = t.PDU[3];
        Cycles = (UInt16)((t.PDU[4] << 8) + t.PDU[5]);
        switch (t.PDU[6])
        {
            case 0:
                Charging = false;
                break;

            case 1:
                Charging = true;
                break;

            default:
                Charging = false;
                log.Trace($"Charging: 0x{t.PDU[6]:X2}");
                break;
        }
    }

    public override string ToString()
    {
        return $"Battery Status: {Voltage}V, {SoC}%, {Temperature}°C, {Charge} Amp, {Cycles}x";
    }
}