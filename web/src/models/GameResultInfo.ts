import GamePlatforms from "./GamePlatforms";
import PackageInfo from "./PackageInfo";

/** Defines game result info. */
export default interface GameResultInfo {
	/** Game name. */
	name: string;

	/** Game platform. */
	platform: GamePlatforms;

	/** Game finish time. */
	finishTime: Date;

	/** Game duration. */
	duration: string;

	/** Game package info. */
	package: PackageInfo;

	/** Game results: player names and their scores. */
	results: Record<string, number>;

	/** Player reviews. */
	reviews: Record<string, number>;
}
