import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { GameState } from '../../../core/models/game-state.model';
import { GameService } from '../../../core/services/game.service';
import { GameStateStore } from '../../../core/state/game-state.store';
import { GameBoard } from './game-board';

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

describe('GameBoard', () => {
  let store: GameStateStore;
  let applyMove: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    applyMove = vi.fn().mockReturnValue(of(makeGame({})));

    await TestBed.configureTestingModule({
      imports: [GameBoard],
      providers: [{ provide: GameService, useValue: { applyMove } }],
    }).compileComponents();

    store = TestBed.inject(GameStateStore);
  });

  it('renders nothing to click before a game exists', () => {
    const fixture = TestBed.createComponent(GameBoard);
    fixture.detectChanges();
    const cells = fixture.nativeElement.querySelectorAll('app-game-cell');
    expect(cells.length).toBe(0);
  });

  it('renders nine cells from the store board', () => {
    store.setGameState(makeGame({}));
    const fixture = TestBed.createComponent(GameBoard);
    fixture.detectChanges();
    const cells = fixture.nativeElement.querySelectorAll('app-game-cell');
    expect(cells.length).toBe(9);
  });

  it('calls GameService.applyMove with the game id, current player and cell index on click', () => {
    store.setGameState(makeGame({ currentPlayer: 'O' }));
    const fixture = TestBed.createComponent(GameBoard);
    fixture.detectChanges();

    fixture.componentInstance.onCellClick(3);

    expect(applyMove).toHaveBeenCalledWith('g1', 'O', 3);
  });

  it('highlights the winning cells reported by the store', () => {
    store.setGameState(
      makeGame({ status: 'Won', winner: 'X', winningCells: [0, 1, 2], board: ['X', 'X', 'X', null, null, null, null, null, null] }),
    );
    const fixture = TestBed.createComponent(GameBoard);
    fixture.detectChanges();
    expect(fixture.componentInstance.winningCells().has(1)).toBe(true);
    expect(fixture.componentInstance.winningCells().has(5)).toBe(false);
  });
});
