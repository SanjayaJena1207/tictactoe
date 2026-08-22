import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { GameState } from '../../../core/models/game-state.model';
import { GameService } from '../../../core/services/game.service';
import { GameStateStore } from '../../../core/state/game-state.store';
import { GameModeSelector } from './game-mode-selector';

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

describe('GameModeSelector', () => {
  let store: GameStateStore;
  let createGame: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    createGame = vi.fn().mockReturnValue(of(makeGame({})));

    await TestBed.configureTestingModule({
      imports: [GameModeSelector],
      providers: [{ provide: GameService, useValue: { createGame } }],
    }).compileComponents();

    store = TestBed.inject(GameStateStore);
  });

  it('is not mid-game before any game exists', () => {
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();
    expect(fixture.componentInstance.isMidGame()).toBe(false);
  });

  it('is not mid-game once a fresh game exists with no moves yet', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();
    expect(fixture.componentInstance.isMidGame()).toBe(false);
  });

  it('is mid-game once moves have been made and the game is still in progress', () => {
    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();
    expect(fixture.componentInstance.isMidGame()).toBe(true);
  });

  it('is not mid-game once the game has finished, even with move history', () => {
    store.setGameState(
      makeGame({
        status: 'Won',
        winner: 'X',
        moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }],
      }),
    );
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();
    expect(fixture.componentInstance.isMidGame()).toBe(false);
  });

  it('calls GameService.createGame with the selected mode', () => {
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();

    fixture.componentInstance.selectMode('VsComputer');

    expect(createGame).toHaveBeenCalledWith('VsComputer');
  });

  it('does not create a game when mode switching is disabled mid-game', () => {
    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    const fixture = TestBed.createComponent(GameModeSelector);
    fixture.detectChanges();

    fixture.componentInstance.selectMode('VsComputer');

    expect(createGame).not.toHaveBeenCalled();
  });
});
