namespace RS485_Monitor.tests;
using NUnit.Framework;

public class BaseTelegramTest
{
    [Test]
    public void ParseTelegramSuccessful()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.RESPONSE));
        Assert.That(telegram.Destination, Is.EqualTo(0xAA));
        Assert.That(telegram.Source, Is.EqualTo(0xDA));
        Assert.That(telegram.Valid, Is.EqualTo(true));
    }

    [Test]
    public void ParseTelegramRequestSuccessful()
    {
        byte[] raw = [0xC5, 0x5C, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.REQUEST));
        Assert.That(telegram.Destination, Is.EqualTo(0xAA));
        Assert.That(telegram.Source, Is.EqualTo(0xDA));
        Assert.That(telegram.Valid, Is.EqualTo(true));
    }

    [Test]
    public void ParseTelegramWrongChecksum()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x10, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);

        Assert.That(telegram.Type, Is.EqualTo(BaseTelegram.TelegramType.RESPONSE));
        Assert.That(telegram.Destination, Is.EqualTo(0xAA));
        Assert.That(telegram.Source, Is.EqualTo(0xDA));
        Assert.That(telegram.Valid, Is.EqualTo(false));
    }

    [Test]
    public void ParseTelegramTooShort()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA];

        var ex = Assert.Throws<ArgumentException>(() => new BaseTelegram(raw));
        Assert.That(ex.Message, Is.EqualTo("Raw data is too short"));
    }

    [Test]
    public void ParseTelegramInvalidDataLength()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x21, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        var ex = Assert.Throws<ArgumentException>(() => new BaseTelegram(raw));
        Assert.That(ex.Message, Is.EqualTo("Invalid data len 33. Max supported: 32"));
    }

    [Test]
    public void ParseTelegramMissingEndTag()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C];

        var ex = Assert.Throws<ArgumentException>(() => new BaseTelegram(raw));
        Assert.That(ex.Message, Is.EqualTo("Raw data does not contain End tag"));
    }

    [Test]
    public void ToStringTest()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);

        string expected = "B6 6B AA DA 0A 02 00 04 00 00 13 00 00 02 01 1C 0D ";
        Assert.That(telegram.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void ToStringDetailedTest()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);

        string expected = "B6 6B AA DA 0A 02 00 04 00 00 13 00 00 02 01 1C 0D ";
        Assert.That(telegram.ToStringDetailed(), Is.EqualTo(expected));
    }

    [Test]
    public void EqualsTest()
    {
        byte[] raw1 = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        byte[] raw2 = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        DateTime ts = DateTime.Now;

        BaseTelegram telegram1 = new(raw1, timestamp: ts);
        BaseTelegram telegram2 = new(raw2, timestamp: ts);

        Assert.That(telegram1.Equals(telegram2), Is.EqualTo(true));
    }

    [Test]
    public void NotEqualsDataTest()
    {
        byte[] raw1 = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        byte[] raw2 = [0xC5, 0x5C, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        DateTime ts = DateTime.Now;

        BaseTelegram telegram1 = new(raw1, timestamp: ts);
        BaseTelegram telegram2 = new(raw2, timestamp: ts);

        Assert.That(telegram1.Equals(telegram2), Is.EqualTo(false));
    }

    [Test]
    public void NotEqualsTimestampTest()
    {
        byte[] raw1 = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        byte[] raw2 = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        DateTime ts = DateTime.Now;

        BaseTelegram telegram1 = new(raw1, timestamp: ts);
        BaseTelegram telegram2 = new(raw2, timestamp: ts - TimeSpan.FromMinutes(1));

        Assert.That(telegram1.Equals(telegram2), Is.EqualTo(false));
    }

    [Test]
    public void AutomaticTimeStampCreated()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];

        BaseTelegram telegram = new(raw);
        var now = DateTime.Now;

        Assert.That(telegram.TimeStamp.Ticks, Is.EqualTo(now.Ticks).Within(300));
    }

    [Test]
    public void ManualTimeStampCreated()
    {
        byte[] raw = [0xB6, 0x6B, 0xAA, 0xDA, 0x0A, 0x02, 0x00, 0x04, 0x00, 0x00, 0x13, 0x00, 0x00, 0x02, 0x01, 0x1C, 0x0D];
        DateTime timestamp = new(2021, 12, 24, 12, 0, 0);

        BaseTelegram telegram = new(raw, timestamp);

        Assert.That(telegram.TimeStamp, Is.EqualTo(timestamp));
    }
}
