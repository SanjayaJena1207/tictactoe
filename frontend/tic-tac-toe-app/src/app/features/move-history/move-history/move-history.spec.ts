import { TestBed } from '@angular/core/testing';
import { GameState } from '../../../core/models/game-state.model';
import { GameStateStore } from '../../../core/state/game-state.store';
import { MoveHistory } from './move-history';

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

describe('MoveHistory', () => {
  let store: GameStateStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [MoveHistory] }).compileComponents();
    store = TestBed.inject(GameStateStore);
  });

  it('shows no rows before a game exists', () => {
    const fixture = TestBed.createComponent(MoveHistory);
    fixture.detectChanges();
    expect(fixture.componentInstance.moves()).toEqual([]);
  });

  it('renders the store moveHistory as Move # / Player / Position rows', () => {
    store.setGameState(
      makeGame({
        moveHistory: [
          { moveNumber: 1, player: 'X', cellIndex: 4, timestamp: '2026-01-01T00:00:00Z' },
          { moveNumber: 2, player: 'O', cellIndex: 8, timestamp: '2026-01-01T00:00:01Z' },
        ],
      }),
    );
    const fixture = TestBed.createComponent(MoveHistory);
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('1');
    expect(rows[0].textContent).toContain('X');
    expect(rows[0].textContent).toContain('Row 2, Column 2');
    expect(rows[1].textContent).toContain('Row 3, Column 3');
  });

  it('updates automatically when the store signal changes, without any API call', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(MoveHistory);
    fixture.detectChanges();
    expect(fixture.componentInstance.moves().length).toBe(0);

    store.setGameState(
      makeGame({ moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 0, timestamp: '2026-01-01T00:00:00Z' }] }),
    );
    fixture.detectChanges();
    expect(fixture.componentInstance.moves().length).toBe(1);
  });
});
