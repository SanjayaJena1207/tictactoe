# Architecture

> Placeholder — recorded at commit 1 so the intended structure is tracked.
> Details will be filled in as the implementation lands.

## Overview

Two independently buildable parts under one repo:

```
backend/    ASP.NET Core Web API (.NET 10)
frontend/   Angular SPA (Angular CLI, latest)
docs/       Architecture and assessment notes
```

The frontend talks to the backend over a small JSON/HTTP API. No shared build step —
each side is scaffolded and built with its own tooling.

## Backend — layered

Four projects in one solution, dependencies pointing inward only:

| Layer | Responsibility | Depends on |
| --- | --- | --- |
| `Domain` | Game entities, value objects, rules (board, move, win/draw detection). No framework references. | — |
| `Application` | Use cases / orchestration: start game, apply move, query state. Defines interfaces it needs from Infrastructure. | Domain |
| `Infrastructure` | Implementations of Application interfaces — persistence, external services. | Application, Domain |
| `Api` | ASP.NET Core host: controllers/endpoints, DI wiring, serialization, error handling. | Application, Infrastructure (composition root only) |

Rules of thumb:

- Domain holds the game logic and stays free of I/O.
- Application never references Api; Api is the only entry point.
- Infrastructure is referenced by Api solely to register implementations at startup.

## Frontend — feature folders

```
src/app/
  core/        Singletons: HTTP clients, interceptors, app-wide services
  shared/      Reusable presentational components, pipes, directives
  features/
    game/      The Tic Tac Toe feature — components, models, feature service, routes
```

Rules of thumb:

- Each feature folder owns its components, state, and routing; features do not import
  from each other, only from `shared/` and `core/`.
- API access is confined to typed services; components stay presentational where practical.

## Open decisions

- Persistence: in-memory vs. durable store.
- Game state ownership: server-authoritative vs. client-side with server validation.
- Test strategy and coverage targets for each side.
