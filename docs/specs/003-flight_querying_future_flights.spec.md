# 003-flight_querying_future_flights Specification

## 1. 👔 Problem Specification

The API must allow API consumers to list only future Flights and optionally filter them by flight state. This supports workshop scenarios where clients need to browse upcoming flights without seeing past launches.

### List Future Flights

- **As a** client app (API consumer)
- **I want to** list only flights with a `launchDate` in the future
- **So that** I can show upcoming flights to users

### Filter Future Flights by State

- **As a** client app (API consumer)
- **I want to** filter the list of future flights by `state`
- **So that** I can show only flights in a specific lifecycle state (e.g., `SCHEDULED`)

## 2. 🧑‍💻 Solution Overview

Implement flight querying as a read-only REST endpoint following the existing 3-layer structure (Presentation → Business → Persistence). Filtering must be deterministic by using injected time (`TimeProvider`) rather than direct calls to `DateTime.UtcNow`.

### Data Models

- **Domain model**: `Flight`
  - `Id` (string)
  - `RocketId` (string)
  - `LaunchDate` (DateTimeOffset)
  - `BasePrice` (decimal)
  - `MinimumPassengers` (int)
  - `State` (`FlightState`)

- **Domain enum**: `FlightState`
  - `SCHEDULED`, `CONFIRMED`, `SOLD_OUT`, `CANCELLED`, `DONE`

- **DTOs**
  - Response: `FlightResponseDto`
    - includes at least `id`, `rocketId`, `launchDate`, `basePrice`, `minimumPassengers`, `state`

### Software Components

- **Presentation** (`lib/Presentation`)
  - Extend `FlightEndpoints` to map `GET /flights`.
  - Accept an optional `state` query parameter (string).
  - Map business outcomes to HTTP status codes (`200`, `400`).

- **Business** (`lib/Business`)
  - Extend `FlightService` with a read method (e.g., `ListFutureFlights`) that:
    - Gets current time from injected `TimeProvider`.
    - Returns only flights whose `LaunchDate` is strictly greater than "now".
    - Applies optional state filtering.
    - Validates the provided state filter can be parsed to `FlightState`.

- **Persistence** (`lib/Persistence`)
  - Add a list method to `IFlightRepository` (if not already present) to retrieve all flights.
  - `InMemoryFlightRepository` returns a snapshot list suitable for filtering in business logic.

### User Interface

HTTP/JSON endpoints (Minimal APIs):

- `GET /flights`
  - Returns future flights only.
  - Supports optional query: `?state=SCHEDULED` (case-insensitive parsing).

### Aspects

- **Validation**: invalid `state` query values return `400 Bad Request`.
- **Deterministic time**: business logic uses injected `TimeProvider` to determine "future".
- **Error handling**: do not throw for expected validation errors; return explicit business results.
- **Logging**: log invalid filter values at `Warning` and successful list calls at `Information` (workshop-friendly).
- **Security**: no authentication/authorization for this workshop version.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client calls `GET /flights`, the system SHALL return `200 OK` with only flights whose `launchDate` is in the future.
- [ ] WHEN a client calls `GET /flights?state={state}`, the system SHALL return `200 OK` with only flights whose `launchDate` is in the future AND whose `state` matches the filter.
- [ ] IF a client calls `GET /flights?state={state}` with an invalid state value, THEN the system SHALL return `400 Bad Request`.

> End of Feature Specification for 003-flight_querying_future_flights, last updated 2025-12-16.
