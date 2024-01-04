import GameResultInfo from './GameResultInfo';

/** Defines a collection of game results. */
export default interface GamesResponse {
	/** Game results. */
	results: GameResultInfo[];
}