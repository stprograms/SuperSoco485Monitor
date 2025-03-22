using NLog;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CommandLine;
using RS485_Monitor.Utils.Storage;
using System.Runtime.CompilerServices;

const string FILE_EXTENSION = ".ssm";

// Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var sec = config.GetSection("NLog");
LogManager.Configuration = new NLogLoggingConfiguration(sec);
Logger log = LogManager.GetCurrentClassLogger();

Configuration cfg = new(config.GetSection("Monitor"));

// command line parser
string? parseFile = null;
bool replayToSerial = false;
bool groupOutput = false;
bool writeToFile = false;

var result = Parser.Default.ParseArguments<CmdOptions>(args)
    .WithParsed(o =>
    {
        if (o.InputFile != null)
        {
            parseFile = o.InputFile;
        }
        replayToSerial = o.ReplayOnSerial;
        groupOutput = o.GroupOutput;
        writeToFile = o.WriteToFile;
    }
    );

// Check return value of parser result
if (result.Tag == ParserResultType.NotParsed)
{
    // Help text requested, or parsing failed. Exit.
    return;
}

// Configure output
IUserVisualizable printer = groupOutput ? new ConsolePrinter() : new LogPrinter();

// Select execution
if (parseFile != null)
{
    if (replayToSerial)
        await ReplayTraffic(parseFile, cfg.ComPort);
    else
        await ReadLocalFile(parseFile);
}
else
{
    StartMonitor(cfg);
}

/// <summary>
/// Start the serial monitor
/// </summary>
void StartMonitor(Configuration cfg)
{
    // Semaphore for ending the monitor
    Semaphore endMonitor = new(0, 1);

    // Register CTRL+C
    Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, args) =>
    {
        log.Info("Exiting ...");

        // Set cancel to true to prevent default CTRL+C behaviour
        args.Cancel = true;

        // release semaphore
        endMonitor.Release();
    });

    log.Info($"Starting Serial Monitor on port {cfg.ComPort}");

    // create telegram exporter
    TelegramExporter? exporter = null;
    if (writeToFile)
    {
        FileInfo? outputFile = null;
        string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_telegram" + FILE_EXTENSION;

        // Determine full output file path
        if (cfg.OutputDir != null)
        {
            outputFile = new(cfg.OutputDir + "/" + filename);
        }
        else
        {
            outputFile = new(filename);
        }

        // create exporter
        exporter = new(outputFile.OpenWrite());
    }

    // Start serial monitor on the given port
    using (SerialMonitor monitor = new(cfg.ComPort, cfg.WriteRawData))
    {
        if (cfg.OutputDir != null)
        {
            monitor.OutputDir = cfg.OutputDir;
        }
        monitor.TelegramReceived += (o, e) =>
        {
            // Print the received telegram
            printer.PrintTelegram(e.Telegram);
            exporter?.PushTelegram(e.Telegram);
        };

        // Start reading monitor
        try
        {
            monitor.Start();
        }
        catch (IOException ex)
        {
            log.Fatal(ex, "Could not start monitor");
            return;
        }

        // Wait for CTRL+C
        endMonitor.WaitOne();
    }
    log.Info("Monitor stopped");

    // close exporter
    exporter?.Dispose();
}


/// <summary>
/// Read a local file and parse it
/// </summary>
async Task ReadLocalFile(String file)
{
    TelegramParser parser = new();
    TelegramPlayer player = new(cfg.ReplayCycle);

    // Register new telegram handler
    parser.NewTelegram += (sender, evt) =>
    {
        BaseTelegram telegram = ((TelegramParser.TelegramArgs)evt).Telegram;
        //log.Info(telegram);
        if (telegram.Valid)
            player.AddTelegram(telegram);
    };

    // Parse the file
    try
    {
        parser.ParseFile(file);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        log.Fatal(ex, "Could not open file to parse");
        return;
    }

    player.TelegramEmitted += (o, e) =>
    {
        printer.PrintTelegram(e.Telegram);
    };

    // Await that all telegrams where emitted
    await player.ReplayTelegramsAsync();

    // Flush all printer output
    printer.Flush();
}



/// <summary>
/// Read a local file and parse it
/// </summary>
async Task ReplayTraffic(string file, string port)
{
    TelegramParser parser = new();
    TelegramPlayer player = new(cfg.ReplayCycle);
    SerialSimulator sim;

    log.Info($"Starting replay of {file} on port {cfg.ComPort}");

    // Create serial simulator
    try
    {
        sim = new(port);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        log.Fatal(ex, "Couldn't open COM Port");
        return;
    }

    // Register new telegram handler of parser
    parser.NewTelegram += (sender, evt) =>
    {
        BaseTelegram telegram = ((TelegramParser.TelegramArgs)evt).Telegram;

        // Add telegram to player
        if (telegram.Valid)
            player.AddTelegram(telegram);
    };

    // Parse the file
    try
    {
        parser.ParseFile(file);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        log.Fatal(ex, "Could not open file to parse");
        return;
    }
    player.TelegramEmitted += (o, e) =>
    {
        sim.WriteTelegram(e.Telegram);
    };

    // Replay telegrams
    await player.ReplayTelegramsAsync();

    log.Info($"Replay finished");
}
