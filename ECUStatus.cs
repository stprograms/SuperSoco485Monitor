using NLog;

public class ECUStatus : BaseTelegram
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Properties
    public byte Mode { get; }
    public UInt16 Current { get; }
    public UInt16 Speed { get; }
    public byte Temperature { get; }
    public bool Parking { get; }

    #endregion


    public ECUStatus(BaseTelegram t)
    : base(t)
    {
        if (t.PDU.Length != 10)
        {
            throw new ArgumentException("Unexpected size");
        }

        Mode = PDU[0];
        Current = (UInt16)((PDU[1] << 8) + PDU[2]);
        Speed = (UInt16)((PDU[3] << 8) + PDU[4]);
        Temperature = PDU[5];
            // Parking == 2 on , ==1 off
        Parking = PDU[8] == 2;
    }


    public override string ToString()
    {
        log.Debug(base.ToString());
        return $"ECU Status: Mode {Mode}, {Current}mA, {Speed}km/h, {Temperature}°C, Parking: {Parking}";
    }
}