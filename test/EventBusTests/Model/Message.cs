using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBusTests.Model
{
    public class Message
    {
        public Message(Guid id, string host, string descricao, DateTime dataEnvio)
        {
            Id = id;
            Host = host;
            Descricao = descricao;
            DataEnvio = dataEnvio;
        }

        public Guid Id { get; private set; }
        public string Host { get; private set; }
        public string Descricao { get; private set; }
        [JsonProperty("data_envio")]
        public DateTime DataEnvio { get; private set; }
    }
}
