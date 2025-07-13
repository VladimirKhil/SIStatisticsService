/** Represents a game package info. */
export default interface PackageInfo {
	/** Game package name. */
	name: string;

	/** Game package hash. */
	hash: string;

	/** Game package authors. */
	authors: string[];

	/** Package author contacts. */
	authorsContacts: string;

	/** Package source URI. */
	source?: string;
}