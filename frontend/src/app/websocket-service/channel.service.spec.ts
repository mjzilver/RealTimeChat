import { TestBed } from '@angular/core/testing';
import { ChannelService } from './channel.service';
import { ChannelPayload, UserPayload } from '../../types/socketMessage';

import { MockDataFactory } from '../../mocks/MockData';

describe('ChannelService', () => {
	let service: ChannelService;

	const mockData = MockDataFactory();

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(ChannelService);
	});

	it('should be created', () => {
		expect(service).toBeTruthy();
	});

	it('should parse and emit channels', done => {
		const socketChannels: ChannelPayload[] = [
			mockData.mockSocketChannels[0],
			mockData.mockSocketChannels[1]
		];

		service.channels$.subscribe(channels => {
			expect(channels.length).toBe(2);
			expect(channels[0]).toEqual(jasmine.objectContaining(mockData.mockChannels[0]));
			expect(channels[1]).toEqual(jasmine.objectContaining(mockData.mockChannels[1]));
			done();
		});

		const parsedChannels = service.parseChannels(socketChannels);
		expect(parsedChannels.length).toBe(2);
	});

	it('should update channel users', () => {
		const testUser1: UserPayload = mockData.mockSocketUsers[0] as UserPayload;
		const testUser2: UserPayload = mockData.mockSocketUsers[1] as UserPayload;

		const socketUsers: UserPayload[] = [
			testUser1, testUser2
		];

		const socketChannels: ChannelPayload[] = [
			mockData.mockSocketChannels[0]
		];
		socketChannels[0].users = socketUsers as UserPayload[];

		service.parseChannels(socketChannels);

		service.updateChannelUsers(socketChannels[0]);

		const channels = service.getChannels();
		expect(channels[0].users.length).toBe(2);
		expect(channels[0].users[0]).toEqual(jasmine.objectContaining(testUser1));
		expect(channels[0].users[1]).toEqual(jasmine.objectContaining(testUser2));
	});

	it('should return channels', () => {
		const socketChannels: ChannelPayload[] = [
			mockData.mockSocketChannels[0],
			mockData.mockSocketChannels[1]
		];

		service.parseChannels(socketChannels);
		const channels = service.getChannels();

		expect(channels.length).toBe(2);
		expect(channels[0]).toEqual(jasmine.objectContaining(mockData.mockChannels[0]));
		expect(channels[1]).toEqual(jasmine.objectContaining(mockData.mockChannels[1]));
	});
});
