using NLog;

public class TelegramParser
{

    public delegate void TelegramHandler(byte[] raw);
    public event TelegramHandler? NewTelegram;

    private enum States
    {
        NO_BLOCK,
        FIRST_BYTE,
        READING_BLOCK
    };
    private States state = States.NO_BLOCK;

    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    private byte[] data = new byte[32];
    private uint offset = 0;

    const byte READ_FIRST_BYTE = 0xB6;
    const byte READ_SECOND_BYTE = 0x6B;
    
    const byte WRITE_FIRST_BYTE = 0xC5;
    const byte WRITE_SECOND_BYTE = 0x5C;

    public void ParseFile(string filePath)
    {
        FileInfo info = new(filePath);
        using (FileStream s = info.OpenRead())
        {
            int i = 0;
            while ((i = s.ReadByte()) != -1)
            {
                byte b = (byte)i;

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
                            // finish previous block
                            if (offset > 2)
                            {
                                byte[] telegram = new byte[offset -2];
                                Array.Copy(data, telegram, telegram.Length);

                                // todo handle block
                                NewTelegram?.Invoke(telegram);

                                byte[] buf = new byte[data.Length];
                                buf[0] = data[offset - 2];
                                buf[1] = data[offset - 1];
                                offset = 2;
                                data = buf;
                            }
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


    }

}