using NLog;

/// <summary>
/// This class defines a basic telegram with data. No special parsing of the
/// content is done here. Check the specialized classes for more detail
/// </summary>
public class BaseTelegram : IEquatable<BaseTelegram>
{
    /// <summary>
    /// Internal logging structure
    /// </summary>
    static readonly Logger log = LogManager.GetCurrentClassLogger();


    #region Constants
    /// <summary>
    /// Maximum supported data length
    /// </summary>
    private const byte MAX_DATA_LEN = 32;

    /// <summary>
    /// Minimum length of a telegram. This contains the following data:
    /// - Type (2 bytes)
    /// - Destination (1 byte)
    /// - Source (1 byte)
    /// - Data length (1 byte)
    /// - Checksum (1 byte)
    /// - End byte (1 byte)
    /// </summary>
    private const byte MIN_DATA_LEN = 7;

    /// <summary>
    /// End byte of the telegram
    /// </summary>
    private const byte END_TELEGRAM = 0x0D;

    /// <summary>
    /// Offset of the high Byte of the the telegram type
    /// </summary>
    private const byte POS_TYPE_H = 0;
    /// <summary>
    /// Offset of the low Byte of the the telegram type
    /// </summary>
    private const byte POS_TYPE_L = 1;
    /// <summary>
    /// Position of the destination id in the raw data
    /// </summary>
    private const byte POS_DES = 2;
    /// <summary>
    /// Position of the source id in the raw data
    /// </summary>
    private const byte POS_SRC = 3;
    /// <summary>
    /// Position of the data length in the raw data
    /// </summary>
    private const byte POS_LEN = 4;
    #endregion

    #region Properties
    /// <summary>
    /// Type of the telegram as raw value
    /// </summary>
    public UInt16 RawType
    {
        get => (UInt16)((Raw[POS_TYPE_H] << 8) + Raw[POS_TYPE_L]);
    }

    /// <summary>
    /// Destination / recipient of the telegram
    /// </summary>
    public byte Destination { get => Raw[POS_DES]; }

    /// <summary>
    /// Source / sender of the telegram
    /// </summary>
    public byte Source { get => Raw[POS_SRC]; }

    /// <summary>
    /// Identifier for the telegram type. Is is a mixture of the telegram type,
    /// the destination and the source.
    /// </summary>
    public UInt16 Id
    {
        get => (UInt16)(Destination << 8 | Source);
    }

    /// <summary>
    /// User Data / specific to the telegram
    /// </summary>
    public byte[] PDU { get; }

    /// <summary>
    /// Received checksum
    /// </summary>
    public byte Checksum { get => Raw[POS_LEN + PDU.Length + 1]; }

    /// <summary>
    /// Complete telegram as raw data
    /// </summary>
    public byte[] Raw { get; }

    /// <summary>
    /// The raw PDU is valid against the received checksum
    /// </summary>
    public bool Valid
    {
        get
        {
            byte calcCheck = Raw[POS_LEN];
            foreach (byte b in PDU)
            {
                calcCheck ^= b;
            }

            log.Trace($"Checksum: read={Checksum.ToString("X2")}, calc={calcCheck.ToString("X2")}");
            return calcCheck == Checksum;
        }
    }

    /// <summary>
    /// Possible types of the telegram
    /// </summary>
    public enum TelegramType
    {
        /// <summary>
        /// Request telegram triggered by bus master to the unit
        /// </summary>
        REQUEST = 0xC55C,
        /// <summary>
        /// Telegram sent as a response to a request from the unit to the bus master
        /// </summary>
        RESPONSE = 0xB66B
    }

    /// <summary>
    /// Type of the telegram
    /// </summary>
    public TelegramType Type { get => (TelegramType)RawType; }

    #endregion

    /// <summary>
    /// Internal constructor for specialized classes. Create empty arrays
    /// </summary>
    protected BaseTelegram()
    {
        PDU = Array.Empty<byte>();
        Raw = Array.Empty<byte>();
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
    }

    /// <summary>
    /// Create a new base telegram based on the given raw data
    /// </summary>
    /// <param name="rawData">raw data of one telegram</param>
    /// <exception cref="ArgumentNullException">Raw data is null.</exception>
    /// <exception cref="ArgumentException">Raw data is too short.</exception>
    /// <exception cref="ArgumentException">Invalid data length in the raw data.</exception>
    /// <exception cref="ArgumentException">Raw data does not contain End tag.</exception>
    public BaseTelegram(byte[] rawData)
    {
        // Basic validation
        ArgumentNullException.ThrowIfNull(rawData);
        if (rawData.Length < MIN_DATA_LEN)
        {
            throw new ArgumentException("Raw data is too short");
        }

        // Copy raw data
        Raw = new byte[rawData.Length];
        Array.Copy(rawData, Raw, Raw.Length);

        // fetch user data
        byte pduLen = rawData[POS_LEN];
        if (pduLen > MAX_DATA_LEN)
        {
            throw new ArgumentException($"Invalid data len {pduLen}. Max supported: {MAX_DATA_LEN}");
        }

        // copy user data to PDU array
        PDU = new byte[pduLen];
        Array.Copy(rawData, POS_LEN + 1, PDU, 0, PDU.Length);

        // Verify checksum
        if (Valid == false)
        {
            log.Warn("Telegram has an invalid checksum");
        }

        // Check end telegram value
        int pos_end_tag = MIN_DATA_LEN - 1 + pduLen;
        if (rawData.Length < pos_end_tag || rawData[pos_end_tag] != END_TELEGRAM)
        {
            log.Error("RawData does not hold Endtag");
            throw new ArgumentException("Raw data does not contain End tag");
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

    /// <summary>
    /// Print detailed information. Starts with Raw Format, followed by parsed content
    /// </summary>
    /// <returns></returns>
    public virtual string ToStringDetailed()
    {
        if (this.GetType() != typeof(BaseTelegram))
        {
            System.Text.StringBuilder hex = new(Raw.Length * 3);

            foreach (byte b in Raw)
                hex.AppendFormat("{0:X2} ", b);

            hex.Append(" -> ");
            hex.Append(ToString());

            return hex.ToString();
        }
        return ToString();
    }

    /// <summary>
    /// Compare two telegrams based on the raw data
    /// </summary>
    /// <param name="other">Second object to compare</param>
    /// <returns></returns>
    public bool Equals(BaseTelegram? other)
    {
        return Array.Equals(this.Raw, other?.Raw);
    }

}
