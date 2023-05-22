﻿using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using URBE.Pokemon.API.Logging.Enrichers;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Logging.Sinks;
public class DatabaseSink : ILogEventSink
{
    private static event Action? Exiting;
    private static bool ExitRegistered;
    public static void RegisterExitEvent()
    {
        lock (typeof(DatabaseSink))
            if (ExitRegistered is false)
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                ExitRegistered = true;
            }
    }

    private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        => Exiting?.Invoke();

    private readonly IServiceProvider Services;
    private readonly ConcurrentQueue<LogEvent> Buffer;
    private readonly int Buffering;
    private readonly LogEventLevel MinimumLevel;

    public DatabaseSink(int buffering, LogEventLevel restrictedToMinimumLevel, IServiceProvider services)
    {
        MinimumLevel = restrictedToMinimumLevel;
        Services = services ?? throw new ArgumentNullException(nameof(services));
        if (buffering <= 0)
            throw new ArgumentOutOfRangeException(nameof(buffering), "The buffering index cannot be less than or equal to 0");

        Buffering = buffering;
        Buffer = new();

        BackgroundTaskStore.Add(
            () => Task.Run(async () =>
            {
                await Task.Delay(2000);
                await Upload();
            }),
            true
        );

        Exiting += ProcessExit;
    }

    private void ProcessExit()
    {
        Upload().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent is null || logEvent.Level < MinimumLevel) return;

        Buffer.Enqueue(logEvent);
        if (Buffer.Count >= Buffering - 1)
            BackgroundTaskStore.Add(Upload, false);
    }

    private async Task Upload()
    {
        Debug.Assert(Buffer is not null);
        await BulkWriteToDb();
    }

    private async Task BulkWriteToDb()
    {
        Debug.Assert(Buffer is not null);
        using (var scope = Services.CreateScope())
        using (var context = scope.ServiceProvider.GetRequiredService<UrbeContext>())
        {
            while (Buffer.TryDequeue(out var le))
                WriteToDb(le, context);

            await context.SaveChangesAsync();
        }
    }

    private class PropertyBuffer
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }

    private void WriteToDb(LogEvent logEvent, UrbeContext context)
    {
        if (logEvent is null) return;

        var e = logEvent.Exception;
        var pb = new PropertyBuffer();
        StringWriter sw = new();

        var ev = new ExecutionLogEntry()
        {
            Date = DateTimeOffset.Now,
            Area = logEvent.Properties.TryGetValue("Area", out var area) ? area.ToString() : null,
            ClientName = Program.Settings.ClientName,
            LoggerName = logEvent.Properties.TryGetValue("LoggerName", out var ln) ? ln.ToString() : null,
            LogEventLevel = logEvent.Level,
            Message = logEvent.RenderMessage(),
            ExceptionType = e?.GetType().Name,
            ExceptionMessage = e?.Message,
            ExceptionDumpPath = logEvent.Properties.TryGetValue(ExceptionDumper.ExceptionDumpProperty, out var prop) ? prop.ToString() : null,
            TraceId = logEvent.Properties.TryGetValue("Trace", out var trace) ? trace.ToString() : null,
            JsonProperties = JsonSerializer.Serialize(logEvent.Properties.Select(x =>
            {
                pb.Name = x.Key;
                sw.GetStringBuilder().Clear();
                x.Value.Render(sw);
                pb.Value = sw.GetStringBuilder().ToString();
                return pb;
            }))
        };

        context.ExecutionLog.Add(ev);
    }
}
