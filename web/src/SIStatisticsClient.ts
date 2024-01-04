import SIStatisticsClientOptions from './SIStatisticsClientOptions';
import GamesResponse from './models/GamesResponse';
import GamesStatistic from './models/GamesStatistic';
import PackagesStatistic from './models/PackagesStatistic';
import StatisticFilter from './models/StatisticFilter';

/** Defines SIStatistics service client. */
export default class SIStatisticsClient {
	/**
	 * Initializes a new instance of SIStatisticsClientOptions class.
	 * @param options Client options.
	 */
	constructor(public options: SIStatisticsClientOptions) { }

	/** Gets latest games info.
	 * @param filter Statistic filter.
	 */
	async getLatestGamesInfoAsync(filter: StatisticFilter) {
		return this.getAsync<GamesResponse>(`games/results${this.buildFilter(filter)}`);
	}

	/** Gets latest games cumulative statistic.
	 * @param filter Statistic filter.
	 */
	async getLatestGamesStatisticAsync(filter: StatisticFilter) {
		return this.getAsync<GamesStatistic>(`games/stats${this.buildFilter(filter)}`);
	}

	/** Gets latest top played packages.
	 * @param filter Statistic filter.
	 */
	async getLatestTopPackagesAsync(filter: StatisticFilter) {
		return this.getAsync<PackagesStatistic>(`games/packages${this.buildFilter(filter)}`);
	}

	private buildFilter(filter: StatisticFilter) {
		return `?platform=${filter.platform}&from=${filter.from.toISOString()}&to=${filter.to.toISOString()}`;
	}

	/**
	 * Gets resource by Uri.
	 * @param uri Resource Uri.
	 */
	async getAsync<T>(uri: string) {
		const response = await fetch(`${this.options.serviceUri}/api/v1/${uri}`);

		if (!response.ok) {
			throw new Error(`Error while retrieving ${uri}: ${response.status} ${await response.text()}`);
		}

		return <T>(await response.json());
	}
}