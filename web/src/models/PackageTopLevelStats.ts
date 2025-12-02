/** Represents top-level statistical data for a package. */
export default interface PackageTopLevelStats {
	/** The total number of started games associated with the package. */
	startedGameCount: number;

	/** The total number of finished games associated with the package. */
	completedGameCount: number;
}
