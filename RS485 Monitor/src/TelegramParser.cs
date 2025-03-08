using NLog;

/// <summary>
/// Class extracting telegrams from byte streams.
/// Feed the read bytes into ParseChunk() to start / continue parsing. If a
/// telegram could be extracted, the event NewTelegram is called.
/// </summary>
public class TelegramParser
{
    #region Constants
    /// <summary>
    /// Maximum length of a telegram
    /// </summary>
    public const uint MAX_TELEGRAM_LENGTH = 64;

    const byte READ_FIRST_BYTE = 0xB6;
    const byte READ_SECOND_BYTE = 0x6B;

    const byte WRITE_FIRST_BYTE = 0xC5;
    const byte WRITE_SECOND_BYTE = 0x5C;

    #endregion

    #region Public Members
    /// <summary>
    /// Arguments for the NewTelegram Event
    /// </summary>
    public class TelegramArgs : EventArgs
    {
        /// <summary>
        /// The extracted telegram
        /// </summary>
        public BaseTelegram Telegram { get; }
        /// <summary>
        /// Create new TelegramArgs
        /// </summary>
        /// <param name="t">telegram</param>
        public TelegramArgs(BaseTelegram t)
        {
            Telegram = t;
            Telegram.Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// New Telegram has been extracted from data stream
    /// </summary>
    public event EventHandler? NewTelegram;

    #endregion
    #region  Private Members
    /// <summary>
    /// Internal parser states
    /// </summary>
    private enum States
    {
        NO_BLOCK,
        FIRST_BYTE,
        READING_BLOCK
    };
    /// <summary>
    /// Current state
    /// </summary>
    private States state = States.NO_BLOCK;

    /// <summary>
    /// Internal logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// data buffer of the currently reading telegram
    /// </summary>
    private byte[] data = new byte[MAX_TELEGRAM_LENGTH];
    /// <summary>
    /// Offset inside the data buffer
    /// </summary>
    private uint offset = 0;

    #endregion

    /// <summary>
    /// Parse the given bytes
    /// </summary>
    /// <param name="rawData">rawData read from a source</param>
    public void ParseChunk(byte[] rawData)
    {
        foreach (byte b in rawData)
        {
            switch (state)
            {
                case States.NO_BLOCK:
                    if (b == READ_FIRST_BYTE || b == WRITE_FIRST_BYTE)
                    {
                        data[offset++] = b;
                        state = States.FIRST_BYTE;
                    }
                    break;

                case States.FIRST_BYTE:
                    data[offset++] = b;
                    if (b == READ_SECOND_BYTE || b == WRITE_SECOND_BYTE)
                    {
                        FinishBlock();
                        state = States.READING_BLOCK;
                    }
                    else if (offset != 0)
                    {
                        state = States.READING_BLOCK;
                    }
                    else
                    {
                        state = States.NO_BLOCK;
                        offset = 0;
                    }
                    break;

                case States.READING_BLOCK:
                    data[offset++] = b;
                    if (b == READ_FIRST_BYTE || b == WRITE_FIRST_BYTE)
                    {
                        state = States.FIRST_BYTE;
                    }
                    break;

                default:
                    log.Error($"Unknown state {state}");
                    break;
            }
        }
    }

    /// <summary>
    /// Parse the given file
    /// </summary>
    /// <param name="filePath"></param> 
    public void ParseFile(string filePath)
    {
        FileInfo info = new(filePath);
        using (FileStream s = info.OpenRead())
        {
            int i = 0;
            while ((i = s.ReadByte()) != -1)
            {
                // Convert the integer into a size 1 byte array and parse it
                byte[] ba = { (byte)i };
                ParseChunk(ba);
            }
        }
    }

    /// <summary>
    /// Finishes the current block, converts the raw data and updates the buffer
    /// to continue with a new block
    /// </summary>
    private void FinishBlock()
    {
        // finish previous block
        if (offset > 2)
        {
            byte[] telegram = new byte[offset - 2];
            Array.Copy(data, telegram, telegram.Length);

            // handle block
            BaseTelegram? t = ConvertBlock(telegram);
            if (t != null)
            {
                NewTelegram?.Invoke(this, new TelegramArgs(t));
            }

            byte[] buf = new byte[data.Length];
            buf[0] = data[offset - 2];
            buf[1] = data[offset - 1];
            offset = 2;
            data = buf;
        }
    }

    /// <summary>
    /// Convert the given raw data into a telegram. If known, also convert the
    /// telegram into the special telegram type
    /// </summary>
    /// <param name="raw">raw data to convert</param>
    /// <returns>Raw data converted to a telegram</returns>
    private BaseTelegram? ConvertBlock(byte[] raw)
    {
        BaseTelegram? tg = null;
        try
        {
            tg = new(raw);
        }
        catch (ArgumentException ae)
        {
            log.Error(ae, "Could not create a new base telegram");
        }

        // return on invalid telegram
        if (tg == null || tg.Valid == false)
        {
            return null;
        }

        // Check if we can convert 
        if (tg.Type == BaseTelegram.TelegramType.READ_RESPONSE)
        {
            if (tg.Source == 0xAA && tg.Destination == 0x5A && tg.PDU.Length == 10)
            {
                tg = new BatteryStatus(tg);
            }
            else if (tg.Source == 0xAA && tg.Destination == 0xDA && tg.PDU.Length == 10)
            {
                tg = new ECUStatus(tg);
            }
        }

        else if (tg.Type == BaseTelegram.TelegramType.READ_REQUEST)
        {
            if (tg.Source == 0xBA && tg.Destination == 0xAA && tg.PDU.Length == GSMStatus.RAW_DATA_LEN)
            {
                tg = new GSMStatus(tg);
            }
        }

        return tg;
    }

}