using System.Text;

namespace RS485_Monitor.Utils.Storage
{
    /// <summary>
    /// This class will export the telegrams into a binary format to a stream.
    /// </summary>
    public class TelegramWriter :IDisposable
    {
        public const byte VERSION = 1;
        public const string IDENTIFIER = "RS485MONITOR";
        public readonly byte[] MAGIC_NUMBER = Encoding.ASCII.GetBytes(IDENTIFIER);

        private readonly BinaryWriter _writer;

        public TelegramWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
            WriteHeader();
        }

        public TelegramWriter(string path)
        {
            FileStream stream = new(path, FileMode.Create);
            _writer = new BinaryWriter(stream);
            WriteHeader();
        }

        public void PushTelegram(BaseTelegram telegram)
        {
            _writer.Write(telegram.TimeStamp.ToBinary());
            _writer.Write(telegram.Raw);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

        protected void WriteHeader()
        {
            _writer.Write(MAGIC_NUMBER);
            _writer.Write(VERSION);
        }
    }
}
