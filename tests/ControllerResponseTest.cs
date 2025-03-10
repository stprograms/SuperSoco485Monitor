namespace RS485_Monitor.tests;
using NUnit.Framework;

public class ControllerResponseTest
{
    private BaseTelegram? ecuBase;

    [SetUp]
    public void Setup()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        ecuBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToEcuStatusSuccess()
    {
        ControllerResponse status = new(ecuBase!);
        Assert.That(status, Is.Not.Null);
    }

    [Test]
    public void CheckContent()
    {
        ControllerResponse status = new(ecuBase!);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(status.Gear, Is.EqualTo(2));
            Assert.That(status.Current, Is.EqualTo(0.4).Within(0.1));
            Assert.That(status.Speed, Is.EqualTo(0));
            Assert.That(status.Temperature, Is.EqualTo(0x13));
            Assert.That(status.IsParking, Is.EqualTo(true));
        }
    }

    [Test]
    public void CheckParkingStatus_On()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x02, 0x1C, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerResponse response = new(baseTelegram);

        Assert.That(response.Parking, Is.EqualTo(ControllerResponse.ParkStatus.PARKING_ON));
        Assert.That(response.IsParking, Is.True);
    }

    [Test]
    public void CheckParkingStatus_Off()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x01, 0x01, 0x1C, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerResponse response = new(baseTelegram);

        Assert.That(response.Parking, Is.EqualTo(ControllerResponse.ParkStatus.PARKING_OFF));
        Assert.That(response.IsParking, Is.False);
    }

    [Test]
    public void CheckParkingStatus_Unknown()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x03, 0xFF, 0x1C, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerResponse response = new(baseTelegram);

        Assert.That(response.Parking, Is.EqualTo(ControllerResponse.ParkStatus.PARKING_UNKNOWN));
        Assert.That(response.IsParking, Is.False);
    }

    [Test]
    public void ToStringTest()
    {
        ControllerResponse status = new(ecuBase!);
        string expected = "Controller Response: Mode 2, 0.4A, 0km/h, 19°C, Parking: PARKING_ON";
        Assert.That(status.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void InvalidPDUSize()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0B, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0xFF, 0x1C, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new ControllerResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size"));
    }

    [Test]
    public void InvalidSourceOrDestination()
    {
        byte[] raw = [0xB6, 0x6B, 0xBB, 0xCC, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new ControllerResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a ControllerResponse telegram"));
    }
}
