import { Injectable, computed, signal } from '@angular/core';
import { ApiError } from '../models/api-error.model';
import { GameState } from '../models/game-state.model';

@Injectable({ providedIn: 'root' })
export class GameStateStore {
  private readonly stateSignal = signal<GameState | null>(null);
  private readonly errorSignal = signal<ApiError | null>(null);

  readonly gameState = this.stateSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();

  /** True once at least one move has been made in a game that hasn't finished yet. */
  readonly isMidGame = computed(() => {
    const state = this.stateSignal();
    return !!state && state.status === 'InProgress' && state.moveHistory.length > 0;
  });

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
