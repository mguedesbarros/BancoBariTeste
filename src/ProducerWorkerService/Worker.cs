using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBus.Infra.EventBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProducerWorkerService.Models;

namespace ProducerWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEventBus _eventBus;

        public Worker(ILogger<Worker> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = new Message($"{Environment.MachineName}", "Ola mundo!");

                _logger.LogInformation($"[x] Mensagem enviada [x] \n {JsonConvert.SerializeObject(message)}");

                _eventBus.Publish<Message>(message);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
