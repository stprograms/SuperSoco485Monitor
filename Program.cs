﻿using NLog;
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
foreach (String s in args)
{
    log.Debug(s);
}

Parser.Default.ParseArguments<CmdOptions>(args)
    .WithParsed<CmdOptions>(o =>
    {
        if (o.InputFile != null)
        {
            parseFile = o.InputFile;
        }
    });


// Select execution
if (parseFile != null)
{
    ReadLocalFile(parseFile);
}
else
{
    StartMonitor(cfg);
}


/// <summary>
/// Read a local file and parse it
/// </summary>
void ReadLocalFile(String file)
{
    TelegramParser parser = new();

    // Register new telegram handler 
    parser.NewTelegram += (tel) =>
    {
        /*System.Text.StringBuilder hex = new(tel.Length * 3);

        foreach (byte b in tel)
            hex.AppendFormat("{0:X2} ", b);

        log.Info(hex);*/

        BaseTelegram t = new(tel);
        if (t.Source == 0xAA && t.Destination == 0x5A && t.PDU.Length == 10)
        {
            BatteryStatus bs = new(t);
            log.Info(bs);
        } else{
            log.Info(t);
        }
    };

    // Parse the file
    parser.ParseFile(file);
}


/// <summary>
/// Start the serial monitor
/// </summary>
void StartMonitor(Configuration cfg)
{
    log.Info($"Starting Serial Monitor on port {cfg.ComPort}");

    SerialMonitor monitor = new(cfg.ComPort, cfg.DelayMS);
    if (cfg.OutputDir != null)
    {
        monitor.OutputDir = cfg.OutputDir;
    }

    // Register CTRL+C
    Console.CancelKeyPress += delegate
    {
        log.Info("Exiting");
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
    while (true) { }
}