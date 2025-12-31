using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Infrastructure.Messaging;

public static class RabbitMQService
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

