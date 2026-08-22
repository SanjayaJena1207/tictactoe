import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Scoreboard } from '../models/scoreboard.model';
import { ScoreboardStore } from '../state/scoreboard.store';

@Injectable({ providedIn: 'root' })
export class ScoreboardService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(ScoreboardStore);
  private readonly baseUrl = `${environment.apiBaseUrl}/scoreboard`;

  getScoreboard(): Observable<Scoreboard> {
    return this.request(this.http.get<Scoreboard>(this.baseUrl));
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.request(this.http.post<Scoreboard>(`${this.baseUrl}/reset`, {}));
  }

  private request(source: Observable<Scoreboard>): Observable<Scoreboard> {
    return source.pipe(tap((scoreboard) => this.store.setScoreboard(scoreboard)));
  }
}
