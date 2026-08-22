import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Scoreboard } from '../models/scoreboard.model';

@Injectable({ providedIn: 'root' })
export class ScoreboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/scoreboard`;

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(this.baseUrl);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/reset`, {});
  }
}
