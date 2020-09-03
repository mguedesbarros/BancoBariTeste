using EventBus.Infra.EventBus;
using EventBus.Infrastructure.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebConsumer.Infrastructure.Data;
using WebConsumer.Infrastructure.Repositories;
using WebConsumer.Models;

namespace WebConsumer.Background
{
    public class ConsumerService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IMessageRepository _repository; 

        public ConsumerService(ILogger<ConsumerService> logger, IEventBus eventBus, IServiceScopeFactory scopeFactory, IRabbitConnection rabbitConnection, IMessageRepository repository)
        {
            _logger = logger;
            _eventBus = eventBus;
            _scopeFactory = scopeFactory;
            _rabbitConnection = rabbitConnection;
            _repository = repository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            new Timer(ExecuteProcess, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("### Proccess stoping ###");
            _logger.LogInformation($"{DateTime.Now}");
            return Task.CompletedTask;
        }

        private void ExecuteProcess(object state)
        {
            Consumer();
            //var message = _eventBus.Consumer();

            //if (message.Length > 0)
            //{
            //    _logger.LogInformation("### Proccess executing ###");
            //    _logger.LogInformation($"[x] Mensagem recebida [x] \n{message}");

            //    var _message = JsonConvert.DeserializeObject<Message>(message);

            //    //_repository.Add(_message);

            //    using (var scope = _scopeFactory.CreateScope())
            //    {
            //        var dbContext = scope.ServiceProvider.GetRequiredService<BariContext>();
            //        dbContext.Messages.Add(_message);

            //    }
            //}
        }

        private void Consumer()
        {
            var channel = _rabbitConnection.GetModel();

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_rabbitConnection.GetRetryCount(), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    Console.WriteLine(ex.ToString());
                });

            policy.Execute(() =>
            {

                channel.QueueDeclare(queue: _rabbitConnection.GetQueueName(),
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Recebida [x] \n{0} ", message);

                    _repository.Add(JsonConvert.DeserializeObject<Message>(message));
                    //using (var scope = _scopeFactory.CreateScope())
                    //{
                    //    var dbContext = scope.ServiceProvider.GetRequiredService<BariContext>();
                    //    dbContext.Messages.Add(JsonConvert.DeserializeObject<Message>(message));

                    //}

                };
                channel.BasicConsume(queue: _rabbitConnection.GetQueueName(),
                    autoAck: true,
                    consumer: consumer);
            });

        }
    }
}
