using NLog;

/// <summary>
/// Base content of a telegram
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
    public byte[] Start { get; }

    /// <summary>
    /// Source of the telegram
    /// </summary>
    public byte Source { get; }

    /// <summary>
    /// Destination of the telegram
    /// </summary>
    public byte Destination { get; }

    /// <summary>
    /// Telegram Data
    /// </summary>
    public byte[] PDU { get; }

    /// <summary>
    /// Received checksum
    /// </summary>
    public byte Checksum { get; }

    public byte[] Raw { get; }
    
    /// <summary>
    /// Are the raw data valid against the Checksum
    /// </summary>
    public bool IsValid { get; }
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

    private const byte POS_SRC = 2;
    private const byte POS_DES = 3;
    private const byte POS_LEN = 4;
    #endregion

    protected BaseTelegram()
    {
        Start = new byte[2];
        PDU = new byte[1];
        Raw = new byte[1];
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="c">Object to copy from</param>
    protected BaseTelegram(BaseTelegram c)
    : this(c.Raw)
    {

    }

    public BaseTelegram(byte[] rawData)
    {
        Start = new byte[2];
        Array.Copy(rawData, Start, Start.Length);
        Source = rawData[POS_SRC];
        Destination = rawData[POS_DES];

        // Read data
        byte dataLen = rawData[POS_LEN];
        if (dataLen > MAX_DATA_LEN)
        {
            throw new ArgumentException("Invalid data len");
        }
        PDU = new byte[dataLen];
        Array.Copy(rawData, POS_LEN + 1, PDU, 0, PDU.Length);

        // Checksum
        Checksum = rawData[POS_LEN + dataLen + 1];

        // Verify checksum
        byte calcCheck = dataLen;
        foreach (byte b in PDU)
        {
            calcCheck ^= b;
        }
        log.Debug($"Checksum: read={Checksum.ToString("X2")}, calc={calcCheck.ToString("X2")}");
        IsValid = (calcCheck == Checksum);

        // Check end telegram
        try
        {
            if (rawData[5 + 1 + dataLen] != END_TELEGRAM)
            {
                log.Warn("RawData does not hold Endtag");
            }
        }
        catch (IndexOutOfRangeException)
        {
            log.Warn("Rawdata does not contain End tag");
        }

        // Copy raw data
        Raw = new byte[rawData.Length];
        Array.Copy(rawData, Raw, Raw.Length);
    }


    public override string ToString()
    {
        System.Text.StringBuilder hex = new(Raw.Length * 3);

        foreach (byte b in Raw)
            hex.AppendFormat("{0:X2} ", b);

        return hex.ToString();
    }
}