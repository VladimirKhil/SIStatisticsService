import GamePlatforms from './GamePlatforms';

/** Defines a statistic request filter. */
export default interface StatisticFilter {
	/** Game platform. */
	platform: GamePlatforms;

	/** Start date. */
	from: Date;

	/** End date. */
	to: Date;

	/** Game language code. */
	languageCode?: string;

	/** Maximum result count. */
	count: number;
}