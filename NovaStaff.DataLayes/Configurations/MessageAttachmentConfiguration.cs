using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");
        builder.HasKey(x => x.MessageAttachmentID);

        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.FilePath).HasMaxLength(500);
        builder.Property(x => x.ContentType).HasMaxLength(100);

        builder.HasOne(x => x.Message)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.ChatMessageID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
