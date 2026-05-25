using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class ChatMemberConfiguration : IEntityTypeConfiguration<ChatMember>
{
    public void Configure(EntityTypeBuilder<ChatMember> builder)
    {
        builder.ToTable("ChatMembers");
        builder.HasKey(x => x.ChatMemberID);

        // Mỗi user chỉ join 1 lần trong 1 channel
        builder.HasIndex(x => new { x.ChatChannelID, x.UserID }).IsUnique();

        builder.HasOne(x => x.Channel)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.ChatChannelID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
