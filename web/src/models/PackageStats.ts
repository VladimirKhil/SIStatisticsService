import PackageTopLevelStats from './PackageTopLevelStats';
import QuestionStats from './QuestionStats';

/** Represents statistical data for a package. */
export default interface PackageStats {
	/** Top-level statistical data for the package. */
	topLevelStats: PackageTopLevelStats;

	/** A dictionary containing statistics for each question in the package. */
	questionStats: Record<string, QuestionStats>;
}
