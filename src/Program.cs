using NLog;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CommandLine;

// Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var sec = config.GetSection("NLog");
LogManager.Configuration = new NLogLoggingConfiguration(sec);
NLog.Logger log = LogManager.GetCurrentClassLogger();

Configuration cfg = new(config.GetSection("Monitor"));

// Commandline parser
String? parseFile = null;
bool replayToSerial = false;

var result = Parser.Default.ParseArguments<CmdOptions>(args)
    .WithParsed<CmdOptions>(o =>
    {
        if (o.InputFile != null)
        {
            parseFile = o.InputFile;
        }
        replayToSerial = o.replayOnSerial;
    }
    );

// Check return value of parser result
if (result.Tag == ParserResultType.NotParsed)
{
    // Help text requested, or parsing failed. Exit.
    return;
}

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
/// Read a local file and parse it
/// </summary>
async Task ReadLocalFile(String file)
{
    TelegramParser parser = new();
    ConsolePrinter printer = new();
    TelegramPlayer player = new(cfg.ReplayCycle);

    // Register new telegram handler 
    parser.NewTelegram += (sender, evt) =>
    {
        BaseTelegram telegram = ((TelegramParser.TelegramArgs)evt).Telegram;
        //log.Info(telegram);
        if (telegram.Valid) //printer.PrintTelegram(telegram);
            player.AddTelegram(telegram);
    };

    // Parse the file
    parser.ParseFile(file);

    player.TelegramEmitted += (o, e) =>
    {
        printer.PrintTelegram(e.Telegram);
    };

    await player.ReplayTelegramsAsync();
    printer.FlushAndStopTimer();
}


/// <summary>
/// Start the serial monitor
/// </summary>
void StartMonitor(Configuration cfg)
{
    log.Info($"Starting Serial Monitor on port {cfg.ComPort}");

    SerialMonitor monitor = new(cfg.ComPort, cfg.WriteRawData);
    if (cfg.OutputDir != null)
    {
        monitor.OutputDir = cfg.OutputDir;
    }

    // Register CTRL+C
    Console.CancelKeyPress += delegate
    {
        log.Info("Exiting ...");
        monitor.Stop();
    };

    // Start reading monitor
    try
    {
        monitor.Start();
    }
    catch (System.IO.IOException ex)
    {
        log.Fatal(ex, "Could not start monitor");
        return;
    }

    // Endless loop
    while (true)
    {
        Thread.Sleep(10);
    }
}


/// <summary>
/// Read a local file and parse it
/// </summary>
async Task ReplayTraffic(string file, string port)
{
    TelegramParser parser = new();
    TelegramPlayer player = new(cfg.ReplayCycle);
    SerialSimulator sim;

    try
    {
        sim = new(port);
    }
    catch (System.IO.FileNotFoundException ex)
    {
        log.Fatal(ex, "Couldn't create simulator");
        return;
    }

    // Register new telegram handler 
    parser.NewTelegram += (sender, evt) =>
    {
        BaseTelegram telegram = ((TelegramParser.TelegramArgs)evt).Telegram;

        // Add telegram to player
        if (telegram.Valid)
            player.AddTelegram(telegram);
    };

    // Parse the file
    parser.ParseFile(file);
    player.TelegramEmitted += (o, e) =>
    {
        sim.WriteTelegram(e.Telegram);
    };

    // Replay telegrams
    await player.ReplayTelegramsAsync();
}
