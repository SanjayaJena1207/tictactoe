import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiError } from '../models/api-error.model';
import { GameMode } from '../models/game-mode.enum';
import { GameState } from '../models/game-state.model';
import { Player } from '../models/player.enum';
import { GameStateStore } from '../state/game-state.store';
import { ScoreboardService } from './scoreboard.service';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(GameStateStore);
  private readonly scoreboardService = inject(ScoreboardService);
  private readonly baseUrl = `${environment.apiBaseUrl}/games`;

  createGame(mode: GameMode): Observable<GameState> {
    return this.request(this.http.post<GameState>(this.baseUrl, { mode }));
  }

  getGame(id: string): Observable<GameState> {
    return this.request(this.http.get<GameState>(`${this.baseUrl}/${id}`));
  }

  applyMove(gameId: string, player: Player, cellIndex: number): Observable<GameState> {
    return this.request(
      this.http.post<GameState>(`${this.baseUrl}/${gameId}/moves`, { player, cellIndex }),
    ).pipe(tap(() => this.scoreboardService.getScoreboard().subscribe()));
  }

  undo(gameId: string): Observable<GameState> {
    return this.request(this.http.post<GameState>(`${this.baseUrl}/${gameId}/undo`, {}));
  }

  resetGame(gameId: string): Observable<GameState> {
    return this.request(this.http.post<GameState>(`${this.baseUrl}/${gameId}/reset`, {}));
  }

  private request(source: Observable<GameState>): Observable<GameState> {
    return source.pipe(
      tap((state) => this.store.setGameState(state)),
      catchError((error: ApiError) => {
        this.store.setError(error);
        return throwError(() => error);
      }),
    );
  }
}
