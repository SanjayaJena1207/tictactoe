import { Injectable, signal } from '@angular/core';
import { Scoreboard } from '../models/scoreboard.model';

@Injectable({ providedIn: 'root' })
export class ScoreboardStore {
  private readonly scoreboardSignal = signal<Scoreboard | null>(null);

  readonly scoreboard = this.scoreboardSignal.asReadonly();

  setScoreboard(scoreboard: Scoreboard): void {
    this.scoreboardSignal.set(scoreboard);
  }
}
