using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.Infrastructure.EventBus
{
    public class RabbitConnection : IRabbitConnection
    {
        private readonly object _lock = new object();
        private IConnection _connection = null;
        private readonly IModel _channel = null;
        private string QueueName = string.Empty;
        private int _retryCount;
        public RabbitConnection(IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            _retryCount = rabbitMqOptions.Value.RetryCount;

            if (_channel == null)
            {
                lock (_lock)
                {
                    if (_channel == null)
                    {

                        var factory = new ConnectionFactory()
                        {
                            HostName = rabbitMqOptions.Value.Hostname,
                            Port = rabbitMqOptions.Value.Port,
                            UserName = rabbitMqOptions.Value.UserName,
                            Password = rabbitMqOptions.Value.Password,
                            AutomaticRecoveryEnabled = true
                        };

                        QueueName = rabbitMqOptions.Value.QueueName;
                        _connection = factory.CreateConnection();
                        _channel = _connection.CreateModel();
                    }
                }
            }
        }
        public string GetExchange() => "";

        public IModel GetModel() => _channel;        

        public string GetRoutingKey() => QueueName;
        
        public string GetQueueName() => QueueName;

        public int GetRetryCount() => _retryCount;
    }
}
