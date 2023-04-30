using Microsoft.Extensions.Configuration;

public class Configuration
{
    private String comPort;
    private uint? delayMS;

    private String? outputDir = null;

    public String ComPort { get => comPort; }
    public uint? DelayMS { get => delayMS; }

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
        if (sec["delayMS"] == null) 
        {
            delayMS = null;
        }
        else
        {
            delayMS = uint.Parse(sec["delayMS"]!);
        }

        if (sec["outputDir"] != null)
        {
            outputDir = sec["outputDir"];
        }
    }
}