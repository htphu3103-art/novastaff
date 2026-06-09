// types/chat.ts

export interface ChannelDto {
  chatChannelID: number;
  name: string;
  description?: string;
  type: 'Group' | 'Direct';
  unreadCount: number;
  lastMessage?: MessageDto;
  /** Chỉ có với DM: userID của người dùng còn lại trong cuộc trò chuyện */
  targetUserID?: number;
  /** DM: true khi người dùng còn lại đã bị xóa hoặc bị vô hiệu hóa */
  isDeactivated?: boolean;
}

export interface MessageDto {
  chatMessageID: number;
  chatChannelID: number;
  senderUserID: number;
  senderName: string;
  senderInitials: string;
  content: string;
  type: 'Text' | 'File' | 'Image' | 'System';
  replyToMessageID?: number;
  isDeleted: boolean;
  sentAt: string; // ISO string
  reactions: ReactionDto[];
  attachments: AttachmentDto[];
}

export interface ReactionDto {
  emoji: string;
  count: number;
  reactedByMe: boolean;
}

export interface AttachmentDto {
  messageAttachmentID: number;
  fileName: string;
  url: string;
  contentType: string;
  fileSize: number;
}

export interface MemberDto {
  userID: number;
  fullName: string;
  initials: string;
  isOnline: boolean;
}

export interface MessagePageResult {
  messages: MessageDto[];
  hasMore: boolean;
  nextCursor?: number;
}

export interface SendMessageRequest {
  content: string;
  replyToMessageID?: number;
}

export interface TypingEvent {
  channelID: number;
  userID: number;
  isTyping: boolean;
}
