using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URBE.Pokemon.API.Workers;

namespace URBE.Pokemon.API.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RegisterUrbeServiceAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }
    public Type? ServiceType { get; }

    public RegisterUrbeServiceAttribute(ServiceLifetime lifetime, Type? serviceType = null)
    {
        Lifetime = lifetime;
        ServiceType = serviceType;
    }
}

public static class UrbeServiceExtensions
{
    public static void AddUrbeWorkers(this IServiceCollection services)
    {
        foreach (var (type, attr) in AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .Select(x => (Type: x, Attr: x.GetCustomAttribute<RegisterUrbeWorkerAttribute>()))
                            .Where(x => x.Attr != null))
        {
            if (type.IsAssignableTo(typeof(IHostedService)) is false)
                throw new InvalidDataException("Classes decorated with RegisterUrbeWorkerAttribute must implement IHostedService");

            services.TryAddEnumerable(
                new ServiceDescriptor(
                    typeof(IHostedService),
                    type,
                    ServiceLifetime.Singleton
                )
            );
        }
    }

    public static void RegisterUrbeServices(this IServiceCollection services)
    {
        foreach (var serv in AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .Select(x => (Type: x, Attr: x.GetCustomAttribute<RegisterUrbeServiceAttribute>()))
                            .Where(x => x.Attr != null))
            services.Add(
                new ServiceDescriptor(
                    serv.Attr!.ServiceType ?? serv.Type,
                    serv.Type,
                    serv.Attr!.Lifetime
                )
            );
    }
}
