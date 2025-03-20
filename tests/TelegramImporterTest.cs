namespace RS485_Monitor.tests;
using RS485_Monitor.Utils.Storage;
using NUnit.Framework;

[TestFixture]
public class TelegramImportTest
{
    public required Stream stream;
    public required List<BaseTelegram> telegrams;

    [OneTimeSetUp]
    public void SetUp()
    {
        stream = new MemoryStream();
        telegrams = [];

        telegrams.Add(new BaseTelegram([0xB6, 0x6B, 0xAA, 0x5A, 0x0A, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x30, 0x0D]));
        telegrams.Add(new BaseTelegram([0xC5, 0x5C, 0x5A, 0xAA, 0x01, 0x00, 0x30, 0x0D]));

        using TelegramExporter writer = new(stream, leaveOpen: true);
        foreach (BaseTelegram telegram in telegrams)
        {
            writer.PushTelegram(telegram);
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        stream.Dispose();
    }

    [SetUp]
    public void SetUpTest()
    {
        stream.Seek(0, SeekOrigin.Begin);
    }

    [Test]
    public void ReadHeaderTest()
    {
        using TelegramImporter reader = new(stream, leaveOpen: true);
        Assert.That(reader.FormatVersion, Is.EqualTo(1));
    }

    [Test]
    public void ReadTelegrams()
    {
        using TelegramImporter reader = new(stream, leaveOpen: true);
        List<BaseTelegram> actual = [];

        // read the telegrams
        foreach (BaseTelegram telegram in reader.GetTelegram())
        {
            actual.Add(telegram);
        }

        // Check that the read telegrams are the same as the ones we wrote
        Assert.That(actual, Is.EqualTo(telegrams));
    }
}
