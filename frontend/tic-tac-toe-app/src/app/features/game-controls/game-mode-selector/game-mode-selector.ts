import { Component, computed, inject } from '@angular/core';
import { GameMode } from '../../../core/models/game-mode.enum';
import { GameService } from '../../../core/services/game.service';
import { GameStateStore } from '../../../core/state/game-state.store';

@Component({
  selector: 'app-game-mode-selector',
  imports: [],
  templateUrl: './game-mode-selector.html',
  styleUrl: './game-mode-selector.scss',
})
export class GameModeSelector {
  private readonly gameService = inject(GameService);
  private readonly store = inject(GameStateStore);

  readonly selectedMode = computed(() => this.store.gameState()?.gameMode ?? null);

  readonly isMidGame = computed(() => {
    const game = this.store.gameState();
    return !!game && game.status === 'InProgress' && game.moveHistory.length > 0;
  });

  selectMode(mode: GameMode): void {
    if (this.isMidGame()) {
      return;
    }
    this.gameService.createGame(mode).subscribe({ error: () => {} });
  }
}
