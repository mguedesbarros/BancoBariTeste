using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebConsumer.Models;

namespace WebConsumer.Infrastructure.Data.Mappings
{
    public class MessageMap : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Message");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Descricao).IsRequired().HasMaxLength(4000).HasColumnType("varchar(4000)");
            builder.Property(x => x.Host).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
            builder.Property(x => x.DataEnvio).IsRequired().HasColumnName("data_envio");
        }
    }
}
