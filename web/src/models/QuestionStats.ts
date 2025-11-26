/** Defines statistics for a question. */
export default interface QuestionStats {
	/** Number of times question was shown. */
	shownCount: number;

	/** Number of players that have seen the question. */
	playerSeenCount: number;

	/** Number of correct answers for the question. */
	correctCount: number;

	/** Number of wrong answers for the question. */
	wrongCount: number;
}
