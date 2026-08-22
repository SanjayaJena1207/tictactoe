import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { errorInterceptor } from '../interceptors/error.interceptor';
import { GameState } from '../models/game-state.model';
import { GameStateStore } from '../state/game-state.store';
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
  let store: GameStateStore;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([errorInterceptor])), provideHttpClientTesting()],
    });
    service = TestBed.inject(GameService);
    store = TestBed.inject(GameStateStore);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('createGame', () => {
    it('POSTs to /games with the requested mode and maps the response into the store', () => {
      let result: GameState | undefined;
      service.createGame('VsComputer').subscribe((state) => (result = state));

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/games`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ mode: 'VsComputer' });

      const game = makeGame({ gameMode: 'VsComputer' });
      req.flush(game);

      expect(result).toEqual(game);
      expect(store.gameState()).toEqual(game);
    });
  });

  describe('getGame', () => {
    it('GETs /games/{id} and maps the response into the store', () => {
      let result: GameState | undefined;
      service.getGame('g1').subscribe((state) => (result = state));

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1`);
      expect(req.request.method).toBe('GET');

      const game = makeGame({ currentPlayer: 'O' });
      req.flush(game);

      expect(result).toEqual(game);
      expect(store.gameState()).toEqual(game);
    });
  });

  describe('applyMove', () => {
    it('POSTs to /games/{id}/moves with the player and cellIndex, and maps the response into the store', () => {
      let result: GameState | undefined;
      service.applyMove('g1', 'X', 4).subscribe((state) => (result = state));

      const moveReq = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/moves`);
      expect(moveReq.request.method).toBe('POST');
      expect(moveReq.request.body).toEqual({ player: 'X', cellIndex: 4 });

      const game = makeGame({
        board: [null, null, null, null, 'X', null, null, null, null],
        currentPlayer: 'O',
        moveHistory: [{ moveNumber: 1, player: 'X', cellIndex: 4, timestamp: '2026-01-01T00:00:00Z' }],
      });
      moveReq.flush(game);

      expect(result).toEqual(game);
      expect(store.gameState()).toEqual(game);

      httpMock.expectOne(`${environment.apiBaseUrl}/scoreboard`).flush({ xWins: 0, oWins: 0, draws: 0 });
    });

    it('refetches the scoreboard after a successful move', () => {
      service.applyMove('g1', 'X', 0).subscribe();

      const moveReq = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/moves`);
      moveReq.flush(makeGame({ board: ['X', null, null, null, null, null, null, null, null], currentPlayer: 'O' }));

      httpMock.expectOne(`${environment.apiBaseUrl}/scoreboard`).flush({ xWins: 0, oWins: 0, draws: 0 });
    });

    it('does not refetch the scoreboard when the move is rejected, but still records the error in the store', () => {
      let error: unknown;
      service.applyMove('g1', 'X', 0).subscribe({ error: (err) => (error = err) });

      const moveReq = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/moves`);
      moveReq.flush(
        { title: 'Invalid game action', status: 400, detail: 'Cell is already occupied.', reason: 'CellOccupied' },
        { status: 400, statusText: 'Bad Request' },
      );

      httpMock.expectNone(`${environment.apiBaseUrl}/scoreboard`);
      expect(error).toBeTruthy();
      expect(store.error()?.detail).toBe('Cell is already occupied.');
    });
  });

  describe('undo', () => {
    it('POSTs to /games/{id}/undo with an empty body and maps the response into the store', () => {
      let result: GameState | undefined;
      service.undo('g1').subscribe((state) => (result = state));

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/undo`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});

      const game = makeGame({ moveHistory: [] });
      req.flush(game);

      expect(result).toEqual(game);
      expect(store.gameState()).toEqual(game);
    });
  });

  describe('resetGame', () => {
    it('POSTs to /games/{id}/reset with an empty body and maps the response into the store', () => {
      let result: GameState | undefined;
      service.resetGame('g1').subscribe((state) => (result = state));

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/games/g1/reset`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});

      const game = makeGame({});
      req.flush(game);

      expect(result).toEqual(game);
      expect(store.gameState()).toEqual(game);
    });
  });
});
