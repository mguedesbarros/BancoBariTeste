using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebConsumer.Models;

namespace WebConsumer.Infrastructure.Repositories
{
    public interface IMessageRepository
    {
        IList<Message> GetAll();
        void Add(Message message);
    }
}
