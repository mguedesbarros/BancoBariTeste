using System;
using System.Collections.Generic;
using System.Text;

namespace ProducerWorkerService.Models
{
    public class Message 
    {
        public Message(string host, string descricao)
        {
            Id = Guid.NewGuid();
            Host = host;
            Descricao = descricao;
            DataEnvio = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string Host { get; private set; }
        public string Descricao { get; private set; }
        public DateTime DataEnvio { get; private set; }
    }
}
