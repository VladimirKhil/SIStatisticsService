import PackageInfo from "./PackageInfo";

/** Represents package statistics. */
export default interface PackageStatistic {
	/** Package info. */
	package: PackageInfo;

	/** Number of games played with this package. */
	gameCount: number;
}