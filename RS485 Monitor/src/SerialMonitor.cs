//

using System.IO.Ports;
using NLog;

/// <summary>
/// Class reading data from the serial interface and parsing using the TelegramParser.
/// </summary>
public class SerialMonitor : IDisposable
{
    #region Private Members
    /// <summary>
    /// Internal class logger
    /// </summary>
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    /// <summary>
    /// Parser for telegrams
    /// </summary>
    private readonly TelegramParser parser;

    /// <summary>
    /// Internal serial port object
    /// </summary>
    private readonly SerialPort port;
    /// <summary>
    /// Optional filestream to raw data
    /// </summary>
    private Stream? rawStream;
    /// <summary>
    /// Export raw data to binary file
    /// </summary>
    private readonly bool writeRawData;
    #endregion

    /// <summary>
    /// Optional directory where raw binaries are stored
    /// </summary>
    public String? OutputDir = null;

    /// <summary>
    /// Monitor is running
    /// </summary>
    public bool Running { get; internal set; }

    /// <summary>
    /// A new response telegram has been received. Will return a
    /// TelegramParser.TelegramArgs argument
    /// </summary>
    public event EventHandler<TelegramParser.TelegramArgs>? TelegramReceived;

    #region Constants
    /// <summary>
    /// Baudrate used for the COMPort
    /// </summary>
    private const int BAUDRATE = 9600;
    /// <summary>
    /// Maximum size of the buffer / chunks
    /// </summary>
    private const int BUFFER_SIZE = 256;
    #endregion

    /// <summary>
    /// Create a new SerialMonitor
    /// </summary>
    /// <param name="comPort">COMPort to open</param>
    /// <param name="writeRawData">Also write data to a raw binary file</param>
    public SerialMonitor(string comPort, bool writeRawData = false)
    {
        // Initialize the comport
        port = new(comPort)
        {
            BaudRate = BAUDRATE
        };
        port.DataReceived += DataReceivedHandler;

        // Set running to false
        Running = false;

        // write the raw data to disk
        this.writeRawData = writeRawData;

        // Initialize the telegram parser
        parser = new();
        parser.NewTelegram += (o, t) =>
        {
            var args = t as TelegramParser.TelegramArgs;

            if (args !=null)
            {
                // Forward the telegram
                TelegramReceived?.Invoke(this, args);
            }
        };
    }

    /// <summary>
    /// Serial Data received
    /// </summary>
    /// <param name="sender">unused</param>
    /// <param name="args">unused</param>
    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs args)
    {
        byte[] buffer;
        while (port.BytesToRead > 0)
        {
            int bytesToRead = port.BytesToRead;
            if (bytesToRead > BUFFER_SIZE)
            {
                bytesToRead = BUFFER_SIZE;
            }
            buffer = new byte[bytesToRead];

            // Read data
            int readBytes = port.Read(buffer, 0, bytesToRead);

            if (readBytes != bytesToRead)
            {
                // should not occure, but adapt the array
                Array.Resize<byte>(ref buffer, readBytes);
            }

            // write to raw file
            rawStream?.Write(buffer, 0, buffer.Length);

            // Forward data to parser
            parser.ParseChunk(buffer);
        }
    }

    /// <summary>
    /// Start the Montoring
    /// </summary>
    public void Start()
    {
        port.Open();

        if (writeRawData)
        {
            // open raw file for output
            String filePath = "";
            if (OutputDir != null)
                filePath = OutputDir + "\\";
            filePath += $"raw_{DateTime.Now.ToString("yyyyMMdd'_'HHmmss")}.bin";

            FileInfo rawFile = new(filePath);
            this.rawStream = rawFile.Create();
        }

        Running = true;
    }

    /// <summary>
    /// Stop the monitor
    /// </summary>
    public void Stop()
    {
        // Close the COMPort
        port.Close();

        // Close the stream
        rawStream?.Close();

        Running = false;
    }

    public void Dispose()
    {
        Stop();
    }
}
