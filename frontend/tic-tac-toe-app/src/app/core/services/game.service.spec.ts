import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { GameState } from '../models/game-state.model';
import { GameService } from './game.service';

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

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('refetches the scoreboard after a successful move', () => {
    service.applyMove('g1', 'X', 0).subscribe();

    const moveReq = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/moves`);
    moveReq.flush(makeGame({ board: ['X', null, null, null, null, null, null, null, null], currentPlayer: 'O' }));

    httpMock.expectOne(`${environment.apiBaseUrl}/scoreboard`).flush({ xWins: 0, oWins: 0, draws: 0 });
  });

  it('does not refetch the scoreboard when the move is rejected', () => {
    service.applyMove('g1', 'X', 0).subscribe({ error: () => {} });

    const moveReq = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/moves`);
    moveReq.flush(
      { title: 'Invalid game action', status: 400, detail: 'Cell is already occupied.', reason: 'CellOccupied' },
      { status: 400, statusText: 'Bad Request' },
    );

    httpMock.expectNone(`${environment.apiBaseUrl}/scoreboard`);
  });
});
