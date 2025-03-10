namespace RS485_Monitor.tests;
using NUnit.Framework;

public class SpeedometerResponseTest
{
    private BaseTelegram? speedometerBase;

    [SetUp]
    public void Setup()
    {
        byte[] raw = [0xC5, 0x5C, 0xAA, 0xBA, 0x01, 0x00, 0x00, 0x0D];
        speedometerBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToSpeedometerResponseSuccess()
    {
        SpeedometerResponse response = new(speedometerBase!);
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public void ConvertToSpeedometerResponseFail_InvalidPDUSize()
    {
        byte[] raw = [0xC5, 0x5C, 0xAA, 0xBA, 0x02, 0x00, 0x00, 0x00, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new SpeedometerResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size"));
    }

    [Test]
    public void ConvertToSpeedometerResponseFail_InvalidSourceOrDestination()
    {
        byte[] raw = [0xC5, 0x5C, 0xBB, 0xCC, 0x01, 0x00, 0x00, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new SpeedometerResponse(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a SpeedometerResponse telegram"));
    }

    [Test]
    public void ToStringTest()
    {
        SpeedometerResponse response = new(speedometerBase!);
        string expected = "Speedometer Response";
        Assert.That(response.ToString(), Is.EqualTo(expected));
    }
}
