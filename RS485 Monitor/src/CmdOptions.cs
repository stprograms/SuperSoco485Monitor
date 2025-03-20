using CommandLine;

/// <summary>
/// Class containing the command line options
/// </summary>
public class CmdOptions
{
    [Value(0, MetaName = "InputFile", HelpText = "File to parse")]
    public string? InputFile { get; set; }

    [Option('r', "replay", HelpText = "Replay data on serial port", Default = false)]
    public bool ReplayOnSerial { get; set; }

    [Option('g', "group", HelpText = "Group telegrams in output", Default = false)]
    public bool GroupOutput { get; set; }

    [Option('w', "write-to-file", HelpText = "Write telegrams including timestamp to a file", Default = false)]
    public bool WriteToFile { get; set; }
}
