import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { Channel } from '../../types/channel';
import { User } from '../../types/user';
import { ChannelPayload } from '../../types/socketMessage';

@Injectable({
	providedIn: 'root',
})
export class ChannelService {
	private channelSubject = new Subject<Channel[]>();
	channels$ = this.channelSubject.asObservable();

	private currentChannel: Channel | null = null;
	currentChannel$ = new Subject<Channel | null>();	

	private channels: Channel[] = [];

	parseChannels(data: ChannelPayload[]): Channel[] {
		this.channels = data.map(item => new Channel(item.id, item.name, item.color, item.created, item.ownerId ?? undefined, item.password ?? undefined));
		this.channelSubject.next(this.channels);
		return this.channels;
	}

	updateChannel(data: ChannelPayload): void {
		const channel = this.channels.find(c => c.id === data.id);
		if (channel) {
			channel.name = data.name;
			channel.color = data.color;
			channel.created = data.created;
			channel.password = data.password ?? undefined;
			channel.ownerId = data.ownerId ?? undefined;
		} else {
			this.channels.push(new Channel(data.id, data.name, data.color, data.created, data.ownerId ?? undefined, data.password ?? undefined));
		}
	}

	updateChannelUsers(data: ChannelPayload): void {
		const channel = this.channels.find(c => c.id === data.id);
		if (channel && data.users) {
			channel.users = data.users.map((item: { id: number; name: string; joined: number; color: string }) => new User(item.id, item.name, item.joined, item.color));
		}
	}

	deleteChannel(data: ChannelPayload) {
		this.channels = this.channels.filter(c => c.id !== data.id);
		this.channelSubject.next(this.channels);
	}

	getChannels(): Channel[] {
		return this.channels;
	}

	setCurrentChannel(channel: Channel | null): void {
		this.currentChannel = channel;
		this.currentChannel$.next(channel);
	}

	removeUserFromChannels(userId: number): void {
		this.channels.forEach(channel => {
			channel.users = channel.users.filter(user => user.id !== userId);
		});
	}
}
