# AI Tools and Prompt Summary

This project was built with Human and Claude Code - as a pair-programming
tool, working phase by phase through the backend and frontend. Each phase below corresponds to
one feature branch / PR in the commit history.

For every phase,AI generated code was reviewed and modified by Human to achieve mentioned functionality.

---

## Phase 1 — Domain model

**Branch:** `feat/phase-01-domain-model`

**What I asked Claude Code to build:** the framework-free `TicTacToe.Domain` project — the
board representation, win/draw detection, move validation, and the `Game` aggregate that ties them together.

---

## Phase 2 — Application layer

**Branch:** `feat/phase-02-application-layer`

**What I asked Claude Code to build:** the `TicTacToe.Application` project — `GameService`, the
in-memory repository abstraction, and the DTOs used to shuttle state out of the domain layer.

---

## Phase 3 — Undo logic

**Branch:** `feat/phase-03-Undo-logic`

**What I asked Claude Code to build:** undo support for the most recent move, including the
vs-computer case where a human move and the computer's reply need to be undone together as a
single step.

---

## Phase 4 — Computer opponent

**Branch:** `feat/phase-04-Opponent-Computer`

**What I asked Claude Code to build:** a rule-based computer opponent using a priority-ordered
move selection strategy (win if possible, block if necessary, then positional preference),
rather than a full minimax search.


---

## Phase 5 — Scoreboard

**Branch:** `feat/phase-05-Scoreboard`

**What I asked Claude Code to build:** the session-wide scoreboard (X wins / O wins / draws),
accumulated across completed games and resettable independently of any single game.

---

## Phase 6 — API layer

**Branch:** `feat/phase-06-api-layer`

**What I asked Claude Code to build:** the ASP.NET Core `TicTacToe.Api` project — REST
endpoints for games, moves, undo, reset, and the scoreboard, wired to the application layer,
plus global exception handling and Swagger. This phase also included manual smoke testing of
the running API against the acceptance criteria and via Swagger UI.

---

## Phase 7 — Angular workspace

**Branch:** `feat/phase-07-angular-workspace`

**What I asked Claude Code to build:** the Angular workspace scaffold — core models mirroring
the backend DTOs, HTTP services for the game and scoreboard endpoints, and a signal-based game
state store.

---

## Phase 8 — Game board UI

**Branch:** `feat/phase-08-Game-board-ui`

**What I asked Claude Code to build:** the game board feature — the board/cell components, the
game mode selector, cell click interaction wired to the store, and the status banner.

---

## Phase 9 — Move history and scoreboard panel

**Branch:** `feat/phase-09-move-history-score-history`

**What I asked Claude Code to build:** the move history list, the scoreboard panel, and the
undo/reset/reset-scoreboard controls in the frontend.

---

## Phase 10 — Style and responsiveness

**Branch:** `feat/phase-10-style-responsiveness`

**What I asked Claude Code to build:** responsive layout across breakpoints, the winning-cell
highlight styling, and disabled-state styling for controls/cells where actions aren't valid.

---

## Phase 11 — Frontend tests

**Branch:** `feat/phase-11-frontend-test`

**What I asked Claude Code to build:** unit tests for the frontend services, the signal-based
stores, and the feature components.

---

## Phase 12 — README and API documentation

**Branch:** `feat/phase-12-readme-api-doc`

**What I asked Claude Code to build:** the top-level `README.md` and this `AI_PROMPTS.md` file,
pulling accurate details (ports, commands, endpoint table) from the existing
`docs/ARCHITECTURE.md` and `docs/API.md` rather than re-describing the system from scratch. This
phase also included a final pass over the codebase for leftover TODOs, debug statements,
commented-out dead code, and naming inconsistencies.

