// src/modules/chat/services/signalRService.ts
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { MessageDto, ReactionDto, SendMessageRequest, TypingEvent } from '../types/chat';

type EventHandler<T = void> = (data: T) => void;

class SignalRService {
  public activeChannelId: number | null = null;
  private connection: HubConnection | null = null;
  private onReconnectingHandlers: Array<() => void> = [];
  private onReconnectedHandlers: Array<() => void> = [];
  private onConnectedHandlers: Array<() => void> = [];
  private handlers: { [event: string]: Array<EventHandler<any>> } = {};
  private connectPromise: Promise<void> | null = null;
  private disconnectTimer: ReturnType<typeof setTimeout> | null = null;

  connect(): Promise<void> {
    if (this.disconnectTimer) {
      clearTimeout(this.disconnectTimer);
      this.disconnectTimer = null;
    }

    if (this.connection?.state === HubConnectionState.Connected) return Promise.resolve();
    if (this.connectPromise) return this.connectPromise;

    this.connectPromise = (async () => {
      try {
        const hubBaseUrl = import.meta.env.VITE_SIGNALR_URL;
        this.connection = new HubConnectionBuilder()

          .withUrl(`${hubBaseUrl}/chat`, {
            accessTokenFactory: () => localStorage.getItem('token') ?? '',
          })
          .withAutomaticReconnect([0, 2000, 5000, 10000])
          .configureLogging(
            import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning
          )
          .build();

        this.connection.onreconnecting(() => {
          console.info('[SignalR] Reconnecting...');
          this.onReconnectingHandlers.forEach(handler => handler());
        });

        this.connection.onreconnected(() => {
          console.info('[SignalR] Reconnected');
          this.onReconnectedHandlers.forEach(handler => handler());
        });

        this.connection.onclose(() => {
          console.warn('[SignalR] Connection closed');
          this.connectPromise = null;
        });

        // Register all stored handlers before starting
        Object.entries(this.handlers).forEach(([event, eventHandlers]) => {
          eventHandlers.forEach(handler => {
            this.connection?.on(event, handler);
          });
        });

        await this.connection.start();
        console.info('[SignalR] Connected');
        this.onConnectedHandlers.forEach(h => h()); // notify tất cả listeners
      } catch (error) {
        this.connectPromise = null;
        throw error;
      }
    })();

    return this.connectPromise;
  }

  async disconnect(): Promise<void> {
    if (this.disconnectTimer) clearTimeout(this.disconnectTimer);

    this.disconnectTimer = setTimeout(async () => {
      this.connectPromise = null;
      this.disconnectTimer = null;

      const currentConn = this.connection;
      this.connection = null;

      if (currentConn) {
        await currentConn.stop();
      }
    }, 100);
  }

  // ── Client → Server ────────────────────────────────────────

  async joinChannel(channelID: number): Promise<void> {
    await this.invoke('JoinChannel', channelID);
  }

  async sendMessage(channelID: number, request: SendMessageRequest): Promise<void> {
    await this.invoke('SendMessage', channelID, request);
  }

  async sendTyping(channelID: number, isTyping: boolean): Promise<void> {
    await this.invoke('Typing', channelID, isTyping);
  }

  async toggleReaction(messageID: number, channelID: number, emoji: string): Promise<void> {
    await this.invoke('ToggleReaction', messageID, channelID, emoji);
  }

  async deleteMessage(messageID: number, channelID: number): Promise<void> {
    await this.invoke('DeleteMessage', messageID, channelID);
  }

  // ── Server → Client ────────────────────────────────────────

  async getOnlineUsers(): Promise<number[]> {
    if (this.connection?.state !== HubConnectionState.Connected) return [];
    return this.connection.invoke<number[]>('GetOnlineUsers');
  }

  onReceiveMessage(handler: EventHandler<MessageDto>): () => void {
    return this.on('ReceiveMessage', handler);
  }

  onMessageDeleted(handler: EventHandler<number>): () => void {
    return this.on('MessageDeleted', handler);
  }

  onReactionUpdated(
    handler: EventHandler<{ messageID: number; reaction: ReactionDto }>
  ): () => void {
    return this.on('ReactionUpdated', handler);
  }

  onUserTyping(handler: EventHandler<TypingEvent>): () => void {
    return this.on('UserTyping', handler);
  }

  onUserOnline(handler: EventHandler<number>): () => void {
    return this.on('UserOnline', handler);
  }

  onUserOffline(handler: EventHandler<number>): () => void {
    return this.on('UserOffline', handler);
  }

  onOnlineUsersList(handler: EventHandler<number[]>): () => void {
    return this.on('OnlineUsersList', handler);
  }

  // ── Connection Event Handlers ───────────────────────────────────

  onReconnecting(handler: () => void): () => void {
    this.onReconnectingHandlers.push(handler);
    return () => {
      const index = this.onReconnectingHandlers.indexOf(handler);
      if (index > -1) this.onReconnectingHandlers.splice(index, 1);
    };
  }

  onReconnected(handler: () => void): () => void {
    this.onReconnectedHandlers.push(handler);
    return () => {
      const index = this.onReconnectedHandlers.indexOf(handler);
      if (index > -1) this.onReconnectedHandlers.splice(index, 1);
    };
  }

  onConnected(handler: () => void): () => void {
    this.onConnectedHandlers.push(handler);
    // Nếu đã connected rồi thì gọi luôn
    if (this.connection?.state === HubConnectionState.Connected) {
      handler();
    }
    return () => {
      const i = this.onConnectedHandlers.indexOf(handler);
      if (i > -1) this.onConnectedHandlers.splice(i, 1);
    };
  }

  // ── Internals ──────────────────────────────────────────────

  private async invoke(method: string, ...args: unknown[]): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn(`[SignalR] Not connected — dropped: ${method}`);
      return;
    }
    await this.connection.invoke(method, ...args);
  }

  private on<T>(event: string, handler: EventHandler<T>): () => void {
    if (!this.handlers[event]) {
      this.handlers[event] = [];
    }
    this.handlers[event].push(handler);

    console.log(`[SignalRService] Registering handler for ${event}. Connection exists? ${!!this.connection}`);
    // If connection already exists, register immediately
    this.connection?.on(event, handler);

    return () => {
      console.log(`[SignalRService] Unregistering handler for ${event}`);
      const index = this.handlers[event].indexOf(handler);
      if (index > -1) {
        this.handlers[event].splice(index, 1);
      }
      this.connection?.off(event, handler);
    };
  }

  get state(): HubConnectionState | null {
    return this.connection?.state ?? null;
  }
}

export const signalRService = new SignalRService();
