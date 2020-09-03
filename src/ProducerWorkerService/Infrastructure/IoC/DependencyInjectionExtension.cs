using EventBus.Infra.EventBus;
using EventBus.Infrastructure.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProducerWorkerService.Infrastructure.IoC
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .Build();

            var rabbitMQConfigurations = builder.GetSection("RabbitMq");

            services.Configure<RabbitMqConfiguration>(rabbitMQConfigurations)
            .AddSingleton<IRabbitConnection, RabbitConnection>(sp => {

                var rabbitConnection = sp.GetService<IOptions<RabbitMqConfiguration>>();

                return new RabbitConnection(rabbitConnection);
            })
            .AddSingleton<IEventBus, RabbitEventBus>();

            return services;
        }
    }
}
