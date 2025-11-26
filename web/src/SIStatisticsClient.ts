import SIStatisticsClientOptions from './SIStatisticsClientOptions';
import GamesResponse from './models/GamesResponse';
import GamesStatistic from './models/GamesStatistic';
import PackagesStatistic from './models/PackagesStatistic';
import PackageStats from './models/PackageStats';
import PackageStatsRequest from './models/PackageStatsRequest';
import StatisticFilter from './models/StatisticFilter';
import TopPackagesRequest from './models/TopPackagesRequest';

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
	 * @param request Request for top packages statistics.
	 */
	async getLatestTopPackagesAsync(request: TopPackagesRequest) {
		return this.getAsync<PackagesStatistic>(`games/packages${this.buildRequest(request)}`);
	}

	/** Gets package statistics.
	 * @param request Request for package statistics.
	 */
	async getPackageStats(request: PackageStatsRequest) {
		const authors = request.authors.join(',');
		return this.getAsync<PackageStats>(`games/packages/stats?name=${encodeURIComponent(request.name)}&hash=${encodeURIComponent(request.hash)}&authors=${encodeURIComponent(authors)}`);
	}

	private buildFilter(filter: StatisticFilter) {
		const languageFilter = filter.languageCode ? '&languageCode=' + filter.languageCode : '';
		return `?platform=${filter.platform}&from=${filter.from.toISOString()}&to=${filter.to.toISOString()}&count=${filter.count}${languageFilter}`;
	}

	private buildRequest(request: TopPackagesRequest) {
		const packageSourceFilter = request.packageSource ? `&source=${encodeURIComponent(request.packageSource)}` : '';
		const fallbackSourceFilter = request.fallbackSource ? `&fallbackSource=${encodeURIComponent(request.fallbackSource)}` : '';
		return `?platform=${request.statisticFilter.platform}&from=${request.statisticFilter.from.toISOString()}&to=${request.statisticFilter.to.toISOString()}&count=${request.statisticFilter.count}${packageSourceFilter}${fallbackSourceFilter}`;
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