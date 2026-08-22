import { GameMode } from './game-mode.enum';
import { GameStatus } from './game-status.enum';
import { Move } from './move.model';
import { Player } from './player.enum';

export interface GameState {
  gameId: string;
  board: (Player | null)[];
  currentPlayer: Player;
  gameMode: GameMode;
  status: GameStatus;
  winner: Player | null;
  winningCells: number[] | null;
  moveHistory: Move[];
}
