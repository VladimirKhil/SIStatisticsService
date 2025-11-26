/** Defines a request for package statistics. */
export default interface PackageStatsRequest {
	/** Package name. */
	name: string;

	/** Package hash. */
	hash: string;

	/** Package authors. */
	authors: string[];
}
