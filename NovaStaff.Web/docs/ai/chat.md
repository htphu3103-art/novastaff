# chat

**Route:** `/chat` · Entry: `modules/chat/ChatPage.tsx` (~1.2k lines — agent: **không đọc full**; dùng section hoặc Semble line range). Ignore `chat/pages/ChatPage.tsx` duplicate.

| kind | path |
|------|------|
| rest | `services/chatApi.ts` |
| realtime | `services/signalRService.ts` → hub `/chathub` token `localStorage.token` |
| hook | `hooks/useChatSignalR.ts` |
| types | `types/chat.ts` |

**Global notify:** `MainLayout` cũng connect SignalR toast khi không ở `/chat`

## REST `/chat`
| op | path |
|----|------|
| GET | /channels · /channels/directs · /channels/:id/messages · /channels/:id/members · /users/lookup |
| POST | /channels · /channels/direct · /channels/:id/messages · /channels/:id/read |

## Semble
`ChatPage SignalR` · `chatApi channels messages` · line range từ kết quả search

## Open order
1. `chatApi.ts` hoặc `signalRService.ts` 2. `ChatPage.tsx` section (offset/limit)
