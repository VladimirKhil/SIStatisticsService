import SIStatisticsClient from '../src/SIStatisticsClient';
import SIStatisticsClientOptions from '../src/SIStatisticsClientOptions';
import GamePlatforms from '../src/models/GamePlatforms';

const options: SIStatisticsClientOptions = {
	//serviceUri: 'http://localhost:5165'
	serviceUri: 'http://vladimirkhil.com/sistatistics'
};

const siStatisticsClient = new SIStatisticsClient(options);

const ONE_HOUR = 60 * 60 * 1000;

test('Get latest games', async () => {
	const now = new Date();

	const statistics = await siStatisticsClient.getLatestGamesInfoAsync({
		platform: GamePlatforms.GameServer,
		from: new Date(now.getTime() - ONE_HOUR),
		to: now,
	});

	expect(statistics).not.toBeNull();
	expect(statistics.results.length).toBeGreaterThanOrEqual(5);
});

test('Get games statistics', async () => {
	const now = new Date();

	const statistics = await siStatisticsClient.getLatestGamesStatisticAsync({
		platform: GamePlatforms.GameServer,
		from: new Date(now.getTime() - ONE_HOUR),
		to: now,
	});

	expect(statistics).not.toBeNull();
	expect(statistics.gameCount).toBeGreaterThanOrEqual(5);
});

test('Get packages statistics', async () => {
	const now = new Date();

	const statistics = await siStatisticsClient.getLatestTopPackagesAsync({
		platform: GamePlatforms.GameServer,
		from: new Date(now.getTime() - ONE_HOUR),
		to: now,
	});

	expect(statistics).not.toBeNull();
	expect(statistics.packages.length).toBeGreaterThanOrEqual(5);
});
