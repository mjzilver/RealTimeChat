import { Channel } from './channel';
import { MessageUser, User } from './user';

export class Message {
	id?: number;
	userId: number;
	user: User | MessageUser;
	text: string;
	time: number;
	channelId: number;
	channel: Channel;

	constructor(
		user: User | MessageUser,
		text: string,
		time: number = Date.now(),
		channel: Channel,
		id?: number
	) {
		this.user = user;
		this.userId = user.id;
		this.text = text;
		this.time = time;
		this.channel = channel;
		this.channelId = channel.id;
		if (id) this.id = id;
	}

	getSerialized() {
		return {
			userId: this.userId,
			channelId: this.channelId,
			text: this.text,
			time: this.time
		};
	}
}