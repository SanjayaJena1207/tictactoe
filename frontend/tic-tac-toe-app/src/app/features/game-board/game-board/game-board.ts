import { Component, computed, inject } from '@angular/core';
import { GameService } from '../../../core/services/game.service';
import { GameStateStore } from '../../../core/state/game-state.store';
import { cellCenter, lineEndpoints } from './cell-geometry';
import { GameCell } from '../game-cell/game-cell';

@Component({
  selector: 'app-game-board',
  imports: [GameCell],
  templateUrl: './game-board.html',
  styleUrl: './game-board.scss',
})
export class GameBoard {
  private readonly gameService = inject(GameService);
  private readonly store = inject(GameStateStore);

  readonly gameState = this.store.gameState;
  readonly errorMessage = computed(() => this.store.error()?.detail ?? null);
  readonly winningCells = computed(() => new Set(this.gameState()?.winningCells ?? []));

  /* Presentation-only: derives the winning-line SVG endpoints from the
     existing winningCells state. Does not affect game logic. */
  readonly winLine = computed(() => lineEndpoints(this.gameState()?.winningCells));
  readonly cellCenter = cellCenter;

  onCellClick(index: number): void {
    const game = this.gameState();
    if (!game) {
      return;
    }
    this.gameService.applyMove(game.gameId, game.currentPlayer, index).subscribe({ error: () => {} });
  }
}
