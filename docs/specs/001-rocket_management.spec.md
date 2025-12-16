# 001-rocket_management Specification

## 1. 👔 Problem Specification

The API must allow workshop users and API consumers to create and query Rockets. Rockets represent the vehicle that determines capacity and range for Flights.

### Create Rocket

- **As a** trainer/student (API operator)
- **I want to** create a Rocket with validated fields
- **So that** I can set up demo data for flights and bookings

### List Rockets

- **As a** client app (API consumer)
- **I want to** list all Rockets
- **So that** I can display available rockets to choose from

### Get Rocket by Id

- **As a** client app (API consumer)
- **I want to** fetch a single Rocket by id
- **So that** I can view details and validate that a referenced rocket exists

## 2. 🧑‍💻 Solution Overview

Implement Rocket Management as a small, workshop-friendly REST feature using the existing 3-layer structure (Presentation → Business → Persistence) and in-memory storage.

### Data Models

- **Domain model**: `Rocket`
  - `Id` (string or GUID-like identifier)
  - `Name` (required)
  - `Capacity` (required, max 10)
  - `Speed` (optional)
  - `Range` (optional enum: `LEO`, `MOON`, `MARS`)
- **DTOs**
  - `RocketDto` (request payload for create)
  - `RocketResponseDto` (response payload)

### Software Components

- **Presentation** (`lib/Presentation`)
  - `RocketEndpoints` maps routes for create/list/get and converts DTOs ↔ domain models.
  - Maps Business outcomes to HTTP status codes (`201`, `200`, `400`, `404`).
- **Business** (`lib/Business`)
  - `RocketService` validates inputs (required fields, capacity limit, allowed range) and orchestrates repository calls.
  - Returns explicit results for expected outcomes (validation failure, not found).
- **Persistence** (`lib/Persistence`)
  - `InMemoryRocketRepository` stores rockets in a thread-safe structure and supports add/list/get-by-id.

### User Interface

HTTP/JSON endpoints (Minimal APIs):

- `POST /rockets` to create a Rocket.
- `GET /rockets` to list Rockets.
- `GET /rockets/{id}` to get a Rocket by id.

### Aspects

- **Validation**: enforce PRD rules (required `name`/`capacity`, `capacity ≤ 10`, `range ∈ {LEO, MOON, MARS}` when provided).
- **Error handling**: validation errors return `400`; missing rocket returns `404`.
- **Logging**: log create events and validation failures using `ILogger<T>` (workshop-friendly).
- **Concurrency**: repository operations are safe for concurrent requests.
- **Security**: no authentication/authorization for this workshop version.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client submits `POST /rockets` with a missing `name` or missing `capacity`, the system SHALL return `400 Bad Request`.
- [ ] WHEN a client submits `POST /rockets` with `capacity` greater than `10`, the system SHALL return `400 Bad Request`.
- [ ] IF a client submits `POST /rockets` with `range` not in `{LEO, MOON, MARS}`, THEN the system SHALL return `400 Bad Request`.
- [ ] WHEN a client submits `POST /rockets` with valid input, the system SHALL create a Rocket and return `201 Created` with the created Rocket representation.
- [ ] WHEN a client calls `GET /rockets`, the system SHALL return `200 OK` with the list of all Rockets.
- [ ] WHEN a client calls `GET /rockets/{id}` for an existing Rocket, the system SHALL return `200 OK` with that Rocket.
- [ ] WHEN a client calls `GET /rockets/{id}` for a non-existent Rocket, the system SHALL return `404 Not Found`.

> End of Feature Specification for 001-rocket_management, last updated 2025-12-16.
