import { TestBed } from '@angular/core/testing';
import { ApiError } from '../models/api-error.model';
import { GameState } from '../models/game-state.model';
import { GameStateStore } from './game-state.store';

function makeGame(overrides: Partial<GameState> = {}): GameState {
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

describe('GameStateStore', () => {
  let store: GameStateStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    store = TestBed.inject(GameStateStore);
  });

  it('starts with no game state and no error', () => {
    expect(store.gameState()).toBeNull();
    expect(store.error()).toBeNull();
    expect(store.isMidGame()).toBe(false);
  });

  it('setGameState updates the gameState signal', () => {
    const game = makeGame({ currentPlayer: 'O' });

    store.setGameState(game);

    expect(store.gameState()).toEqual(game);
  });

  it('setGameState overwrites a previously set game state', () => {
    store.setGameState(makeGame({ currentPlayer: 'X' }));
    const updated = makeGame({ currentPlayer: 'O', board: ['X', null, null, null, null, null, null, null, null] });

    store.setGameState(updated);

    expect(store.gameState()).toEqual(updated);
  });

  it('setGameState clears any previous error', () => {
    store.setError({ status: 400, title: 'Invalid game action', detail: 'Cell is already occupied.' });
    expect(store.error()).not.toBeNull();

    store.setGameState(makeGame());

    expect(store.error()).toBeNull();
  });

  it('setError updates the error signal without touching gameState', () => {
    store.setGameState(makeGame());
    const error: ApiError = { status: 404, title: 'Game not found', detail: "Game 'g1' was not found." };

    store.setError(error);

    expect(store.error()).toEqual(error);
    expect(store.gameState()?.gameId).toBe('g1');
  });

  it('clear resets both gameState and error to null', () => {
    store.setGameState(makeGame());
    store.setError({ status: 400, title: 'x', detail: 'y' });

    store.clear();

    expect(store.gameState()).toBeNull();
    expect(store.error()).toBeNull();
  });

  describe('isMidGame', () => {
    it('is false when there is no game', () => {
      expect(store.isMidGame()).toBe(false);
    });

    it('is false when the game is in progress but no moves have been made', () => {
      store.setGameState(makeGame({ moveHistory: [] }));

      expect(store.isMidGame()).toBe(false);
    });

    it('is true once a move has been made and the game is still in progress', () => {
      store.setGameState(
        makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
      );

      expect(store.isMidGame()).toBe(true);
    });

    it('is false once the game has finished (Won), even with move history', () => {
      store.setGameState(
        makeGame({
          status: 'Won',
          winner: 'X',
          moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }],
        }),
      );

      expect(store.isMidGame()).toBe(false);
    });

    it('is false once the game has finished (Draw), even with move history', () => {
      store.setGameState(
        makeGame({
          status: 'Draw',
          moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }],
        }),
      );

      expect(store.isMidGame()).toBe(false);
    });
  });
});
