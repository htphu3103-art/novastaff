// src/modules/chat/hooks/useChatSignalR.ts
import { useCallback, useEffect, useState } from 'react';
import { signalRService } from '../services/signalRService';
import { chatApi } from '../services/chatApi';
import {
  ChannelDto,
  MessageDto,
  MessagePageResult,
  SendMessageRequest,
} from '../types/chat';

interface UseChatSignalROptions {
  currentUserID: number;
}

export function useChatSignalR({ currentUserID }: UseChatSignalROptions) {
  const [channels, setChannels] = useState<ChannelDto[]>([]);
  const [activeChannelID, setActiveChannelID] = useState<number | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [hasMore, setHasMore] = useState(false);
  const [nextCursor, setNextCursor] = useState<number | undefined>();
  const [onlineUserIDs, setOnlineUserIDs] = useState<Set<number>>(new Set());
  const [typingUsers, setTypingUsers] = useState<Map<number, ReturnType<typeof setTimeout>>>(new Map());
  const [isConnected, setIsConnected] = useState(false);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);

  // ── Connect — signalRService tự lấy token từ localStorage ──

  useEffect(() => {
    signalRService.connect().then(() => setIsConnected(true));
    return () => { signalRService.disconnect(); };
  }, []); // không phụ thuộc getToken nữa

  // ── Load channels ──────────────────────────────────────────

  useEffect(() => {
    chatApi.getChannels().then(setChannels);
  }, []);

  // ── Server → Client events ────────────────────────────────

  useEffect(() => {
    if (!isConnected) return;

    const cleanups = [
      signalRService.onReceiveMessage((msg) => {
        setMessages((prev) =>
          prev.some((m) => m.chatMessageID === msg.chatMessageID)
            ? prev
            : [...prev, msg]
        );
        setChannels((prev) =>
          prev.map((ch) =>
            ch.chatChannelID === msg.chatChannelID
              ? {
                  ...ch,
                  lastMessage: msg,
                  unreadCount:
                    msg.senderUserID !== currentUserID &&
                    ch.chatChannelID !== activeChannelID
                      ? ch.unreadCount + 1
                      : ch.unreadCount,
                }
              : ch
          )
        );
      }),

      signalRService.onMessageDeleted((messageID) => {
        setMessages((prev) =>
          prev.map((m) =>
            m.chatMessageID === messageID
              ? { ...m, isDeleted: true, content: '[Tin nhắn đã bị xoá]' }
              : m
          )
        );
      }),

      signalRService.onReactionUpdated(({ messageID, reaction }) => {
        setMessages((prev) =>
          prev.map((m) => {
            if (m.chatMessageID !== messageID) return m;
            const others = m.reactions.filter((r) => r.emoji !== reaction.emoji);
            return {
              ...m,
              reactions: reaction.count > 0 ? [...others, reaction] : others,
            };
          })
        );
      }),

      signalRService.onUserTyping(({ channelID, userID, isTyping }) => {
        if (channelID !== activeChannelID || userID === currentUserID) return;
        setTypingUsers((prev) => {
          const next = new Map(prev);
          if (isTyping) {
            clearTimeout(next.get(userID));
            const timer = setTimeout(() => {
              setTypingUsers((p) => {
                const m = new Map(p);
                m.delete(userID);
                return m;
              });
            }, 3000);
            next.set(userID, timer);
          } else {
            clearTimeout(next.get(userID));
            next.delete(userID);
          }
          return next;
        });
      }),

      signalRService.onUserOnline((uid) =>
        setOnlineUserIDs((prev) => new Set([...prev, uid]))
      ),

      signalRService.onUserOffline((uid) =>
        setOnlineUserIDs((prev) => {
          const s = new Set(prev);
          s.delete(uid);
          return s;
        })
      ),
    ];

    return () => cleanups.forEach((fn) => fn());
  }, [isConnected, activeChannelID, currentUserID]);

  // ── Switch channel ────────────────────────────────────────

  const switchChannel = useCallback(async (channelID: number) => {
    setActiveChannelID(channelID);
    setMessages([]);
    setHasMore(false);
    setNextCursor(undefined);

    await signalRService.joinChannel(channelID);

    setIsLoadingMessages(true);
    const result: MessagePageResult = await chatApi.getMessages(channelID);
    setMessages(result.messages);
    setHasMore(result.hasMore);
    setNextCursor(result.nextCursor);
    setIsLoadingMessages(false);

    setChannels((prev) =>
      prev.map((ch) =>
        ch.chatChannelID === channelID ? { ...ch, unreadCount: 0 } : ch
      )
    );
  }, []);

  // ── Load more ─────────────────────────────────────────────

  const loadMoreMessages = useCallback(async () => {
    if (!activeChannelID || !hasMore || isLoadingMessages) return;
    setIsLoadingMessages(true);
    const result = await chatApi.getMessages(activeChannelID, 30, nextCursor);
    setMessages((prev) => [...result.messages, ...prev]);
    setHasMore(result.hasMore);
    setNextCursor(result.nextCursor);
    setIsLoadingMessages(false);
  }, [activeChannelID, hasMore, nextCursor, isLoadingMessages]);

  // ── Actions ───────────────────────────────────────────────

  const sendMessage = useCallback(
    async (request: SendMessageRequest) => {
      if (!activeChannelID) return;
      await signalRService.sendMessage(activeChannelID, request);
    },
    [activeChannelID]
  );

  const sendTyping = useCallback(
    (isTyping: boolean) => {
      if (activeChannelID) signalRService.sendTyping(activeChannelID, isTyping);
    },
    [activeChannelID]
  );

  const toggleReaction = useCallback(
    (messageID: number, emoji: string) => {
      if (activeChannelID)
        signalRService.toggleReaction(messageID, activeChannelID, emoji);
    },
    [activeChannelID]
  );

  const deleteMessage = useCallback(
    (messageID: number) => {
      if (activeChannelID)
        signalRService.deleteMessage(messageID, activeChannelID);
    },
    [activeChannelID]
  );

  return {
    channels,
    activeChannelID,
    messages,
    hasMore,
    isLoadingMessages,
    isConnected,
    onlineUserIDs,
    typingUserIDs: [...typingUsers.keys()],
    switchChannel,
    loadMoreMessages,
    sendMessage,
    sendTyping,
    toggleReaction,
    deleteMessage,
    setChannels,
  };
}
