using System.Diagnostics.CodeAnalysis;
using System.Net;
using PokeApiNet;
using Serilog;
using URBE.Pokemon.API.Attributes;
using URBE.Pokemon.API.Filters;
using URBE.Pokemon.API.Logging.Enrichers;
using URBE.Pokemon.API.Logging.Sinks;
using URBE.Pokemon.API.Middleware;
using URBE.Pokemon.API.Models.Database;
using URBE.Pokemon.API.Services;
using URBE.Pokemon.API.Storage;
using URBE.Pokemon.API.Storage.Implementations;
using URBE.Pokemon.API.Workers;

namespace URBE.Pokemon.API;

public static class Program
{
    private static AppSettings? settings;

    public static IServiceProvider Services { get; }
    public static WebApplication App { get; }
    public static Server Server { get; }

    public static AppSettings Settings 
    {
        get => settings!;
        private set => settings = value ?? throw new ArgumentNullException(nameof(value)); 
    }

    static Program()
    {
        DatabaseSink.RegisterExitEvent();
        Helper.CreateAppDataDirectory();

        var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());
        builder.Host.UseSerilog();

        // Add services to the container.
        var services = builder.Services;
        var conf = builder.Configuration;

        services.RegisterUrbeServices();

        services.AddUrbeWorkers();
        services.AddRazorPages();
        services.AddControllers();
        services.AddDbContext<UrbeContext>(o =>
        {
            var dbk = builder.Configuration.GetValue<DatabaseKind>("DatabaseKind");
            switch (dbk)
            {
                case DatabaseKind.SQLite:
                    o.UseSqlite(conf.GetFormattedConnectionString("UrbeContext"));
                    break;
                case DatabaseKind.SQLServer:
                    o.UseSqlServer(conf.GetFormattedConnectionString("UrbeContext"));
                    break;
                default:
                    throw new InvalidDataException($"Unknown database kind {dbk}");
            };
        });
        services.AddSingleton(new PokeApiClient());

        //services.AddScoped<IStorageProvider>(x => new FileSystemStorageProvider("Dynamic"));
        
        builder.Configuration.AddJsonFile("appsettings.secret.json");

        App = builder.Build();
        Services = App.Services;

        LogHelper.DefaultFormat = p => $"[{{Timestamp:yyyy-MM-dd hh:mm:ss.fffffffzzz.fff zzz}} [{{Level:u3}}] (Area: {{Area}}) (Logger: {{LoggerName}}){(string.IsNullOrWhiteSpace(p) ? $" {p}" : "")}]{{NewLine}} > {{Message:lj}}{{NewLine}}{{Exception}}";

        LogHelper.AddEnricher(new ExceptionDumper());
        LogHelper.AddConfigurator(
            (c, f, la, ln, conf) =>
            {
                c.WriteTo.Console(conf.Console, f)
                 .WriteTo.File(conf.FileLocation, conf.File, f)
                 .WriteTo.Sink(new DatabaseSink(20, conf.Database, Services))
                 .MinimumLevel.Is(conf.Minimum);

                //if (OperatingSystem.IsWindows())
                //    c.WriteTo.EventLog(Settings.ClientName, "Application", ".", true, f, null, conf.Syslog);
                if (OperatingSystem.IsLinux())
                    c.WriteTo.LocalSyslog(Settings.ClientName, outputTemplate: f, restrictedToMinimumLevel: conf.Syslog);
            }
        );

        var settings = App.Configuration.GetRequiredSection("Settings").Get<AppSettings>() ?? throw new InvalidDataException("Could not bind AppSettings from Configuration");
        settings.Validate();
        Settings = settings;

        App.Configuration.GetReloadToken().RegisterChangeCallback(x =>
        {
            try
            {
                Log.Debug("Reloading settings");
                var settings = App.Configuration.GetRequiredSection("Settings").Get<AppSettings>() ?? throw new InvalidDataException("Could not bind AppSettings from Configuration");
                settings.Validate();
                Program.Settings = settings;
                Log.Verbose("Succesfully reloaded settings");
            }
            catch(Exception e)
            {
                Log.Error(e, "An error ocurred while reloading AppSettings");
            }
        }, null);

        App.UseMiddleware<ExceptionLogger>();

        // Configure the HTTP request pipeline.
        if (!App.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            App.UseHsts();
        }

        App.UseExceptionHandler("/Error");
        App.UseHttpsRedirection();
        App.UseStaticFiles();

        App.UseRouting();

        App.MapControllers();

        App.MapRazorPages();

        Server = new()
        {
            Id = Id<Server>.New(),
            Name = Settings.ClientName,
            Registered = DateTimeOffset.Now
        };
    }

    private static Task Main()
    {
        return App.RunAsync();
    }
}
