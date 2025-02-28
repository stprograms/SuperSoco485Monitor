namespace RS485_Monitor.tests;
using NUnit.Framework;

public class BatteryStatusTest
{
    private BaseTelegram? batteryBase = null;

    [SetUp]
    public void Setup()
    {
        byte[] raw = new byte[] { 0xB6, 0x6B, 0xAA, 0x5A, 0x0A, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x30, 0x0D };
        batteryBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToBatteryStatusSuccess()
    {
        BatteryResponse status = new(batteryBase!);
        Assert.That(status, Is.Not.Null);
    }

    [Test]
    public void ConvertToBatteryStatusFail()
    {
        byte[] raw = new byte[] { 0xB6, 0x6B, 0xAA, 0x5A, 0x09, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x30, 0x0D };
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
}
