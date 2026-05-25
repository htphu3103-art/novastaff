// src/modules/chat/services/chatApi.ts
import { axiosClient } from '../../../utils/axiosClient';
import { ChannelDto, MemberDto, MessageDto, MessagePageResult } from '../types/chat';

export const chatApi = {
  getChannels: (): Promise<ChannelDto[]> =>
    axiosClient.get('/chat/channels').then((r) => r.data),

  getMessages: (
    channelID: number,
    pageSize = 30,
    beforeMessageID?: number
  ): Promise<MessagePageResult> =>
    axiosClient
      .get(`/chat/channels/${channelID}/messages`, {
        params: { pageSize, beforeMessageID },
      })
      .then((r) => r.data),

  getMembers: (channelID: number): Promise<MemberDto[]> =>
    axiosClient.get(`/chat/channels/${channelID}/members`).then((r) => r.data),

  createChannel: (payload: {
    name: string;
    description?: string;
    memberUserIDs: number[];
  }) => axiosClient.post('/chat/channels', payload).then((r) => r.data),

  createDirect: (targetUserID: number): Promise<ChannelDto> =>
    axiosClient
      .post('/chat/channels/direct', { targetUserID })
      .then((r) => r.data),

  markRead: (channelID: number): Promise<void> =>
    axiosClient
      .post(`/chat/channels/${channelID}/read`)
      .then(() => undefined),

  getDirects: (): Promise<ChannelDto[]> =>
    axiosClient.get('/chat/channels/directs').then((r) => r.data),

  sendMessage: (
    channelID: number,
    payload: { content: string; replyToMessageID?: number }
  ): Promise<MessageDto> =>
    axiosClient
      .post(`/chat/channels/${channelID}/messages`, payload)
      .then((r) => r.data),
  addMembers: (
    channelID: number,
    memberUserIDs: number[]
  ): Promise<{ message: string }> =>
    axiosClient
      .post(`/chat/channels/${channelID}/members`, memberUserIDs)
      .then((r) => r.data),
  getUsersLookup: (): Promise<{ userID: number, fullName: string, initials: string, department: string | null }[]> =>
    axiosClient.get('/chat/users').then((r) => r.data),
};
