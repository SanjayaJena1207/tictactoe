import { Injectable, signal } from '@angular/core';
import { ApiError } from '../models/api-error.model';
import { GameState } from '../models/game-state.model';

@Injectable({ providedIn: 'root' })
export class GameStateStore {
  private readonly stateSignal = signal<GameState | null>(null);
  private readonly errorSignal = signal<ApiError | null>(null);

  readonly gameState = this.stateSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();

  setGameState(state: GameState): void {
    this.stateSignal.set(state);
    this.errorSignal.set(null);
  }

  setError(error: ApiError): void {
    this.errorSignal.set(error);
  }

  clear(): void {
    this.stateSignal.set(null);
    this.errorSignal.set(null);
  }
}
