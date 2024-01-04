/** Provides common games statistic info for a period. */
export default interface GamesStatistic {
	/** Finished games count. */
	gameCount: number;

	/** Finished games total duration. */
	totalDuration: string;
}