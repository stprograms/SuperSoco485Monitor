namespace RS485_Monitor.tests;
using NUnit.Framework;

public class SpeedometerRequestTest
{
    private BaseTelegram? speedometerBase;

    [SetUp]
    public void Setup()
    {
        byte[] raw = [0xC5, 0x5C, 0xBA, 0xAA, 0x0E, 0x34, 0x00, 0x00, 0x01, 0x0A, 0x26, 0x00, 0x00, 0x01, 0x01, 0x00, 0x01, 0x00, 0x48, 0x5E, 0x0D];
        speedometerBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToSpeedometerRequestSuccess()
    {
        SpeedometerRequest request = new(speedometerBase!);
        Assert.That(request, Is.Not.Null);
    }

    [Test]
    public void ConvertToSpeedometerRequestFail_InvalidPDUSize()
    {
        byte[] raw = [0xC5, 0x5C, 0xBA, 0xAA, 0x0F, 0x34, 0x00, 0x00, 0x01, 0x0A, 0x26, 0x00, 0x00, 0x01, 0x01, 0x00, 0x01, 0x00, 0x48, 0x00, 0x5E, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new SpeedometerRequest(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size"));
    }

    [Test]
    public void ConvertToSpeedometerRequestFail_InvalidSourceOrDestination()
    {
        byte[] raw = [0xC5, 0x5C, 0xBC, 0xAA, 0x0E, 0x34, 0x00, 0x04, 0x01, 0x0A, 0x26, 0x00, 0x00, 0x01, 0x01, 0x00, 0x01, 0x00, 0x48, 0x5E, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new SpeedometerRequest(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a SpeedometerRequest telegram"));
    }

    [Test]
    public void CheckContent()
    {
        // 0x34, 0x00, 0x00, 0x01, 0x0A, 0x26, 0x00, 0x00, 0x01, 0x01, 0x00, 0x01, 0x00, 0x48, 0x5E, 0x0D
        SpeedometerRequest request = new(speedometerBase!);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.Soc, Is.EqualTo(0x34));
            Assert.That(request.CtrlCurrent, Is.EqualTo(0.0).Within(0.1));
            Assert.That(request.Speed, Is.EqualTo(0));
            Assert.That(request.TempLevel, Is.EqualTo(0x01));
            Assert.That(request.Hour, Is.EqualTo(0x0A));
            Assert.That(request.Minutes, Is.EqualTo(0x26));
            Assert.That(request.ErrorValue.State, Is.EqualTo((ErrorCode.ErrorBits)0));
            Assert.That(request.State, Is.EqualTo(SpeedometerRequest.VehicleState.PARKING));
            Assert.That(request.Gear, Is.EqualTo(0x01));
            Assert.That(request.SpeedCtrlValue, Is.EqualTo(0x0001));
            Assert.That(request.Range, Is.EqualTo(0x48));
        }
    }

    [Test]
    public void ToStringTest()
    {
        SpeedometerRequest request = new(speedometerBase!);
        string expected = "Speedometer Request: Soc 52%, Current 0A, Speed 0km/h, TempLvl 1, Time 10:38, Error 0, Vehicle State PARKING, Gear 1, Speed Ctrl 1, Range 72km";
        Assert.That(request.ToString(), Is.EqualTo(expected));
    }
}
