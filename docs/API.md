# API Contract

Base URL (local dev): `http://localhost:5105` (see `backend/src/TicTacToe.Api/Properties/launchSettings.json`
for the exact port; an HTTPS profile is also available on `https://localhost:7229`).

All request/response bodies are JSON. All state is in-memory and process-wide — restarting
the API clears every game and the scoreboard.

Interactive docs: run the API and open `/swagger` (Development environment only) for a
live, XML-comment-annotated Swagger UI covering every endpoint below.

## Endpoints

| Method | Route | Body | Success | Failure |
| --- | --- | --- | --- | --- |
| POST | `/api/games` | `{ "mode": "TwoPlayer" \| "VsComputer" }` | `201 Created` → `GameStateDto` | `400` invalid/missing mode |
| GET | `/api/games/{id}` | — | `200 OK` → `GameStateDto` | `404` unknown id |
| POST | `/api/games/{id}/moves` | `{ "player": "X" \| "O", "cellIndex": 0-8 }` | `200 OK` → `GameStateDto` | `400` invalid move, `404` unknown id |
| POST | `/api/games/{id}/undo` | — | `200 OK` → `GameStateDto` | `400` nothing to undo / game finished, `404` unknown id |
| POST | `/api/games/{id}/reset` | — | `200 OK` → fresh `GameStateDto` (same `gameId`) | `404` unknown id |
| GET | `/api/scoreboard` | — | `200 OK` → `ScoreboardDto` | — |
| POST | `/api/scoreboard/reset` | — | `200 OK` → zeroed `ScoreboardDto` | — |

Field names in JSON bodies are camelCase (`gameId`, `cellIndex`, `currentPlayer`, ...), matching
the default System.Text.Json serialization of the C# DTOs shown below.

### POST /api/games

Starts a new game.

Request body:

```json
{ "mode": "TwoPlayer" }
```

`mode` must be exactly `"TwoPlayer"` or `"VsComputer"`. In `VsComputer` mode, `X` is always the
human and `O` is always the computer.

Response `201 Created` (also sets `Location: /api/games/{id}`):

```json
{
  "gameId": "d4b86272-b640-43fa-b8e8-457eae7fa98a",
  "board": [null, null, null, null, null, null, null, null, null],
  "currentPlayer": "X",
  "gameMode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": []
}
```

`400 Bad Request` if `mode` is missing or not one of the two allowed values (standard ASP.NET
Core `ValidationProblemDetails`, field errors keyed by `Mode`).

### GET /api/games/{id}

Returns the current state of a game, in the same `GameStateDto` shape shown above.

`404 Not Found` if `id` is not a known game:

```json
{ "title": "Game not found", "status": 404, "detail": "Game '...' was not found." }
```

### POST /api/games/{id}/moves

Applies a move for the given player.

Request body:

```json
{ "player": "X", "cellIndex": 0 }
```

- `player` must be `"X"` or `"O"`.
- `cellIndex` must be an integer `0`-`8`.

Response `200 OK` → updated `GameStateDto`. In `VsComputer` mode, if the human's move doesn't
end the game, the computer's reply move is applied automatically before the response is
returned — the response reflects both moves.

`400 Bad Request` for any invalid move. Body is a `ProblemDetails` with a machine-readable
`reason` extension field:

```json
{
  "title": "Invalid game action",
  "status": 400,
  "detail": "Cell is already occupied.",
  "reason": "CellOccupied"
}
```

`reason` is one of:

| Reason | Meaning |
| --- | --- |
| `CellOccupied` | The target cell is already filled. |
| `CellIndexOutOfRange` | `cellIndex` is outside `0`-`8` (also caught earlier by model validation, which returns the standard `ValidationProblemDetails` shape instead). |
| `NotPlayersTurn` | It isn't the requesting player's turn. |
| `GameAlreadyCompleted` | The game has already finished (won or drawn). |
| `ValidationFailed` | Fallback for any other rejected move. |

Malformed `player`/`cellIndex` values (wrong type, `player` not `"X"`/`"O"`, `cellIndex` out
of `0`-`8`) are rejected by model validation before reaching game logic, returning the
standard ASP.NET Core `ValidationProblemDetails` shape instead of the `reason` shape above.

`404 Not Found` if `id` is not a known game.

### POST /api/games/{id}/undo

Undoes the most recent move. In `VsComputer` mode this undoes the computer's reply together
with the human move that triggered it, as a single step (except when only the human has moved
so far, where just that one move is undone).

Response `200 OK` → updated `GameStateDto`.

`400 Bad Request` with a `reason` of `NoMovesToUndo` (no moves yet) or `GameAlreadyCompleted`
(game already finished — undo is only allowed while a game is in progress).

`404 Not Found` if `id` is not a known game.

### POST /api/games/{id}/reset

Resets a game to a fresh, empty board, keeping the same `gameId`.

Response `200 OK` → fresh `GameStateDto`.

`404 Not Found` if `id` is not a known game.

### GET /api/scoreboard

Returns the running win/draw tally, accumulated across every completed game since the process
started (or since the last scoreboard reset).

```json
{ "xWins": 0, "oWins": 0, "draws": 0 }
```

### POST /api/scoreboard/reset

Zeroes the scoreboard and returns it.

```json
{ "xWins": 0, "oWins": 0, "draws": 0 }
```

## DTO shapes

```csharp
record GameStateDto(
    Guid GameId,
    string?[] Board,             // 9 cells, each null, "X", or "O"
    string CurrentPlayer,        // "X" | "O"
    string GameMode,             // "TwoPlayer" | "VsComputer"
    string Status,               // "InProgress" | "Won" | "Draw"
    string? Winner,              // "X" | "O" | null
    IReadOnlyList<int>? WinningCells,  // the 3 winning cell indices, or null
    IReadOnlyList<MoveDto> MoveHistory);

record MoveDto(int MoveNumber, string Player, int CellIndex, DateTimeOffset Timestamp);

record ScoreboardDto(int XWins, int OWins, int Draws);
```

## Cross-cutting behavior

- **Unhandled exceptions** anywhere in the pipeline are caught by a global exception handler,
  logged at Error level, and returned as a generic `500` `ProblemDetails` body — no stack
  traces are ever sent to the client.
- **CORS**: the allowed origin is read from configuration (`Cors:AllowedOrigins` in
  `appsettings.json`), defaulting to `http://localhost:4200` for the Angular dev server.
- **Logging**: each game action (create, move, undo, reset, scoreboard reset) is logged at
  Information level via the built-in `ILogger`.
