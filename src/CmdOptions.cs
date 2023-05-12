using CommandLine;

/// <summary>
/// Class containing the command line options
/// </summary>
public class CmdOptions
{
    [Value(0, MetaName = "InputFile", HelpText = "File to parse")]
    public String? InputFile { get; set; }

    [Option('r', "replay", HelpText = "Replay data on serial port", Default = false)]
    public bool replayOnSerial { get; set; }

    [Option('g', "group", HelpText = "Group telegrams in output", Default = false)]
    public bool groupOutput { get; set; }
}