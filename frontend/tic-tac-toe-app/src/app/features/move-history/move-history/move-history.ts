import { Component, computed, inject } from '@angular/core';
import { GameStateStore } from '../../../core/state/game-state.store';
import { formatCellPosition } from './cell-position';

@Component({
  selector: 'app-move-history',
  imports: [],
  templateUrl: './move-history.html',
  styleUrl: './move-history.scss',
})
export class MoveHistory {
  private readonly store = inject(GameStateStore);

  readonly moves = computed(() => this.store.gameState()?.moveHistory ?? []);

  readonly formatPosition = formatCellPosition;
}
