namespace RS485_Monitor.tests;
using NUnit.Framework;

public class ECUStatusTest
{
    private BaseTelegram? ecuBase;

    [SetUp]
    public void Setup()
    {
        byte[] raw = new byte[] { 0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D };
        ecuBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToEcuStatusSuccess()
    {
        ControllerResponse status = new(ecuBase!);
        Assert.That(status, Is.Not.Null);
    }

    [Test]
    public void ConvertToEcuStatusFail()
    {
        byte[] raw = new byte[] { 0xC5, 0x5C, 0xBA, 0xAA, 0x0E, 0x48, 0x00, 0x00, 0x00, 0x16, 0x19, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x40, 0x0A, 0x0D };
        BaseTelegram gsmTelegram = new(raw);

        Assert.Throws<ArgumentException>(() => new ControllerResponse(gsmTelegram));
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
}
