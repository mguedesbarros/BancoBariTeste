using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Infra.EventBus;
using Polly.Retry;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Polly;

namespace EventBus.Infrastructure.EventBus
{
    public class RabbitEventBus : IEventBus
    {
        private readonly IRabbitConnection _rabbitConnection;        

        public RabbitEventBus(IRabbitConnection rabbitConnection)
        {
            this._rabbitConnection = rabbitConnection;
        }

        public string Consumer()
        {
            var message = string.Empty;
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
                    message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Recebida [x] \n{0} ", message);
                };
                channel.BasicConsume(queue: _rabbitConnection.GetQueueName(),
                    autoAck: true,
                    consumer: consumer);
            });

            return message;
        }

        public void Publish<T>(T entity)
        {

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
               .Or<SocketException>()
               .WaitAndRetry(_rabbitConnection.GetRetryCount(), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   Console.WriteLine(ex.ToString());
               });

            var json = JsonConvert.SerializeObject(entity);
            var channel = _rabbitConnection.GetModel();
            channel.QueueDeclare(queue: _rabbitConnection.GetQueueName(),
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: _rabbitConnection.GetExchange(),
                    routingKey: _rabbitConnection.GetRoutingKey(),
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(json)
                );
            });
        }
    }
}
