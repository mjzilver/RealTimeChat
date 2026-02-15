// Generic envelope used for websocket messages
export interface SocketEnvelope<T = any> {
    type: string;
    payload: T;
}

// Error payload
export interface SocketError {
    code?: string;
    message: string;
}

// User payload (matches backend UserPayload)
export interface UserPayload {
    id: number;
    name: string;
    color: string;
    joined: number;
    existingUser?: boolean;
}

// Message payload (matches backend MessagePayload)
export interface MessagePayload {
    id?: number;
    userId: number;
    channelId: number;
    text: string;
    time: number;
    user?: UserPayload | null;
}

// Channel payload (matches backend ChannelPayload)
export interface ChannelPayload {
    id: number;
    name: string;
    color: string;
    created: number;
    password?: string | null;
    ownerId?: number | null;
    users?: UserPayload[] | null;
    messages?: MessagePayload[] | null;
}

// Specific envelopes used by the client
export type BroadcastEnvelope = SocketEnvelope<{ message: MessagePayload }>;
export type MessagesEnvelope = SocketEnvelope<{ messages: MessagePayload[] }>;
export type ChannelsEnvelope = SocketEnvelope<{ channels: ChannelPayload[] }>;
export type ChannelEnvelope = SocketEnvelope<{ channel: ChannelPayload }>;
export type UsersEnvelope = SocketEnvelope<{ users: UserPayload[] }>;
export type UserEnvelope = SocketEnvelope<{ user: UserPayload }>;
export type ErrorEnvelope = SocketEnvelope<{ error: SocketError }>;

export type SocketResponse =
    | BroadcastEnvelope
    | MessagesEnvelope
    | ChannelsEnvelope
    | ChannelEnvelope
    | UsersEnvelope
    | UserEnvelope
    | ErrorEnvelope;