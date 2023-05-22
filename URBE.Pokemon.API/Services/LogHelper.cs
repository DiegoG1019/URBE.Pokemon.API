﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace URBE.Pokemon.API.Services;

public readonly record struct LogProperty(string Name, object? Value, bool Destructure = false);

public readonly record struct LogConfig(LogEventLevel Console, LogEventLevel File, LogEventLevel Database, LogEventLevel Syslog, string FileLocation)
{
    public LogEventLevel Minimum => Min(Console, Min(File, Min(Database, Syslog)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static LogEventLevel Min(LogEventLevel a, LogEventLevel b)
        => a > b ? b : a;
}

public static class LogHelper
{
    public delegate void LoggerConfiguratorDelegate(
        LoggerConfiguration configuratiom, 
        string Format,
        string? LoggerArea,
        string? LoggerName,
        LogConfig Config
    );

    private static readonly object Sync = new();
    private static readonly HashSet<Func<ILogEventEnricher>> Enrichers = new();
    private static readonly HashSet<LoggerConfiguratorDelegate> Configurators = new();

    private static readonly Dictionary<string, LogConfig> Configs;

    static LogHelper()
    {
        Configs = Program.App.Configuration.GetRequiredSection("LogSettings").Get<Dictionary<string, LogConfig>>()
            ?? throw new InvalidDataException("Could not bind LogSettings");
        if (Configs.ContainsKey("Default") is false)
            throw new InvalidDataException("LogConfigs MUST contain a \"Default\" configuration entry");
    }

    private static bool IsFrozen = false;

    private static Func<string?, string>? defaultFormat;

    [MemberNotNull(nameof(defaultFormat))]
    public static Func<string?, string> DefaultFormat
    {
        get => defaultFormat ?? throw new InvalidOperationException("DefaultFormat has not been set");
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            ThrowIfFrozen();
            defaultFormat = value;
        }
    }

    [ThreadStatic]
    private static ILogEventEnricher[]? EnricherBuffer;

    public static ILogger CreateLogger(string? logArea = null, string? loggerName = null, string? propertyFormat = null, params LogProperty[]? properties)
    {
        lock (Sync)
            IsFrozen = true;

        EnricherBuffer ??= new ILogEventEnricher[1];
        var c = new LoggerConfiguration();

        LogConfig logConfig = logArea is null ? Configs["Default"] : Configs.TryGetValue(logArea, out var v) ? v : Configs["Default"];

        foreach (var conf in Configurators)
            conf(c, DefaultFormat(propertyFormat), logArea, loggerName, logConfig);

        foreach (var enr in Enrichers)
        {
            EnricherBuffer[0] = enr();
            c.Enrich.With(EnricherBuffer);
        }

        if (logArea is not null)
            c.Enrich.WithProperty("Area", logArea);

        if (loggerName is not null)
            c.Enrich.WithProperty("LoggerName", loggerName);

        if (properties is not null)
            foreach (var prop in properties)
                c.Enrich.WithProperty(prop.Name, prop.Value!, prop.Destructure);

        return c.CreateLogger();
    }

    public static void AddConfigurator(LoggerConfiguratorDelegate configurator)
    {
        lock (Sync)
        {
            ThrowIfFrozen();
            Configurators.Add(configurator);
        }
    }

    public static void AddEnricher(ILogEventEnricher enricher)
        => AddEnricher(() => enricher);

    public static void AddEnricher(Func<ILogEventEnricher> enricher)
    {
        lock (Sync)
        {
            ThrowIfFrozen();
            Enrichers.Add(enricher);
        }
    }

    private static void ThrowIfFrozen()
    {
        lock (Sync)
            if (IsFrozen)
                throw new InvalidOperationException("Cannot add enrichers or configurators once the LogHelper has been frozen by being used at least once");
    }
}
