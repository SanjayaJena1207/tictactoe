import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ScoreboardService } from './core/services/scoreboard.service';
import { GameBoard } from './features/game-board/game-board/game-board';
import { StatusBanner } from './features/game-board/status-banner/status-banner';
import { GameActions } from './features/game-controls/game-actions/game-actions';
import { GameModeSelector } from './features/game-controls/game-mode-selector/game-mode-selector';
import { MoveHistory } from './features/move-history/move-history/move-history';
import { ScoreboardPanel } from './features/scoreboard/scoreboard/scoreboard';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    GameModeSelector,
    StatusBanner,
    GameBoard,
    GameActions,
    MoveHistory,
    ScoreboardPanel,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('tic-tac-toe-app');

  constructor() {
    inject(ScoreboardService).getScoreboard().subscribe();
  }
}
