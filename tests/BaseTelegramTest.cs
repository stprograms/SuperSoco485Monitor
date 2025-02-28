namespace RS485_Monitor.tests;
using NUnit.Framework;

public class BaseTelegramTest
{
    [Test]
    public void ParseTelegramSuccessful()
    {
        byte[] raw = new byte[] { 0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D };

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.RESPONSE));
        Assert.That(telegram.Destination, Is.EqualTo(0xDA));
        Assert.That(telegram.Source, Is.EqualTo(0xAA));
        Assert.That(telegram.Valid, Is.EqualTo(true));
    }

    [Test]
    public void ParseTelegramRequestSuccessful()
    {
        byte[] raw = new byte[] { 0xC5, 0x5C, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D };

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.REQUEST));
        Assert.That(telegram.Destination, Is.EqualTo(0xDA));
        Assert.That(telegram.Source, Is.EqualTo(0xAA));
        Assert.That(telegram.Valid, Is.EqualTo(true));
    }

    [Test]
    public void ParseTelegramWrongChecksum()
    {
        byte[] raw = new byte[] { 0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x10, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D };

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.RESPONSE));
        Assert.That(telegram.Destination, Is.EqualTo(0xDA));
        Assert.That(telegram.Source, Is.EqualTo(0xAA));
        Assert.That(telegram.Valid, Is.EqualTo(false));
    }

}
