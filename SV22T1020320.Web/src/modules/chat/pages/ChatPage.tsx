// src/modules/chat/pages/ChatPage.tsx
import React, { useEffect, useRef, useState } from 'react';
import {
  Layout, Avatar, Badge, Input, Button, Typography, Spin,
} from 'antd';
import {
  SearchOutlined, PaperClipOutlined, SmileOutlined,
  SendOutlined, TeamOutlined, SettingOutlined, LoadingOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../../contexts/AuthContext';
import { useChatSignalR } from '../hooks/useChatSignalR';
import { MessageDto } from '../types/chat';

const { Sider, Content } = Layout;
const { Text } = Typography;

// ── Sub-components ──────────────────────────────────────────

const OnlineDot: React.FC<{ online: boolean }> = ({ online }) => (
  <span style={{
    position: 'absolute', bottom: 0, right: 0,
    width: 9, height: 9, borderRadius: '50%',
    background: online ? '#1D9E75' : '#aaa',
    border: '1.5px solid #fff',
  }} />
);

const MessageBubble: React.FC<{
  msg: MessageDto;
  currentUserID: number;
  onReact: (messageID: number, emoji: string) => void;
}> = ({ msg, currentUserID, onReact }) => {
  const isMe = msg.senderUserID === currentUserID;
  return (
    <div style={{
      display: 'flex', flexDirection: isMe ? 'row-reverse' : 'row',
      gap: 10, marginBottom: 16,
    }}>
      <Avatar
        style={{ background: '#E6F1FB', color: '#185FA5', fontWeight: 500, fontSize: 12, flexShrink: 0 }}
        size={34}
      >
        {msg.senderInitials}
      </Avatar>

      <div style={{
        maxWidth: '65%', display: 'flex', flexDirection: 'column',
        alignItems: isMe ? 'flex-end' : 'flex-start',
      }}>
        <div style={{
          display: 'flex', flexDirection: isMe ? 'row-reverse' : 'row',
          alignItems: 'baseline', gap: 6, marginBottom: 3,
        }}>
          <Text strong style={{ fontSize: 12 }}>{msg.senderName}</Text>
          <Text type="secondary" style={{ fontSize: 11 }}>
            {new Date(msg.sentAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
          </Text>
        </div>

        <div style={{
          padding: '8px 12px',
          borderRadius: isMe ? '12px 4px 12px 12px' : '4px 12px 12px 12px',
          fontSize: 13, lineHeight: 1.6,
          background: msg.isDeleted ? '#f5f5f5' : isMe ? '#185FA5' : '#f5f5f5',
          color: msg.isDeleted ? '#aaa' : isMe ? '#fff' : '#222',
          fontStyle: msg.isDeleted ? 'italic' : 'normal',
          border: isMe ? 'none' : '0.5px solid #ebebeb',
        }}>
          {msg.content}
        </div>

        {msg.reactions.length > 0 && (
          <div style={{ display: 'flex', gap: 4, marginTop: 4, flexWrap: 'wrap' }}>
            {msg.reactions.map((r) => (
              <span
                key={r.emoji}
                onClick={() => onReact(msg.chatMessageID, r.emoji)}
                style={{
                  background: r.reactedByMe ? '#E6F1FB' : '#f0f0f0',
                  border: `0.5px solid ${r.reactedByMe ? '#185FA5' : '#e0e0e0'}`,
                  borderRadius: 12, padding: '2px 7px', fontSize: 11, cursor: 'pointer',
                }}
              >
                {r.emoji} {r.count}
              </span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

// ── Main Component ──────────────────────────────────────────

const ChatPage: React.FC = () => {
  const { user } = useAuth();

  const currentUserID: number = user?.userId ?? 0;

  const [searchValue, setSearchValue] = useState('');
  const [inputValue, setInputValue] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const typingTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const {
    channels, activeChannelID, messages, hasMore,
    isLoadingMessages, onlineUserIDs, typingUserIDs,
    switchChannel, loadMoreMessages, sendMessage, sendTyping, toggleReaction,
  } = useChatSignalR({ currentUserID });

  const activeChannel = channels.find((c) => c.chatChannelID === activeChannelID);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async () => {
    const text = inputValue.trim();
    if (!text || !activeChannelID) return;
    setInputValue('');
    sendTyping(false);
    await sendMessage({ content: text });
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) { handleSend(); return; }
    sendTyping(true);
    if (typingTimerRef.current) clearTimeout(typingTimerRef.current);
    typingTimerRef.current = setTimeout(() => sendTyping(false), 1500);
  };

  const groupChannels = channels.filter(
    (c) => c.type === 'Group' && c.name.toLowerCase().includes(searchValue.toLowerCase())
  );
  const dmChannels = channels.filter(
    (c) => c.type === 'Direct' && c.name.toLowerCase().includes(searchValue.toLowerCase())
  );

  return (
    <Layout style={{ background: '#fff', borderRadius: 10, border: '0.5px solid #ebebeb', flex: 1, height: '100%' }}>

      {/* ── Sidebar ─────────────────────────────────────── */}
      <Sider width={260} theme="light" style={{ background: '#fafafa', borderRight: '0.5px solid #ebebeb' }}>
        <div style={{ padding: '14px 16px', borderBottom: '0.5px solid #ebebeb' }}>
          <Text strong style={{ fontSize: 13, color: '#666', letterSpacing: 0.5, textTransform: 'uppercase' }}>
            NovaStaff Workspace
          </Text>
        </div>

        <div style={{ padding: '10px 12px', borderBottom: '0.5px solid #ebebeb' }}>
          <Input
            prefix={<SearchOutlined style={{ color: '#bbb', fontSize: 12 }} />}
            placeholder="Tìm kiếm..."
            size="small"
            value={searchValue}
            onChange={(e) => setSearchValue(e.target.value)}
            style={{ borderRadius: 6, fontSize: 12 }}
          />
        </div>

        {/* Group channels */}
        <div style={{ padding: '10px 14px 4px', fontSize: 11, color: '#aaa', letterSpacing: 0.6, textTransform: 'uppercase' }}>
          Kênh
        </div>
        {groupChannels.map((ch) => (
          <div
            key={ch.chatChannelID}
            onClick={() => switchChannel(ch.chatChannelID)}
            style={{
              display: 'flex', alignItems: 'center', gap: 10,
              padding: '8px 14px', cursor: 'pointer', borderRadius: 6, margin: '1px 6px',
              background: activeChannelID === ch.chatChannelID ? '#fff' : 'transparent',
              boxShadow: activeChannelID === ch.chatChannelID ? '0 0 0 0.5px #ebebeb' : 'none',
              transition: 'all 0.15s',
            }}
          >
            <div style={{
              width: 30, height: 30, borderRadius: 8, background: '#E6F1FB', color: '#185FA5',
              display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 500, fontSize: 13, flexShrink: 0,
            }}>
              #
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <Text strong style={{ fontSize: 13, display: 'block' }}>{ch.name}</Text>
              <Text type="secondary" ellipsis style={{ fontSize: 11, width: 130 }}>
                {ch.lastMessage?.content ?? ''}
              </Text>
            </div>
            {ch.unreadCount > 0 && <Badge count={ch.unreadCount} size="small" />}
          </div>
        ))}

        {/* DMs */}
        <div style={{ padding: '12px 14px 4px', fontSize: 11, color: '#aaa', letterSpacing: 0.6, textTransform: 'uppercase' }}>
          Tin nhắn trực tiếp
        </div>
        {dmChannels.map((dm) => {
          const isActiveDM = activeChannelID === dm.chatChannelID;
          const unreadCount = Math.max(0, dm.unreadCount ?? 0);
          return (
            <div
              key={dm.chatChannelID}
              onClick={() => switchChannel(dm.chatChannelID)}
              style={{
                display: 'flex', alignItems: 'center', gap: 8,
                padding: '7px 14px', cursor: 'pointer', borderRadius: 6, margin: '1px 6px',
                background: isActiveDM ? '#fff' : 'transparent',
                boxShadow: isActiveDM ? '0 0 0 0.5px #ebebeb' : 'none',
                transition: 'all 0.15s',
              }}
            >
              <Badge dot={unreadCount > 0} offset={[-1, 3]}>
                <div style={{ position: 'relative', flexShrink: 0 }}>
                  <Avatar size={28} style={{ background: '#E6F1FB', color: '#185FA5', fontSize: 11, fontWeight: 500 }}>
                    {dm.name.slice(0, 2).toUpperCase()}
                  </Avatar>
                  {/* DM: cần map targetUserID — hiện để placeholder, sẽ fix sau khi có UserProfileDto */}
                  <OnlineDot online={false} />
                </div>
              </Badge>
              <div style={{ flex: 1, minWidth: 0 }}>
                <Text strong style={{ fontSize: 13, display: 'block' }}>{dm.name}</Text>
                <Text type="secondary" ellipsis style={{ fontSize: 11, width: 130 }}>
                  {dm.lastMessage?.content ?? ''}
                </Text>
              </div>
              {unreadCount > 0 && <Badge count={unreadCount} size="small" />}
            </div>
          );
        })}
      </Sider>

      {/* ── Chat area ────────────────────────────────────── */}
      <Content style={{ display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>

        {/* Header */}
        <div style={{
          padding: '12px 18px', borderBottom: '0.5px solid #ebebeb',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between', background: '#fff', flexShrink: 0,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              width: 30, height: 30, borderRadius: 8, background: '#E6F1FB', color: '#185FA5',
              display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 500, fontSize: 14,
            }}>
              {activeChannel?.type === 'Direct' ? '@' : '#'}
            </div>
            <div>
              <Text strong style={{ fontSize: 15, display: 'block' }}>
                {activeChannel?.name ?? 'Chọn kênh để bắt đầu'}
              </Text>
              {activeChannel?.description && (
                <Text type="secondary" style={{ fontSize: 12 }}>{activeChannel.description}</Text>
              )}
            </div>
          </div>
          <div style={{ display: 'flex', gap: 6 }}>
            <Button icon={<SearchOutlined />} size="small" />
            <Button icon={<TeamOutlined />} size="small" />
            <Button icon={<SettingOutlined />} size="small" />
          </div>
        </div>

        {/* Messages */}
        <div
          style={{ flex: 1, padding: '16px 18px', overflowY: 'auto', background: '#fff' }}
          onScroll={(e) => {
            if ((e.target as HTMLDivElement).scrollTop === 0 && hasMore && !isLoadingMessages)
              loadMoreMessages();
          }}
        >
          {isLoadingMessages && (
            <div style={{ textAlign: 'center', padding: '12px 0' }}>
              <Spin indicator={<LoadingOutlined spin />} size="small" />
            </div>
          )}

          {messages.length === 0 && !isLoadingMessages && activeChannelID && (
            <div style={{ textAlign: 'center', color: '#bbb', fontSize: 13, marginTop: 40 }}>
              Chưa có tin nhắn nào. Hãy bắt đầu cuộc trò chuyện! 👋
            </div>
          )}

          {messages.map((msg) => (
            <MessageBubble
              key={msg.chatMessageID}
              msg={msg}
              currentUserID={currentUserID}
              onReact={toggleReaction}
            />
          ))}
          <div ref={messagesEndRef} />
        </div>

        {/* Input */}
        <div style={{ padding: '12px 18px', borderTop: '0.5px solid #ebebeb', background: '#fff', flexShrink: 0 }}>
          <div style={{
            display: 'flex', alignItems: 'center', gap: 6,
            border: '0.5px solid #d9d9d9', borderRadius: 10, padding: '7px 10px', background: '#fafafa',
          }}>
            <Button icon={<PaperClipOutlined />} type="text" size="small" style={{ color: '#aaa' }} />
            <Input
              variant="borderless"
              placeholder={
                activeChannelID
                  ? `Nhắn tin tới ${activeChannel?.type === 'Direct' ? '' : '#'}${activeChannel?.name ?? ''}...` 
                  : 'Chọn kênh để bắt đầu...'
              }
              style={{ flex: 1, fontSize: 13, background: 'transparent' }}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={!activeChannelID}
            />
            <Button icon={<SmileOutlined />} type="text" size="small" style={{ color: '#aaa' }} />
            <Button
              type="primary"
              icon={<SendOutlined />}
              size="small"
              onClick={handleSend}
              disabled={!activeChannelID || !inputValue.trim()}
              style={{ borderRadius: 6 }}
            >
              Gửi
            </Button>
          </div>

          {typingUserIDs.length > 0 && (
            <Text type="secondary" style={{ fontSize: 11, marginTop: 5, display: 'block', paddingLeft: 4 }}>
              {typingUserIDs.length === 1 ? 'Có người đang nhập...' : `${typingUserIDs.length} người đang nhập...`}
            </Text>
          )}
        </div>
      </Content>
    </Layout>
  );
};

export default ChatPage;
