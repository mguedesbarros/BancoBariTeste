using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebConsumer.Infrastructure.Data.Mappings;
using WebConsumer.Models;

namespace WebConsumer.Infrastructure.Data
{
    public class BariContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public BariContext(DbContextOptions<BariContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseInMemoryDatabase("BariBD");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MessageMap());

            base.OnModelCreating(modelBuilder);
        }
    }
}
