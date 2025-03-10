namespace RS485_Monitor.tests;
using NUnit.Framework;

public class ControllerRequestTest
{
    private BaseTelegram? controllerBase = null;

    [SetUp]
    public void Setup()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x02, 0x00, 0x00, 0x01, 0x0D];
        controllerBase = new BaseTelegram(raw);
    }

    [Test]
    public void ConvertToControllerRequestSuccess()
    {
        ControllerRequest request = new(controllerBase!);
        Assert.That(request, Is.Not.Null);
    }

    [Test]
    public void ConvertToControllerRequestFail_InvalidPDUSize()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x03, 0x00, 0x01, 0x02, 0x03, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new ControllerRequest(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Unexpected size"));
    }

    [Test]
    public void ConvertToControllerRequestFail_InvalidSourceOrDestination()
    {
        byte[] raw = [0xC5, 0x5C, 0xBB, 0xCC, 0x02, 0x00, 0x00, 0x01, 0x0D];
        BaseTelegram invalidTelegram = new(raw);

        var ex = Assert.Throws<ArgumentException>(() => new ControllerRequest(invalidTelegram));
        Assert.That(ex.Message, Is.EqualTo("Not a ControllerRequest telegram"));
    }

    [Test]
    public void CheckChargingState_On()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x02, 0x00, 0x01, 0x01, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerRequest request = new(baseTelegram);

        Assert.That(request.Charge, Is.EqualTo(ControllerRequest.ChargingState.CHARGING_ON));
        Assert.That(request.IsCharging, Is.True);
    }

    [Test]
    public void CheckChargingState_Off()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x02, 0x00, 0x00, 0x00, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerRequest request = new(baseTelegram);

        Assert.That(request.Charge, Is.EqualTo(ControllerRequest.ChargingState.CHARGING_OFF));
        Assert.That(request.IsCharging, Is.False);
    }

    [Test]
    public void CheckChargingState_Unknown()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x02, 0x00, 0xFF, 0xFF, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerRequest request = new(baseTelegram);

        Assert.That(request.Charge, Is.EqualTo(ControllerRequest.ChargingState.CHARGING_UNKNOWN));
        Assert.That(request.IsCharging, Is.False);
    }

    [Test]
    public void ToStringTest()
    {
        byte[] raw = [0xC5, 0x5C, 0xDA, 0xAA, 0x02, 0x00, 0x01, 0x0F, 0x0D];
        BaseTelegram baseTelegram = new(raw);
        ControllerRequest request = new(baseTelegram);

        string expected = "Controller Request: CHARGING_ON";
        Assert.That(request.ToString(), Is.EqualTo(expected));
    }
}
