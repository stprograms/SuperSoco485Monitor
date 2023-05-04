using NLog;

/// <summary>
/// This class defines a basic telegram with data. No special parsing of the 
/// content is done here. Check the specialized classes for more detail
/// </summary>
public class BaseTelegram
{
    /// <summary>
    /// Internal logging structure
    /// </summary>
    static readonly Logger log = LogManager.GetCurrentClassLogger();

    #region Properties
    /// <summary>
    /// Start Sequence of the telegram
    /// </summary>
    public UInt16 Start { get => (UInt16)((Raw[0] << 8) + Raw[1]); }

    /// <summary>
    /// Source of the telegram
    /// </summary>
    public byte Source { get => Raw[POS_SRC]; }

    /// <summary>
    /// Destination of the telegram
    /// </summary>
    public byte Destination { get => Raw[POS_DES]; }

    /// <summary>
    /// User Data
    /// </summary>
    public byte[] PDU { get; }

    /// <summary>
    /// Received checksum
    /// </summary>
    public byte Checksum { get => Raw[POS_LEN + PDU.Length + 1]; }

    /// <summary>
    /// Copy of the received raw data of the telegram
    /// </summary>
    public byte[] Raw { get; }

    /// <summary>
    /// Are the raw data valid against the Checksum
    /// </summary>
    public bool Valid { get; }

    public enum TelegramType
    {
        READ_REQUEST = 0xC55C,
        READ_RESPONSE = 0xB66B
    }
    /// <summary>
    /// Type of the telegram
    /// </summary>
    public TelegramType Type { get => (TelegramType)Start; }

    #endregion

    #region Constants
    /// <summary>
    /// Maximum supported data length
    /// </summary>
    private const byte MAX_DATA_LEN = 32;

    /// <summary>
    /// End byte of the telegram
    /// </summary>
    private const byte END_TELEGRAM = 0x0D;

    /// <summary>
    /// Position of the source id in the raw data
    /// </summary>
    private const byte POS_SRC = 2;
    /// <summary>
    /// Position of the destination id in the raw data
    /// </summary>
    private const byte POS_DES = 3;
    /// <summary>
    /// Position of the data length in the raw data
    /// </summary>
    private const byte POS_LEN = 4;
    #endregion

    /// <summary>
    /// Internal constructor for specialized classes
    /// </summary>
    protected BaseTelegram()
    {
        PDU = new byte[1];
        Raw = new byte[1];
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="c">Object to copy from</param>
    protected BaseTelegram(BaseTelegram c)
    {
        this.Raw = new byte[c.Raw.Length];
        Array.Copy(c.Raw, this.Raw, Raw.Length);

        this.PDU = new byte[c.PDU.Length];
        Array.Copy(c.PDU, this.PDU, this.PDU.Length);
        
        this.Valid = c.Valid;
    }

    /// <summary>
    /// Create a new base telegram based on the given raw data
    /// </summary>
    /// <param name="rawData">raw data of one telegram</param>
    /// <exception cref="ArgumentException">Given data length in the raw data is invalid.</exception>
    public BaseTelegram(byte[] rawData)
    {
        // Copy raw data
        Raw = new byte[rawData.Length];
        Array.Copy(rawData, Raw, Raw.Length);

        // fetch user data
        byte dataLen = rawData[POS_LEN];
        if (dataLen > MAX_DATA_LEN)
        {
            throw new ArgumentException($"Invalid data len {dataLen}. Max supported: {MAX_DATA_LEN}");
        }
        PDU = new byte[dataLen];
        Array.Copy(rawData, POS_LEN + 1, PDU, 0, PDU.Length);

        /* Verify checksum
         * Checksum is calculated as XOR of the user data, including the length byte
         */
        byte calcCheck = dataLen;
        foreach (byte b in PDU)
        {
            calcCheck ^= b;
        }
        log.Trace($"Checksum: read={Checksum.ToString("X2")}, calc={calcCheck.ToString("X2")}");
        Valid = (calcCheck == Checksum);
        if (Valid == false)
        {
            log.Warn($"Telegram has an invalid checksum read:{Checksum.ToString("X2")}, calc:{calcCheck.ToString("X2")}");
        }

        // Check end telegram
        try
        {
            if (rawData.Length < (5 + 1 + dataLen) || rawData[5 + 1 + dataLen] != END_TELEGRAM)
            {
                log.Warn("RawData does not hold Endtag");
            }
        }
        catch (IndexOutOfRangeException)
        {
            log.Warn("Rawdata does not contain End tag");
        }
    }

    /// <summary>
    /// String representation of the telegram. Prints the raw data as hex bytes
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        System.Text.StringBuilder hex = new(Raw.Length * 3);

        foreach (byte b in Raw)
            hex.AppendFormat("{0:X2} ", b);

        return hex.ToString();
    }
}