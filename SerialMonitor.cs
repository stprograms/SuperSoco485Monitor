//

using System.IO.Ports;
using NLog;
using System.Diagnostics;
using System.Text;

public class SerialMonitor
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    private const int BAUDRATE = 9600;
    private Stopwatch watch = new();
    private readonly uint delayMS;
    private SerialPort port;
    private Timer timer;
    private Stream? rawStream;

    public String? OutputDir = null;


    const uint DEFAULT_DELAY_MS = 10;

    public SerialMonitor(string comPort, uint? delayMS = null)
    {
        if (delayMS != null)
        {
            this.delayMS = (uint)delayMS;
        }
        else
        {
            this.delayMS = DEFAULT_DELAY_MS;
        }

        // Initialize the comport
        port = new(comPort);

        // initilize timer
        timer = new(readSerial, this, System.Threading.Timeout.Infinite, this.delayMS);
    }

    /// <summary>
    /// Start the Montoring
    /// </summary>
    public void Start()
    {
        port.BaudRate = BAUDRATE;

        port.Open();
        // Start watch

        watch.Start();
        timer.Change(0, this.delayMS);

        // open raw file for output
        String filePath = "";
        if (OutputDir != null)
            filePath = OutputDir + "\\";
        filePath += $"raw_{new DateTime().ToFileTime()}.bin";

        FileInfo rawFile = new(filePath);
        this.rawStream = rawFile.Create();
    }

    /// <summary>
    /// Stop the monitor
    /// </summary>
    public void Stop()
    {
        watch.Stop();
        port.Close();
        timer.Change(System.Threading.Timeout.Infinite,
            System.Threading.Timeout.Infinite);

        if (this.rawStream != null)
        {
            this.rawStream.Close();
        }
    }

    /// <summary>
    /// Periodically read the data from serial and print to nlog
    /// </summary>
    /// <param name="stateInfo"></param>
    /// <exception cref="NullReferenceException"></exception>
    private static void readSerial(Object? stateInfo)
    {
        SerialMonitor? monitor = stateInfo as SerialMonitor;
        if (monitor == null) throw new NullReferenceException();

        if (monitor.port.IsOpen == false)
            return;

        if (monitor.port.BytesToRead > 0)
        {
            StringBuilder output = new();
            output.Append(monitor.watch.ElapsedMilliseconds);
            output.Append(":");

            for (int i = 0; i < monitor.port.BytesToRead; ++i)
            {
                byte b = (byte)monitor.port.ReadByte();
                output.AppendFormat(" {0:2X}", b);

                if (monitor.rawStream != null)
                {
                    monitor.rawStream.WriteByte(b);
                }
            }
            output.AppendLine();

            // Print to nlog
            log.Info(output.ToString());
        }
    }
}