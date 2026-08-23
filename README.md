# Tic Tac Toe

A browser-based Tic Tac Toe game with an Angular frontend and an ASP.NET Core backend,
supporting two-player and vs-computer modes, undo, move history, and a running scoreboard.

## Tech stack

| Layer | Technology |
| --- | --- |
| Frontend | Angular 22 (standalone components, signals) |
| Backend | ASP.NET Core Web API on .NET 10 |
| Storage | In-memory, process-wide (no database) |
| API style | REST over JSON/HTTP |

## Features implemented

- Two-player (human vs human) mode
- Vs-computer mode, with a rule-based computer opponent (`X` is always human, `O` is always computer)
- Move validation (turn order, cell occupancy, bounds) enforced server-side
- Win and draw detection, with the winning three cells reported to the client
- Undo of the most recent move (single step; in vs-computer mode this undoes the human
  move and the computer's reply together)
- Reset of the current game (fresh board, same game id)
- Move history (player, cell, move number)
- Running scoreboard (X wins / O wins / draws) across games, with a scoreboard reset
- Responsive layout, winning-cell highlighting, and disabled-state handling in the UI

## Architecture summary

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full breakdown.

- **Backend — layered** (`Domain` → `Application` → `Infrastructure`/`Api`): game rules live
  in a framework-free `Domain` project, orchestrated by `Application`, with `Infrastructure`
  and `Api` as replaceable outer layers. This keeps the win/draw/move-validation logic
  testable and reusable independent of ASP.NET Core or the storage mechanism.
- **Frontend — feature folders** (`core/`, `shared/`, `features/game`): each feature owns its
  own components, state, and routing, importing only from `shared/` and `core/`. This keeps
  the game feature self-contained and prevents cross-feature coupling as the app grows.

## Running the backend locally

```bash
cd backend
dotnet restore
dotnet run --project src/TicTacToe.Api
```

The API listens on `http://localhost:5105`.
With the `Development` environment active, Swagger UI is available at
`http://localhost:5105/swagger`.

## Running the frontend locally

```bash
cd frontend/tic-tac-toe-app
npm install
ng serve
```

The app is served at `http://localhost:4200`. It calls the backend at the URL configured in
`src/environments/environment.development.ts` (`apiBaseUrl`) — this must point at the port the
backend is actually running on (`http://localhost:5105/api` by default, matching step above).

## API endpoint summary

Full contract, request/response bodies, and error shapes: [docs/API.md](docs/API.md).

| Method | Route | Body | Success | Failure |
| --- | --- | --- | --- | --- |
| POST | `/api/games` | `{ "mode": "TwoPlayer" \| "VsComputer" }` | `201 Created` → `GameStateDto` | `400` invalid/missing mode |
| GET | `/api/games/{id}` | — | `200 OK` → `GameStateDto` | `404` unknown id |
| POST | `/api/games/{id}/moves` | `{ "player": "X" \| "O", "cellIndex": 0-8 }` | `200 OK` → `GameStateDto` | `400` invalid move, `404` unknown id |
| POST | `/api/games/{id}/undo` | — | `200 OK` → `GameStateDto` | `400` nothing to undo / game finished, `404` unknown id |
| POST | `/api/games/{id}/reset` | — | `200 OK` → fresh `GameStateDto` (same `gameId`) | `404` unknown id |
| GET | `/api/scoreboard` | — | `200 OK` → `ScoreboardDto` | — |
| POST | `/api/scoreboard/reset` | — | `200 OK` → zeroed `ScoreboardDto` | — |

## Running tests

```bash
# Backend
dotnet test backend/TicTacToe.slnx

# Frontend
cd frontend/tic-tac-toe-app
ng test
```

## Design decisions

**Undo vs. scoreboard: Option A — disable undo after game completion.** Once a game reaches
`Won` or `Draw`, `/api/games/{id}/undo` returns `400 GameAlreadyCompleted` instead of reverting
the result. We chose this over allowing undo-after-completion because it keeps the scoreboard's
invariant simple — a completed game has recorded exactly one outcome, once — with no risk of
double-counting or retracting a tally entry. Reopening a finished game to a lower-stakes,
harder-to-reason-about state also isn't a real requirement here: a player who wants to try
again has `reset` available.

## Clarifications and assumptions

- **Game id is stable across Reset**: `POST /api/games/{id}/reset` returns a fresh board under
  the *same* `gameId` rather than minting a new game — reset is "clear the board", not "start a
  new game".
- **Moves are addressed by `cellIndex` (0-8), not row/col**: the wire format flattens the 3x3
  board to a single index; row/column is a presentation concern only (see
  `formatCellPosition` in the frontend's move-history feature).
- **Single in-memory process, not persisted**: all game and scoreboard state lives in memory
  for the lifetime of the API process. Restarting the API clears every game and the scoreboard;
  there is no database.
- **`X` is always the human, `O` is always the computer** in vs-computer mode — this isn't
  configurable via the API.
- **A vs-computer undo is one step for both plies**: undoing after the computer has replied
  rolls back the human move and the computer's reply together, so the human is always the one
  "to move" again after an undo (except immediately after game start, where nothing has moved
  yet).
- **CORS is scoped to the Angular dev origin** (`http://localhost:4200` by default), read from
  `Cors:AllowedOrigins` in `appsettings.json` — not left wide open.

## Known limitations

- **No persistence across restarts.** All state is in-memory; restarting the backend process
  clears every in-progress game and the scoreboard. A durable option (e.g. SQLite) would be a
  follow-up, not a rewrite, given the layered backend.
- **No authentication or multi-user session separation** beyond the per-game `gameId`. Anyone
  who knows a `gameId` can act on that game; there's no user accounts or access control.
- **The computer opponent is rule-based, not a full minimax search** — it plays by a fixed
  priority order (win-if-possible, block-if-necessary, take-center, take-corner, etc.) rather
  than searching the full game tree. It is therefore beatable and not perfectly optimal. This
  is a deliberate, defensible trade-off given the assignment's priority-based spec for the
  computer opponent, not an oversight.

## Future improvements

- SQLite (or similar) persistence so games and the scoreboard survive a restart
- A minimax-based "unbeatable" AI mode, selectable alongside the current rule-based opponent
- Real-time multiplayer over WebSockets instead of same-device two-player
- A Dockerized run (single `docker compose up` for both frontend and backend)
- A CI pipeline (build + test on push/PR for both projects)

## AI tools and prompt summary

This project was built with Claude Code as a pair-programming tool. See
[docs/AI_PROMPTS.md](docs/AI_PROMPTS.md) for a phase-by-phase summary of what was asked of it
and the accompanying manual-review notes.
