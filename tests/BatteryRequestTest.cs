namespace RS485_Monitor.tests;
using NUnit.Framework;

public class BatteryRequestTest
{
    [Test]
    public void BatteryRequestSuccessful()
    {
        byte[] raw = [0xC5, 0x5C, 0x5A, 0xAA, 0x01, 0x00, 0x01, 0x0D];

        BaseTelegram baseTelegram = new(raw);
        BatteryRequest batteryRequest = new(baseTelegram);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(batteryRequest.Type, Is.EqualTo(BaseTelegram.TelegramType.REQUEST));
            Assert.That(batteryRequest.Destination, Is.EqualTo((byte)Units.BATTERY));
            Assert.That(batteryRequest.Source, Is.EqualTo((byte)Units.ECU));
            Assert.That(batteryRequest.Valid, Is.EqualTo(true));
        }
    }

    [Test]
    public void BatteryRequestInvalidPDUSize()
    {
        byte[] raw = [0xC5, 0x5C, 0x5A, 0xAA, 0x02, 0x00, 0x01, 0x02, 0x0D];

        BaseTelegram baseTelegram = new(raw);
        var ex = Assert.Throws<ArgumentException>(() => new BatteryRequest(baseTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size of 2"));
    }

    [Test]
    public void BatteryRequestInvalidSourceOrDestination()
    {
        byte[] raw = [0xC5, 0x5C, 0xBB, 0xCC, 0x01, 0x00, 0x01, 0x0D];

        BaseTelegram baseTelegram = new(raw);
        var ex = Assert.Throws<ArgumentException>(() => new BatteryRequest(baseTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a BatteryResponse telegram"));
    }

    [Test]
    public void BatteryRequestToString()
    {
        byte[] raw = [0xC5, 0x5C, 0x5A, 0xAA, 0x01, 0x00, 0x01, 0x0D];

        BaseTelegram baseTelegram = new(raw);
        BatteryRequest batteryRequest = new(baseTelegram);

        string expected = "Battery Request";
        Assert.That(batteryRequest.ToString(), Is.EqualTo(expected));
    }
}
