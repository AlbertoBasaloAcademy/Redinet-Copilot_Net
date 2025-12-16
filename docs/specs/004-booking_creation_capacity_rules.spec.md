# 004-booking_creation_capacity_rules Specification

## 1. 👔 Problem Specification

The API must allow API consumers to create Bookings for a Flight while enforcing strict capacity and flight-state rules. Bookings capture passenger identity and reserve a seat on a specific flight.

### Create Booking for a Flight

- **As a** client app (API consumer)
- **I want to** create a Booking for a specific Flight
- **So that** I can reserve a seat for a passenger

### Prevent Booking When Flight Is Not Bookable

- **As a** client app (API consumer)
- **I want to** be prevented from creating a Booking when the Flight is `CANCELLED` or `SOLD_OUT`
- **So that** I cannot book into invalid flight states

### Enforce Rocket Capacity and Sold-Out Transition

- **As a** client app (API consumer)
- **I want to** be prevented from exceeding Rocket capacity, and have the Flight become `SOLD_OUT` when the last seat is taken
- **So that** capacity is enforced consistently under concurrent requests

## 2. 🧑‍💻 Solution Overview

Implement booking creation as a workshop-friendly REST feature following the existing 3-layer structure (Presentation → Business → Persistence) with in-memory, thread-safe persistence.

### Data Models

- **Domain model**: `Booking`
  - `Id` (string, assigned by repository; deterministic sequential IDs like `b0001`)
  - `FlightId` (string, required)
  - `PassengerName` (string, required)
  - `PassengerEmail` (string, required)

Notes:
- Booking pricing and discount calculation (e.g., `finalPrice`) are intentionally out of scope for this feature and covered by a separate requirement.

- **Domain model**: `Flight` (existing)
  - `Id`, `RocketId`, `LaunchDate`, `MinimumPassengers`, `State`

- **Domain model**: `Rocket` (existing)
  - `Id`, `Capacity`

- **DTOs**
  - Create request: `CreateBookingDto`
    - `passengerName`, `passengerEmail`
  - Response: `BookingResponseDto`
    - `id`, `flightId`, `passengerName`, `passengerEmail`

### Software Components

- **Presentation** (`lib/Presentation`)
  - Add `BookingEndpoints` to map booking routes and convert DTOs ↔ domain models.
  - Prefer a flight-scoped route for simplicity and alignment with later querying:
    - `POST /flights/{flightId}/bookings`
  - Map Business outcomes to HTTP status codes (`201`, `400`, `404`, `409`).

- **Business** (`lib/Business`)
  - Add `BookingService` (or extend an existing service) that:
    - Validates input (`passengerName`/`passengerEmail` required).
    - Loads the target Flight; returns not-found if missing.
    - Enforces state rule: bookings are rejected when Flight is `CANCELLED` or `SOLD_OUT`.
    - Enforces capacity rule using Rocket capacity and current booking count.
    - Creates the Booking and persists it.
    - Transitions Flight to `SOLD_OUT` when the last seat is taken.
  - Uses explicit result types for expected outcomes (validation failure, not found, conflict) rather than throwing.

- **Persistence** (`lib/Persistence`)
  - Add `IBookingRepository` and `InMemoryBookingRepository`:
    - Store bookings by id and by `flightId` (index) using thread-safe collections.
    - Provide operations needed by business logic:
      - `Add(Booking booking)`
      - `CountByFlightId(string flightId)`
      - (Optional for later features) `ListByFlightId(string flightId)`
  - Ensure atomicity of “capacity check + add booking + potential sold-out transition”:
    - Simplest approach: a per-flight lock in business layer (e.g., `ConcurrentDictionary<string, object>` lock objects), or a repository method that performs an atomic add with capacity guard.

### User Interface

HTTP/JSON endpoints (Minimal APIs):

- `POST /flights/{flightId}/bookings`
  - Request: passenger name/email
  - Response: created booking
  - Error responses: validation (`400`), missing flight (`404`), booking conflicts (`409`)

### Aspects

- **Validation**:
  - Require `passengerName` and `passengerEmail`.
  - Validate `flightId` exists.

- **Error handling / Status codes**:
  - `400 Bad Request` for malformed/invalid payload.
  - `404 Not Found` when the referenced Flight does not exist.
  - `409 Conflict` when the Flight is not bookable (`CANCELLED`/`SOLD_OUT`) or capacity would be exceeded.

- **Logging**:
  - Log successful booking creation at `Information`.
  - Log rejected booking attempts (state/capacity) at `Warning`.
  - Log flight state transition to `SOLD_OUT` at `Information`.

- **Concurrency**:
  - Ensure the system never exceeds Rocket capacity even under concurrent booking requests.

- **Security**:
  - No authentication/authorization for this workshop version.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client submits `POST /flights/{flightId}/bookings` with missing `passengerName` or missing `passengerEmail`, the system SHALL return `400 Bad Request`.
- [ ] IF a client submits `POST /flights/{flightId}/bookings` for a non-existent `flightId`, THEN the system SHALL return `404 Not Found`.

- [ ] IF a client submits `POST /flights/{flightId}/bookings` and the Flight is `CANCELLED` or `SOLD_OUT`, THEN the system SHALL return `409 Conflict` and SHALL NOT create a Booking.
- [ ] IF creating a Booking would exceed the Rocket capacity for the Flight, THEN the system SHALL return `409 Conflict` and SHALL NOT create a Booking.

- [ ] WHEN a client creates a Booking that consumes the last available seat for a Flight, the system SHALL transition the Flight state to `SOLD_OUT`.
- [ ] WHEN a client submits `POST /flights/{flightId}/bookings` with valid input for a bookable Flight and available capacity, the system SHALL create a Booking and return `201 Created` with the created Booking representation.

> End of Feature Specification for 004-booking_creation_capacity_rules, last updated 2025-12-16.
