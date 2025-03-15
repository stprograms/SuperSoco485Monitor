namespace RS485_Monitor.tests;
using RS485_Monitor.Utils.Storage;
using NUnit.Framework;
using System.Text;

[TestFixture]
public class TelegramExporterTest
{

    [Test]
    public void WriteHeaderTest()
    {
        MemoryStream stream = new();
        using TelegramExporter writer = new(stream);

        // Extract identifier
        byte[] expected = Encoding.ASCII.GetBytes(TelegramExporter.IDENTIFIER);
        byte[] actual = new byte[expected.Length];

        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(actual, 0, expected.Length);

        // Extract version
        byte expected_version = 1;
        byte actual_version = (byte)stream.ReadByte();

        // Test the values
        Assert.That(actual, Is.EqualTo(expected));
        Assert.That(actual_version, Is.EqualTo(expected_version));
    }

    [Test]
    public void CreateFileSuccess() {
        string path = Path.GetTempFileName();

        using (TelegramExporter writer = new(path)) {
            Assert.That(File.Exists(path), Is.True);
        }

        using (Stream stream = new FileStream(path, FileMode.Open)) {

            // Read the content of the file and check the header
            using (var reader = new BinaryReader(stream)) {
                byte[] actual = new byte[TelegramExporter.IDENTIFIER.Length];
                reader.Read(actual, 0, TelegramExporter.IDENTIFIER.Length);
                byte actual_version = reader.ReadByte();

                byte[] expected = Encoding.ASCII.GetBytes(TelegramExporter.IDENTIFIER);
                byte expected_version = TelegramExporter.VERSION;

                Assert.That(actual, Is.EqualTo(expected));
                Assert.That(actual_version, Is.EqualTo(expected_version));
            }
        }

        // Cleanup
        File.Delete(path);
    }


    [Test]
    public void PushTelegrams() {
        MemoryStream stream = new();
        using TelegramExporter writer = new(stream);


        byte[] raw1 = [0xB6, 0x6B, 0xAA, 0x5A, 0x0A, 0x4D, 0x48, 0x17, 0x00, 0x00, 0x23, 0x00, 0x0B, 0x00, 0x00, 0x30, 0x0D];
        byte[] raw2 = [0xC5, 0x5C, 0x5A, 0xAA, 0x01, 0x00, 0x30, 0x0D];
        List<BaseTelegram> telegrams = [new BaseTelegram(raw1), new BaseTelegram(raw2)];

        // Push the first telegram to the writer
        foreach (var telegram in telegrams) {
            writer.PushTelegram(telegram);
        }

        // Jump over the header and start reading the telegrams
        var headerlen = TelegramExporter.IDENTIFIER.Length + 1;
        stream.Seek(headerlen, SeekOrigin.Begin);

        using (var reader = new BinaryReader(stream)) {
            foreach (var telegram in telegrams) {
                long timestamp = reader.ReadInt64();
                byte[] actual = new byte[telegram.Raw.Length];
                reader.Read(actual, 0, telegram.Raw.Length);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(timestamp, Is.EqualTo(telegram.TimeStamp.ToBinary()));
                    Assert.That(actual, Is.EqualTo(telegram.Raw));
                }
            }
        }
    }
}
