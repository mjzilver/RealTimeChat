import { Injectable } from '@angular/core';
import { ConnectionService } from './connection.service';
import { MessageService } from './message.service';
import { ChannelService } from './channel.service';
import { UserService } from './user.service';
import { ErrorService } from './error.service';
import { SocketResponse, ErrorEnvelope, BroadcastEnvelope, MessagesEnvelope, ChannelsEnvelope, ChannelEnvelope, UsersEnvelope, UserEnvelope } from '../../types/socketMessage'
import { Message } from '../../types/message';
import { Channel, NewChannel } from '../../types/channel';
import { User, UserLogin } from '../../types/user';
import { environment } from '../../environments/environment';

@Injectable({
	providedIn: 'root',
})
export class WebsocketService {
	constructor(
        private wsConnectionService: ConnectionService,
        private messageService: MessageService,
        private channelService: ChannelService,
        private userService: UserService,
        private errorService: ErrorService
	) {
		this.wsConnectionService.connect(environment.websocketUrl);
		this.wsConnectionService.onMessage((event: MessageEvent) => this.handleMessage(event));
	}

	private handleMessage(event: MessageEvent): void {
		const parsed: SocketResponse = JSON.parse(event.data);

		console.log(`Received message: ${JSON.stringify(parsed)}`);

		const type = (parsed as any).type as string | undefined;
		const payload = (parsed as any).payload;

		if (!type) {
			console.warn('Received envelope without type');
			return;
		}

		this.errorService.setError(null);

		switch (type) {
		case 'broadcast': {
			const env = parsed as BroadcastEnvelope;
			this.messageService.parseMessage(env.payload.message, this.channelService.getChannels());
			break;
		}
		case 'messages': {
			const env = parsed as MessagesEnvelope;
			this.messageService.parseMessages(env.payload.messages, this.channelService.getChannels());
			break;
		}
		case 'channels': {
			const env = parsed as ChannelsEnvelope;
			this.channelService.parseChannels(env.payload.channels);
			break;
		}
		case 'channelCreated':
		case 'channelUpdated': {
			const env = parsed as ChannelEnvelope;
			this.channelService.updateChannel(env.payload.channel);
			break;
		}
		case 'channelDeleted': {
			const env = parsed as ChannelEnvelope;
			this.channelService.deleteChannel(env.payload.channel);
			break;
		}
		case 'userJoinedChannel':
		case 'userLeftChannel': {
			// payload carries channel with users
			const env = parsed as ChannelEnvelope;
			this.channelService.updateChannelUsers(env.payload.channel);
			break;
		}
		case 'users': {
			const env = parsed as UsersEnvelope;
			this.userService.parseUsers(env.payload.users);
			break;
		}
		case 'userUpdated':
		case 'loginUser':
		case 'registerUser': {
			const env = parsed as UserEnvelope;
			this.userService.handleLogin(env.payload.user);
			break;
		}
		case 'logoutUser': {
			const env = parsed as UserEnvelope;
			this.channelService.removeUserFromChannels(env.payload.user.id ?? -1);
			break;
		}
		case 'error': {
			const env = parsed as ErrorEnvelope;
			console.error(env.payload.error);
			this.errorService.setError(env.payload.error?.message ?? 'Unknown error');
			break;
		}
		default:
			console.warn(`Unknown type: ${type}`);
		}
	}

	// MESSAGE COMMANDS //
	sendMessage(message: Message): void {
		this.wsConnectionService.sendMessage({ type: 'broadcastMessage', payload: { userId: message.userId, channelId: message.channelId, text: message.text } });
	}

	getMessages(channel: Channel): void {
		this.wsConnectionService.sendMessage({ type: 'getMessages', payload: { channelId: channel.id } });
	}

	// CHANNEL COMMANDS //

	getChannels(): void {
		this.wsConnectionService.sendMessage({ type: 'getChannels', payload: {} });
	}

	joinChannel(channel: Channel, user: User): void {
		this.wsConnectionService.sendMessage({ type: 'joinChannel', payload: { channelId: channel.id, userId: user.id } });
	}

	leaveChannel(channel: Channel, user: User): void {
		this.wsConnectionService.sendMessage({ type: 'leaveChannel', payload: { channelId: channel.id, userId: user.id } });
	}

	createChannel(channel: Channel | NewChannel): void {
		this.wsConnectionService.sendMessage({ type: 'createChannel', payload: { name: (channel as any).name, password: (channel as any).password ?? null, color: (channel as any).color ?? '#000000', ownerId: (channel as any).ownerId ?? null } });
	}

	updateChannel(channel: Channel): void {
		this.wsConnectionService.sendMessage({ type: 'updateChannel', payload: { id: channel.id, name: channel.name, password: channel.password, color: channel.color, ownerId: channel.ownerId } });
	}

	deleteChannel(channel: Channel): void {
		this.wsConnectionService.sendMessage({ type: 'deleteChannel', payload: { id: channel.id } });
	}

	// USER COMMANDS //
	getUsers(): void {
		this.wsConnectionService.sendMessage({ type: 'getUsers', payload: {} });
	}

	updateUser(user: User): void {
		this.wsConnectionService.sendMessage({ type: 'updateUser', payload: { id: user.id, name: user.name, color: user.color } });
	}

	attemptLogin(user: UserLogin): void {
		this.wsConnectionService.sendMessage({ type: 'loginUser', payload: { username: user.name, password: user.password } });
	}

	registerUser(user: UserLogin): void {
		this.wsConnectionService.sendMessage({ type: 'registerUser', payload: { username: user.name, password: user.password, color: user.color } });
	}

	logout(user: User, channel: Channel | null): void {
		this.wsConnectionService.sendMessage({ type: 'logoutUser', payload: {} });
	}
}
