using EventBus.Infra.EventBus;
using EventBus.Infrastructure.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebConsumer.Infrastructure.Data;
using WebConsumer.Infrastructure.Repositories;

namespace WebConsumer.Infrastructure.IoC
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .Build();

            var rabbitMQConfigurations = config.GetSection("RabbitMq");

            services.Configure<RabbitMqConfiguration>(rabbitMQConfigurations)
            .AddSingleton<IRabbitConnection, RabbitConnection>(sp => {

                var rabbitConnection = sp.GetService<IOptions<RabbitMqConfiguration>>();

                return new RabbitConnection(rabbitConnection);
            })
            .AddDbContext<BariContext>(options =>                
                options.UseInMemoryDatabase("InMemoryBariDB"), ServiceLifetime.Singleton)
            .AddTransient<IMessageRepository, MessageRepository>()
            .AddSingleton<IEventBus, RabbitEventBus>();

            return services;
        }
    }
}
