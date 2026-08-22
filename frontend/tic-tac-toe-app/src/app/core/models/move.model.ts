import { Player } from './player.enum';

export interface Move {
  moveNumber: number;
  player: Player;
  cellIndex: number;
  timestamp: string;
}
