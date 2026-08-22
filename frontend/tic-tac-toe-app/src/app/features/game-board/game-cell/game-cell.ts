import { Component, computed, input, output } from '@angular/core';
import { GameStatus } from '../../../core/models/game-status.enum';
import { Player } from '../../../core/models/player.enum';

@Component({
  selector: 'app-game-cell',
  imports: [],
  templateUrl: './game-cell.html',
  styleUrl: './game-cell.scss',
})
export class GameCell {
  readonly value = input<Player | null>(null);
  readonly status = input.required<GameStatus>();
  readonly index = input.required<number>();
  readonly highlighted = input(false);

  readonly cellClick = output<number>();

  readonly isClickable = computed(() => this.value() === null && this.status() === 'InProgress');
  readonly display = computed(() => this.value() ?? '');

  onClick(): void {
    if (!this.isClickable()) {
      return;
    }
    this.cellClick.emit(this.index());
  }
}
