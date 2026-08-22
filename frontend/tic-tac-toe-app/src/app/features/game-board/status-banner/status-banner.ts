import { Component, computed, inject } from '@angular/core';
import { GameStateStore } from '../../../core/state/game-state.store';

type StatusVariant = 'progress' | 'win' | 'draw';

@Component({
  selector: 'app-status-banner',
  imports: [],
  templateUrl: './status-banner.html',
  styleUrl: './status-banner.scss',
})
export class StatusBanner {
  private readonly store = inject(GameStateStore);

  readonly statusVariant = computed<StatusVariant | null>(() => {
    const game = this.store.gameState();
    if (!game) {
      return null;
    }
    switch (game.status) {
      case 'InProgress':
        return 'progress';
      case 'Won':
        return 'win';
      case 'Draw':
        return 'draw';
    }
  });

  readonly message = computed(() => {
    const game = this.store.gameState();
    if (!game) {
      return '';
    }
    switch (game.status) {
      case 'InProgress':
        return `Player ${game.currentPlayer}'s turn`;
      case 'Won':
        return `Player ${game.winner} wins!`;
      case 'Draw':
        return `It's a draw!`;
    }
  });
}
