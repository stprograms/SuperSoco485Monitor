using System.Text;
using RS485Monitor.Telegrams;

namespace RS485_Monitor.Utils.Storage
{
    /// <summary>
    /// This class will import telegrams from a stream created by the TelegramExporter.
    /// </summary>
    ///
    public class TelegramImporter : IDisposable
    {
        private readonly BinaryReader _reader;
        /// <summary>
        /// Version read from the stream
        /// </summary>
        public byte FormatVersion { get; }

        /// <summary>
        /// Create a new TelegramImporter from a stream.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <exception cref="InvalidDataException">Invalid file format</exception>
        public TelegramImporter(Stream stream, bool leaveOpen = false)
        {
            _reader = new BinaryReader(stream, new UTF8Encoding(), leaveOpen);
            FormatVersion = ReadHeader();
        }

        /// <summary>
        /// Create a new TelegramImporter from a file.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <exception cref="InvalidDataException">Invalid file format</exception>
        public TelegramImporter(string path, bool leaveOpen = false)
        {
            FileStream stream = new(path, FileMode.Open);
            _reader = new BinaryReader(stream, new UTF8Encoding(), leaveOpen);
            FormatVersion = ReadHeader();
        }

        /// <summary>
        /// Iterator function to iterate over all telegrams in the file.
        /// </summary>
        /// <returns>An enumerable of BaseTelegram objects.</returns>
        public IEnumerable<BaseTelegram> GetTelegram()
        {
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                yield return PopTelegram();
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }

        /// <summary>
        /// Read the header of the file and check if it is a valid file.
        /// </summary>
        protected byte ReadHeader()
        {
            byte[] magic_number = _reader.ReadBytes(TelegramExporter.IDENTIFIER.Length);
            byte version = _reader.ReadByte();

            if (!magic_number.SequenceEqual(TelegramExporter.MAGIC_NUMBER) || version != TelegramExporter.VERSION)
            {
                throw new InvalidDataException("Invalid file format");
            }

            return version;
        }

        /// <summary>
        /// Pop the next telegram from the stream
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Could not find an endTag</exception>
        /// <exception cref="EndOfStreamException"></exception>
        protected BaseTelegram PopTelegram()
        {
            long timestamp = _reader.ReadInt64();
            List<byte> raw = [];
            byte b;

            // Read until end telegram is found or max length is reached
            do
            {
                b = _reader.ReadByte();
                raw.Add(b);
            } while (b != BaseTelegram.END_TELEGRAM && raw.Count < BaseTelegram.MAX_RAW_DATA_LEN);

            if (raw.Count >= BaseTelegram.MAX_RAW_DATA_LEN)
            {
                throw new InvalidDataException("Endtag not found. File is corrupted.");
            }

            // Create and specialize the telegram
            BaseTelegram baseTelegram = new(raw.ToArray<byte>(), DateTime.FromBinary(timestamp));
            return TelegramSpecializer.Specialize(baseTelegram);
        }
    }
}
