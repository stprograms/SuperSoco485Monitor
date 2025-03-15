using System.Text;

namespace RS485_Monitor.Utils.Storage
{
    /// <summary>
    /// This class will export the telegrams into a binary format to a stream.
    /// </summary>
    public class TelegramExporter :IDisposable
    {
        public const byte VERSION = 1;
        public const string IDENTIFIER = "RS485MONITOR";
        public static readonly byte[] MAGIC_NUMBER = Encoding.ASCII.GetBytes(IDENTIFIER);

        private readonly BinaryWriter _writer;

        /// <summary>
        /// Create a new TelegramExporter from a stream.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="leaveOpen">leave the stream open after reading</param>
        public TelegramExporter(Stream stream, bool leaveOpen = false)
        {
            _writer = new BinaryWriter(stream, new UTF8Encoding(), leaveOpen);
            WriteHeader();
        }

        public TelegramExporter(string path, bool leaveOpen = false)
        {
            FileStream stream = new(path, FileMode.Create);
            _writer = new BinaryWriter(stream, new UTF8Encoding(), leaveOpen);
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
