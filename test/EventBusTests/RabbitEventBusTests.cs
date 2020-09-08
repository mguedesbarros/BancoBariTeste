using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;
using RabbitMQ.Client;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBusTests.Model;
using System.Threading;

namespace RabbitMqTest.RabbitEventBusTests
{
    [TestClass()]
    public class RabbitEventBusTests
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _queueName;
        private readonly Message _messageSend;

        public RabbitEventBusTests()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();

            var rabbitMQConfigurations = builder.GetSection("RabbitMq");

            _queueName = rabbitMQConfigurations["QueueName"];

            _connectionFactory = new ConnectionFactory()
            {
                HostName = rabbitMQConfigurations["Hostname"],
                Port = Convert.ToInt32(rabbitMQConfigurations["Port"]),
                UserName = rabbitMQConfigurations["UserName"],
                Password = rabbitMQConfigurations["Password"],
                AutomaticRecoveryEnabled = true
            };

            _messageSend = new Message(Guid.NewGuid(), "HostTest", "Mensagem Teste", DateTime.Now);
        }

        [TestMethod()]
        public void ProducerMessage()
        {
            uint messageCount = 0;
            
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonConvert.SerializeObject(_messageSend);

                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: Encoding.UTF8.GetBytes(json));
                Thread.Sleep(1500);
                messageCount = channel.MessageCount(_queueName);
            }

            NUnit.Framework.Assert.That(messageCount, Is.EqualTo(1));
        }

        [TestMethod()]
        public void ConsumerAllMessage()
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var messageCount = channel.MessageCount(_queueName);

                for (int i = 0; i < messageCount; i++)
                {
                    var message = channel.BasicGet(_queueName, autoAck: true);

                    var body = message.Body.ToArray();
                    var messageBody = Encoding.UTF8.GetString(body);
                    var returnMessage = JsonConvert.DeserializeObject<Message>(messageBody);

                    NUnit.Framework.Assert.That(returnMessage, Is.Not.Null);

                    NUnit.Framework.Assert.That(returnMessage.Descricao, Is.EqualTo(_messageSend.Descricao));
                }

                messageCount = channel.MessageCount(_queueName);
                NUnit.Framework.Assert.That(messageCount, Is.EqualTo(0));
            }            
        }

        [TestMethod()]
        public void ProducerConsumerTest()
        {           

            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var json = JsonConvert.SerializeObject(_messageSend);

                channel.QueueDeclare(queue: _queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: "",
                    routingKey: _queueName,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(json)
                );

                Task.Delay(10000);

                var _message = channel.BasicGet(_queueName, false);

                var body = _message.Body.ToArray();
                var messageBody = Encoding.UTF8.GetString(body);
                var returnMessage = JsonConvert.DeserializeObject<Message>(messageBody);

                NUnit.Framework.Assert.That(returnMessage, Is.Not.Null);

                NUnit.Framework.Assert.That(returnMessage.Descricao, Is.EqualTo(_messageSend.Descricao));

                channel.BasicAck(_message.DeliveryTag, multiple: false);
            }
        }       

    }
}