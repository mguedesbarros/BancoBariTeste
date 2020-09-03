using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebConsumer.Infrastructure.Data;
using WebConsumer.Models;

namespace WebConsumer.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly BariContext _context;
        protected readonly DbSet<Message> dbSet;

        public MessageRepository(BariContext context)
        {
            _context = context;
            dbSet = _context.Set<Message>();
        }

        public void Add(Message message)
        {
            dbSet.Add(message);

            _context.SaveChanges();
        }

        public IList<Message> GetAll()
        {
            return dbSet.ToList();
        }
    }
}
