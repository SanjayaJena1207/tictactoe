import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { GameState } from '../../../core/models/game-state.model';
import { GameService } from '../../../core/services/game.service';
import { ScoreboardService } from '../../../core/services/scoreboard.service';
import { GameStateStore } from '../../../core/state/game-state.store';
import { GameActions } from './game-actions';

function makeGame(overrides: Partial<GameState>): GameState {
  return {
    gameId: 'g1',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    gameMode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moveHistory: [],
    ...overrides,
  };
}

function getUndoButton(fixture: { nativeElement: HTMLElement }): HTMLButtonElement {
  const button = Array.from(fixture.nativeElement.querySelectorAll('button')).find((btn) =>
    btn.textContent?.includes('Undo Last Move'),
  );
  if (!button) {
    throw new Error('Undo button not found');
  }
  return button as HTMLButtonElement;
}

describe('GameActions', () => {
  let store: GameStateStore;
  let undo: ReturnType<typeof vi.fn>;
  let resetGame: ReturnType<typeof vi.fn>;
  let resetScoreboard: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    undo = vi.fn().mockReturnValue(of(makeGame({})));
    resetGame = vi.fn().mockReturnValue(of(makeGame({})));
    resetScoreboard = vi.fn().mockReturnValue(of({ xWins: 0, oWins: 0, draws: 0 }));

    await TestBed.configureTestingModule({
      imports: [GameActions],
      providers: [
        { provide: GameService, useValue: { undo, resetGame } },
        { provide: ScoreboardService, useValue: { resetScoreboard } },
      ],
    }).compileComponents();

    store = TestBed.inject(GameStateStore);
  });

  it('disables undo when there is no game', () => {
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(fixture.componentInstance.canUndo()).toBe(false);
  });

  it('disables undo when no moves have been made yet', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(fixture.componentInstance.canUndo()).toBe(false);
  });

  it('enables undo once a move has been made and the game is still in progress', () => {
    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(fixture.componentInstance.canUndo()).toBe(true);
  });

  it('disables undo once the game has finished, even with move history', () => {
    store.setGameState(
      makeGame({
        status: 'Won',
        winner: 'X',
        moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }],
      }),
    );
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(fixture.componentInstance.canUndo()).toBe(false);
  });

  it('renders the Undo Last Move button as disabled when there is no game', () => {
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(getUndoButton(fixture).disabled).toBe(true);
  });

  it('renders the Undo Last Move button as disabled when moveHistory is empty', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(getUndoButton(fixture).disabled).toBe(true);
  });

  it('renders the Undo Last Move button as disabled once the game has finished, even with move history', () => {
    store.setGameState(
      makeGame({
        status: 'Won',
        winner: 'X',
        moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }],
      }),
    );
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(getUndoButton(fixture).disabled).toBe(true);
  });

  it('renders the Undo Last Move button as enabled once a move has been made and the game is in progress', () => {
    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(getUndoButton(fixture).disabled).toBe(false);
  });

  it('does not call GameService.undo when the button is disabled and clicked', () => {
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();

    getUndoButton(fixture).click();

    expect(undo).not.toHaveBeenCalled();
  });

  it('calls GameService.undo with the current game id', () => {
    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();

    fixture.componentInstance.undo();

    expect(undo).toHaveBeenCalledWith('g1');
  });

  it('disables reset game when there is no game', () => {
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();
    expect(fixture.componentInstance.canResetGame()).toBe(false);
  });

  it('calls GameService.resetGame with the current game id', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();

    fixture.componentInstance.resetGame();

    expect(resetGame).toHaveBeenCalledWith('g1');
  });

  it('calls ScoreboardService.resetScoreboard without touching game state', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameActions);
    fixture.detectChanges();

    fixture.componentInstance.resetScoreboard();

    expect(resetScoreboard).toHaveBeenCalled();
    expect(store.gameState()?.gameId).toBe('g1');
  });
});
