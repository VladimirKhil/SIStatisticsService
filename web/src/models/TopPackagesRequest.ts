import StatisticFilter from "./StatisticFilter";

/** Defines request for top packages statistics. */
export default interface TopPackagesRequest {
	/** Statistic filter. */
	statisticFilter: StatisticFilter;

	/** Optional package source URI. */
	packageSource?: string;

	/** Optional package source fallback URI. */
	fallbackSource?: string;
}