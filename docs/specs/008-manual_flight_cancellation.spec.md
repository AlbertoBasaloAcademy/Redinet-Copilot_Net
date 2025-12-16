# 008-manual_flight_cancellation Specification

## 1. 👔 Problem Specification

AstroBookings needs a workshop/admin operation to manually cancel a flight. Cancelling a flight must:

- Prevent the flight from being performed.
- Ensure the flight state becomes `CANCELLED`.
- Trigger a cancellation notification/refund workflow for any existing bookings (workshop-friendly: simulated via logs).
- Be safe to call multiple times (idempotent).

### Cancel a Flight (Admin Operation)

- **As a** trainer/admin
- **I want to** cancel a flight by id
- **So that** no further operations treat it as an active flight

### Trigger Cancellation Workflows (Workshop-Friendly)

- **As a** trainer/admin
- **I want to** see that cancellations trigger notifications/refunds for existing bookings
- **So that** the workshop can validate cancellation behavior without external providers

## 2. 🧑‍💻 Solution Overview

Implement flight cancellation as a Minimal API endpoint that calls a business operation which:

- Serializes operations per flight to avoid races.
- Applies a small, explicit state-transition rule set.
- Logs a notification/refund workflow trigger (as the demo implementation).

### Data Models

- **Flight**
  - Fields used: `Id`, `State`, `RocketId`, `LaunchDate`, `BasePrice`, `MinimumPassengers`.
  - State change: `State` is set to `CANCELLED`.

- **Booking**
  - Fields used indirectly: bookings are queried by `FlightId` to determine if workflows should be triggered and to log `BookingCount`.
  - No booking data is modified by this feature (refunds are simulated).

- **Response DTO**
  - `FlightResponseDto` is returned from the cancellation endpoint.

### Software Components

- **Presentation**
  - Endpoint: `POST /flights/{flightId}/cancel`
  - Maps business outcomes to HTTP:
    - Success → `200 OK` with `FlightResponseDto`
    - Flight not found → `404 Not Found`
    - Conflict (invalid transition) → `409 Conflict` with `{ "error": "..." }`

- **Business**
  - `FlightService.CancelAsync(flightId)`
    - Validates `flightId`.
    - Acquires a per-flight async gate (`IFlightOperationGate`) to serialize cancel/perform/booking-driven state changes.
    - Enforces transition rules:
      - If `State == CANCELLED`: return success (idempotent no-op).
      - If `State == DONE`: return conflict.
      - Otherwise: set `State = CANCELLED` and persist.
    - Logs:
      - A state transition entry (`FromState` → `CANCELLED`) including booking count.
      - A “notification/refund workflow triggered” entry including booking count.

- **Persistence**
  - `IFlightRepository.GetByIdAsync` and `UpdateAsync` to load and persist the flight state.
  - `IBookingRepository.CountByFlightIdAsync` to obtain booking count for workflow logging.

### User Interface

- No UI screens.
- HTTP interface only:
  - `POST /flights/{flightId}/cancel`
  - No request body.

### Aspects

- **Idempotency**
  - Re-cancelling an already cancelled flight returns success and must not produce duplicate workflow triggers.

- **Error handling**
  - Not found: unknown/empty `flightId` behaves as “not found”.
  - Conflict: cancelling a performed flight (`DONE`) is rejected.

- **Observability (workshop-friendly)**
  - Cancellation must emit `Information` logs for:
    - State transition (`FromState` → `CANCELLED`).
    - Notification/refund workflow trigger.

- **Concurrency**
  - Cancellation is serialized per flight using an async gate to avoid state races (e.g., concurrent cancel vs perform).

- **Security**
  - No authentication/authorization in this version (per PRD non-goals). Endpoint is considered a workshop/admin operation.

## 3. 🧑‍⚖️ Acceptance Criteria

### User Story: Cancel a Flight (Admin Operation)

- [ ] WHEN `POST /flights/{flightId}/cancel` is called for an existing flight that is not `DONE`, the system SHALL set the flight state to `CANCELLED` and SHALL return `200 OK` with the updated flight state.
- [ ] IF the flight does not exist, THEN the system SHALL return `404 Not Found`.
- [ ] WHILE a flight is in state `DONE` AND WHEN `POST /flights/{flightId}/cancel` is called, the system SHALL return `409 Conflict`.

### User Story: Trigger Cancellation Workflows (Workshop-Friendly)

- [ ] WHEN a flight transitions to `CANCELLED`, the system SHALL trigger the notification/refund workflow by logging an `Information` message that includes the `flightId` and the number of existing bookings.
- [ ] WHILE a flight is already in state `CANCELLED` AND WHEN `POST /flights/{flightId}/cancel` is called, the system SHALL be idempotent by returning `200 OK` and SHALL NOT trigger the notification/refund workflow again.

> End of Feature Specification for 008-manual_flight_cancellation, last updated 2025-12-16.
