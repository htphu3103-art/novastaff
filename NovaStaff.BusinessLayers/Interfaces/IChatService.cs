using System.Collections.Generic;
using System.Threading.Tasks;
using NovaStaff.Models.DTOs.Chat;

namespace NovaStaff.BusinessLayers.Interfaces;

public interface IChatService
{
    // Channels
    Task<List<ChannelDto>> GetChannelsForUserAsync(int userID);
    Task<ChannelDto> CreateGroupChannelAsync(int creatorUserID, CreateChannelRequest request);
    Task<ChannelDto> GetOrCreateDirectChannelAsync(int userID, int targetUserID);

    // Messages
    Task<MessagePageResult> GetMessagesAsync(int channelID, int userID, int pageSize = 30, int? beforeMessageID = null);
    Task<MessageDto> SaveMessageAsync(int channelID, int senderUserID, SendMessageRequest request);
    Task<bool> DeleteMessageAsync(int messageID, int requestingUserID);

    // Read tracking
    Task MarkChannelReadAsync(int channelID, int userID);

    // Reactions
    Task<ReactionDto> ToggleReactionAsync(int messageID, int userID, string emoji);

    // Members
    Task<List<MemberDto>> GetChannelMembersAsync(int channelID);
    Task<List<ChatUserLookupDto>> GetChatUsersLookupAsync(int currentUserID);
    Task<bool> AddMembersToChannelAsync(int channelID, int requestingUserID, List<int> newUserIDs);
}

