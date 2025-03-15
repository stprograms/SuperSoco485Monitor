namespace RS485_Monitor.tests;
using NUnit.Framework;
using RS485Monitor.Telegrams;

[TestFixture]
public class TelegramSpecializerTest
{
    [Test]
    public void Specialize_KnownTelegramType()
    {
        byte[] raw = [0xC5, 0x5C, 0xAA, 0xBA, 0x01, 0x00, 0x00, 0x0D];
        BaseTelegram telegram = new BaseTelegram(raw);
        BaseTelegram specializedTelegram = TelegramSpecializer.specialize(telegram);

        Assert.That(specializedTelegram, Is.InstanceOf<SpeedometerResponse>());
    }

    [Test]
    public void Specialize_UnknownTelegramType()
    {
        byte[] raw = [0xC5, 0x5C, 0xBB, 0xCC, 0x01, 0x00, 0x00, 0x0D];
        BaseTelegram telegram = new BaseTelegram(raw);
        BaseTelegram specializedTelegram = TelegramSpecializer.specialize(telegram);

        Assert.That(specializedTelegram, Is.InstanceOf<BaseTelegram>());
    }

}
