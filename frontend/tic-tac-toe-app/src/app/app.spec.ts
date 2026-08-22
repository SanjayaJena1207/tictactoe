import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { App } from './app';
import { GameService } from './core/services/game.service';
import { ScoreboardService } from './core/services/scoreboard.service';

describe('App', () => {
  let getScoreboard: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    getScoreboard = vi.fn().mockReturnValue(of({ xWins: 0, oWins: 0, draws: 0 }));

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: GameService, useValue: {} },
        { provide: ScoreboardService, useValue: { getScoreboard } },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('fetches the scoreboard on init', () => {
    TestBed.createComponent(App);
    expect(getScoreboard).toHaveBeenCalled();
  });

  it('lays out the mode selector and status banner at top, board and controls in the main area, and move history and scoreboard in a side panel', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('.top-bar app-game-mode-selector')).toBeTruthy();
    expect(compiled.querySelector('.top-bar app-status-banner')).toBeTruthy();
    expect(compiled.querySelector('.main-area app-game-board')).toBeTruthy();
    expect(compiled.querySelector('.main-area app-game-actions')).toBeTruthy();
    expect(compiled.querySelector('.side-panel app-move-history')).toBeTruthy();
    expect(compiled.querySelector('.side-panel app-scoreboard')).toBeTruthy();
  });
});
