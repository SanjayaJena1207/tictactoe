import { Component, computed, inject } from '@angular/core';
import { GameService } from '../../../core/services/game.service';
import { ScoreboardService } from '../../../core/services/scoreboard.service';
import { GameStateStore } from '../../../core/state/game-state.store';

@Component({
  selector: 'app-game-actions',
  imports: [],
  templateUrl: './game-actions.html',
  styleUrl: './game-actions.scss',
})
export class GameActions {
  private readonly gameService = inject(GameService);
  private readonly scoreboardService = inject(ScoreboardService);
  private readonly store = inject(GameStateStore);

  /** Mirrors the backend's undo rule for UX only; the server remains the enforcement point. */
  readonly canUndo = this.store.isMidGame;
  readonly canResetGame = computed(() => this.store.gameState() !== null);

  undo(): void {
    const game = this.store.gameState();
    if (!game) {
      return;
    }
    this.gameService.undo(game.gameId).subscribe({ error: () => {} });
  }

  resetGame(): void {
    const game = this.store.gameState();
    if (!game) {
      return;
    }
    this.gameService.resetGame(game.gameId).subscribe({ error: () => {} });
  }

  resetScoreboard(): void {
    this.scoreboardService.resetScoreboard().subscribe({ error: () => {} });
  }
}
