import { Component, inject } from '@angular/core';
import { ScoreboardStore } from '../../../core/state/scoreboard.store';

@Component({
  selector: 'app-scoreboard',
  imports: [],
  templateUrl: './scoreboard.html',
  styleUrl: './scoreboard.scss',
})
export class ScoreboardPanel {
  private readonly store = inject(ScoreboardStore);

  readonly scoreboard = this.store.scoreboard;
}
