/// <summary>
/// Writes telegrams to the serial port
/// </summary>
public class SerialSimulator
{
    /// <summary>
    /// COM port to use
    /// </summary>
    private readonly System.IO.Ports.SerialPort comPort;
    /// <summary>
    /// Baudrate used
    /// </summary>
    public const int BAUDRATE = 9600;

    /// <summary>
    /// Create new SerialSimulator
    /// </summary>
    /// <param name="port">name of the COM port to use</param>
    public SerialSimulator(string port)
    {
        comPort = new System.IO.Ports.SerialPort(port) { BaudRate = BAUDRATE };
        comPort.Open();
    }

    ~SerialSimulator()
    {
        comPort.Close();
    }

    /// <summary>
    /// Write the given telegram to the serial port
    /// </summary>
    /// <param name="telegram">telegram to write</param>
    public void WriteTelegram(BaseTelegram telegram)
    {
        comPort.Write(telegram.Raw, 0, telegram.Raw.Length);
    }
}