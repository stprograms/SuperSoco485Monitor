using Microsoft.Extensions.Configuration;

public class Configuration
{
    private String comPort;
    private bool? writeRawData;

    private String? outputDir = null;

    public String ComPort { get => comPort; }
    public bool WriteRawData
    {
        get => (writeRawData != null) ? (bool)writeRawData : false;
    }

    public String? OutputDir { get => outputDir; }

    public Configuration(IConfigurationSection? sec)
    {
        if (sec == null)
        {
            throw new ArgumentNullException("Expecting a valid section");

        }
        if (sec["port"] == null)
        {
            throw new ArgumentNullException("Missing Configuration option <port>");
        }
        else
        {
            comPort = sec["port"]!;
        }
        if (sec["writeRawData"] == null)
        {
            writeRawData = null;
        }
        else
        {
            writeRawData = bool.Parse(sec["writeRawData"]!);
        }

        if (sec["outputDir"] != null)
        {
            outputDir = sec["outputDir"];
        }
    }
}