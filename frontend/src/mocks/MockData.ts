import { Channel } from "../types/channel";
import { ChannelPayload, UserPayload, MessagePayload } from "../types/socketMessage";
import { User } from "../types/user";

const getUnixTimestamp = (): number => Date.now();

const generateMockDates = (): number[] => [
	getUnixTimestamp(),
	946684799999, // 1999-12-31T23:59:59.999Z
	946684800000  // 2000-01-01T00:00:00.000Z
];

const generateMockUsers = (mockDates: number[]): User[] => [
	new User(1, 'Jeroen', getUnixTimestamp(), 'green'),
	new User(2, '1721191---;', mockDates[1], 'yellow'),
	new User(3, 'Charlie', getUnixTimestamp(), 'blue'),
	new User(4, 'Alice', mockDates[2], 'red')
];

const generateMockSocketUsers = (mockUsers: User[]) => 
	mockUsers.map(user => ({
		id: user.id,
		name: user.name,
		joined: user.joined,
		color: user.color
	} as UserPayload));

const generateMockChannels = (): Channel[] => [
	new Channel(1, 'Channel 1', 'red'),
	new Channel(2, 'Channel 2', 'blue')
];

const generateMockSocketChannels = (mockChannels: Channel[]) => 
	mockChannels.map(channel => ({
		id: channel.id,
		name: channel.name,
		color: channel.color,
		created: getUnixTimestamp()
	} as ChannelPayload));

const generateMockMessages = (mockUsers: User[], mockChannels: Channel[]) => [
	{
		channel: mockChannels[0],
		text: 'Hello World!',
		time: getUnixTimestamp(),
		user: mockUsers[0]
	},
	{
		channel: mockChannels[1],
		text: 'Goodbye!',
		time: getUnixTimestamp(),
		user: mockUsers[1]
	}
];

const generateMockSocketMessages = 
	(mockSocketUsers: UserPayload[], mockSocketChannels: ChannelPayload[]) => [
		{
			channelId: mockSocketChannels[0].id,
			text: 'Hello World!',
			time: getUnixTimestamp(),
			userId: mockSocketUsers[0].id,
			user: mockSocketUsers[0]
		} as MessagePayload,
		{
			channelId: mockSocketChannels[1].id,
			text: 'Goodbye!',
			time: getUnixTimestamp(),
			userId: mockSocketUsers[1].id,
			user: mockSocketUsers[1]
		} as MessagePayload
	];

export const MockDataFactory = () => {
	const mockDates = generateMockDates();
	const mockUsers = generateMockUsers(mockDates);
	const mockSocketUsers = generateMockSocketUsers(mockUsers);
	const mockChannels = generateMockChannels();
	const mockSocketChannels = generateMockSocketChannels(mockChannels);
	const mockMessages = generateMockMessages(mockUsers, mockChannels);
	const mockSocketMessages = generateMockSocketMessages(mockSocketUsers, mockSocketChannels);

	return {
		mockDates,
		mockUsers,
		mockSocketUsers,
		mockChannels,
		mockSocketChannels,
		mockMessages,
		mockSocketMessages
	};
};
