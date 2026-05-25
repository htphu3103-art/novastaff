using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");
        builder.HasKey(x => x.MessageReactionID);

        // Mỗi user chỉ react 1 emoji type trên 1 message
        builder.HasIndex(x => new { x.ChatMessageID, x.UserID, x.Emoji }).IsUnique();

        builder.Property(x => x.Emoji).HasMaxLength(10);

        builder.HasOne(x => x.Message)
            .WithMany(x => x.Reactions)
            .HasForeignKey(x => x.ChatMessageID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
