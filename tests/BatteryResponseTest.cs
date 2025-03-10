namespace RS485_Monitor.tests;
using NUnit.Framework;

public class BatteryResponseTest
{
    private BaseTelegram? batteryBase = null;

    [SetUp]
    public void Setup()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0x5A, 0x0A, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x30, 0x0D];
        batteryBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToBatteryReponseSuccess()
    {
        BatteryResponse status = new(batteryBase!);
        Assert.That(status, Is.Not.Null);
    }

    [Test]
    public void ConvertToBatteryResponseFail()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0x5A, 0x09, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x30, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        Assert.Throws<ArgumentException>(() => new BatteryResponse(invalidTelegram));
    }

    [Test]
    public void CheckContent()
    {
        BatteryResponse status = new(batteryBase!);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(status.Voltage, Is.EqualTo(77));
            Assert.That(status.SoC, Is.EqualTo(72));
            Assert.That(status.Temperature, Is.EqualTo(23));
            Assert.That(status.Current, Is.EqualTo(0x0));
            Assert.That(status.Cycles, Is.EqualTo(35));
            Assert.That(status.DischargeCycles, Is.EqualTo(11));
            Assert.That(status.VBreaker, Is.EqualTo(BatteryResponse.VBreakerStatus.OK));
            Assert.That(status.Activity, Is.EqualTo(BatteryResponse.BatteryActivity.NO_ACTIVITY));
            Assert.That(status.Charging, Is.False);
        }
    }

    [Test]
    public void ToStringTest()
    {
        BatteryResponse status = new(batteryBase!);
        string expected = "Battery Response: 77V, 72%, 23°C, 0 Amp, Charged: 35x, Discharged: 11x, VBreaker: OK, Activity: NO_ACTIVITY, Charging: False";
        Assert.That(status.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void BatteryResponseInvalidSourceOrDestination()
    {
        byte[] raw = [0xB6, 0x6B, 0xBB, 0xCC, 0x0A, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x30, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new BatteryResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a BatteryResponse telegram"));
    }

    [Test]
    public void BatteryResponseInvalidPDUSize()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0x5A, 0x0B, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x30, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new BatteryResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size of 11"));
    }
}
