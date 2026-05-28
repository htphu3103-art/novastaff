using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(x => x.ChatMessageID);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasOne(x => x.Channel)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ChatChannelID)
            .OnDelete(DeleteBehavior.Cascade);

        // Dùng Restrict để tránh multiple cascade paths trên PostgreSQL
        builder.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderUserID)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing reply
        builder.HasOne(x => x.ReplyToMessage)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ReplyToMessageID)
            .OnDelete(DeleteBehavior.Restrict);

        // Index để query history nhanh
        builder.HasIndex(x => new { x.ChatChannelID, x.CreatedDate });
    }
}
