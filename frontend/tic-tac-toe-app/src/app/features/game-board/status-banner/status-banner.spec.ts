import { TestBed } from '@angular/core/testing';
import { GameState } from '../../../core/models/game-state.model';
import { GameStateStore } from '../../../core/state/game-state.store';
import { StatusBanner } from './status-banner';

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

describe('StatusBanner', () => {
  let store: GameStateStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusBanner],
    }).compileComponents();
    store = TestBed.inject(GameStateStore);
  });

  it('shows nothing before a game exists', () => {
    const fixture = TestBed.createComponent(StatusBanner);
    fixture.detectChanges();
    expect(fixture.componentInstance.message()).toBe('');
  });

  it('shows whose turn it is while in progress', () => {
    store.setGameState(makeGame({ currentPlayer: 'O' }));
    const fixture = TestBed.createComponent(StatusBanner);
    fixture.detectChanges();
    expect(fixture.componentInstance.message()).toBe("Player O's turn");
  });

  it('announces the winner when won', () => {
    store.setGameState(makeGame({ status: 'Won', winner: 'X', winningCells: [0, 1, 2] }));
    const fixture = TestBed.createComponent(StatusBanner);
    fixture.detectChanges();
    expect(fixture.componentInstance.message()).toBe('Player X wins!');
  });

  it('announces a draw', () => {
    store.setGameState(makeGame({ status: 'Draw' }));
    const fixture = TestBed.createComponent(StatusBanner);
    fixture.detectChanges();
    expect(fixture.componentInstance.message()).toBe("It's a draw!");
  });
});
