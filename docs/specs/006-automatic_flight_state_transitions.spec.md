# 006-automatic_flight_state_transitions Specification

## 1. 👔 Problem Specification

The API must keep Flight state consistent with bookings and operational actions, automatically transitioning states when key thresholds are reached and triggering workshop-friendly workflows (notifications/refunds) via logs or abstractions.

### Auto-confirm Flight When Minimum Passengers Reached

- **As a** client app (API consumer)
- **I want to** have a Flight automatically transition to `CONFIRMED` when the number of bookings reaches the Flight `minimumPassengers`
- **So that** I can reliably show that the flight will run without implementing server-side rules myself

### Auto-sell-out Flight When Capacity Reached

- **As a** client app (API consumer)
- **I want to** have a Flight automatically transition to `SOLD_OUT` when the Rocket capacity is fully booked
- **So that** I can prevent further bookings and communicate availability accurately

### Support Operational Completion and Cancellation Transitions

- **As a** trainer/admin (workshop operator)
- **I want to** mark a Flight as performed (`DONE`) or cancelled (`CANCELLED`)
- **So that** workshop scenarios can exercise state changes and downstream workflows (notifications/refunds)

## 2. 🧑‍💻 Solution Overview

Implement state transitions in the Business layer where booking creation and operational actions are orchestrated. Use in-memory repositories to read current booking counts and update the Flight state. Trigger notification/refund workflows via logging (or simple ports) to keep the workshop surface minimal.

### Data Models

- **Domain model**: `Flight` (existing)
  - `Id` (string)
  - `RocketId` (string)
  - `LaunchDate` (DateTime)
  - `MinimumPassengers` (int)
  - `State` (`FlightState` enum)

- **Domain model**: `Booking` (existing)
  - `FlightId` (string)

- **Domain model**: `Rocket` (existing)
  - `Capacity` (int)

- **Enum**: `FlightState` (existing)
  - `SCHEDULED`, `CONFIRMED`, `SOLD_OUT`, `DONE`, `CANCELLED`

Notes:
- This feature assumes Flight creation sets initial state to `SCHEDULED` (covered by FR2).
- Automatic cancellation based on time is explicitly out of scope (per PRD).

### Software Components

- **Presentation** (`lib/Presentation`)
  - Keep `POST /flights/{flightId}/bookings` as the booking entrypoint; flight state updates happen as a side-effect of successful booking creation.
  - Add simple admin/workshop endpoints for operational transitions (minimal, no auth):
    - `POST /flights/{flightId}/cancel` to cancel a flight.
    - `POST /flights/{flightId}/perform` to mark a flight as done.
  - Map expected Business outcomes to HTTP:
    - `404 Not Found` when flight does not exist.
    - `409 Conflict` when the requested transition is invalid for the current state.

- **Business** (`lib/Business`)
  - Extend `BookingService` booking creation flow to re-evaluate flight state after a booking is persisted.
  - Extend `FlightService` (or add small methods) to support operational transitions.

  State transition rules (simplest interpretation):
  - **Confirm**:
    - After a successful booking, compute `newBookingCount`.
    - If `newBookingCount >= minimumPassengers` AND Flight state is `SCHEDULED`, transition to `CONFIRMED`.
    - Trigger a confirmation notification workflow (workshop-friendly: log at `Information`).
  - **Sold out**:
    - After a successful booking, if `newBookingCount >= rocketCapacity`, transition to `SOLD_OUT`.
    - `SOLD_OUT` may supersede `CONFIRMED` if both conditions become true.
  - **Cancel (manual)**:
    - On `cancel`, set state to `CANCELLED`.
    - Trigger notification and refund workflow for existing bookings (workshop-friendly: log at `Information`).
  - **Done (performed)**:
    - On `perform`, set state to `DONE`.

  Validity rules:
  - Flight state transitions should be idempotent (repeating the same transition should not create duplicate side-effects).
  - Transitions must be rejected for terminal/non-applicable states:
    - No transitions out of `CANCELLED`.
    - No booking-driven transitions once `DONE`.

  Deterministic time:
  - If the implementation needs to validate “performed” relative to time (optional), it SHALL use `TimeProvider` rather than `DateTime.UtcNow`.

- **Persistence** (`lib/Persistence`)
  - Use existing repositories:
    - `IFlightRepository` to get/update Flight.
    - `IBookingRepository` to count bookings by flight.
    - `IRocketRepository` to get Rocket capacity.
  - Ensure concurrency safety so that booking-driven transitions cannot produce an impossible state (e.g., exceeding capacity).

### User Interface

HTTP/JSON behavior (Minimal APIs):

- Booking-driven transitions
  - `POST /flights/{flightId}/bookings`
    - On success, may return the booking response while the associated Flight transitions server-side.

- Admin/workshop transitions
  - `POST /flights/{flightId}/cancel`
    - Cancels a flight; triggers notification/refund workflow.
  - `POST /flights/{flightId}/perform`
    - Marks a flight as done.

No UI screens are required (API-only).

### Aspects

- **Observability**:
  - Log state transitions at `Information` with structured fields: `flightId`, `fromState`, `toState`, `bookingCount`, `minimumPassengers`, `capacity`.
  - Log rejected transitions at `Warning` with `reason`.

- **Error handling**:
  - Expected conflicts (invalid transitions) should not throw; they should return a Business result mapped to `409 Conflict`.

- **Security**:
  - No authentication/authorization in this workshop version; admin endpoints are intended for local demos.

- **Concurrency**:
  - Booking-driven state changes must be consistent under concurrent booking requests; the system must not emit duplicate “confirmed” notifications for the same flight.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a booking is created successfully AND the resulting booking count is greater than or equal to `minimumPassengers` AND the Flight state is `SCHEDULED`, the system SHALL transition the Flight to `CONFIRMED` AND SHALL trigger a confirmation notification workflow.
- [ ] WHEN the Flight has already transitioned to `CONFIRMED`, the system SHALL NOT trigger the confirmation notification workflow again for subsequent bookings.
- [ ] IF a booking is created successfully AND the resulting booking count is greater than or equal to the Rocket capacity, THEN the system SHALL transition the Flight to `SOLD_OUT`.

- [ ] WHEN a booking is created successfully AND both confirmation and sold-out conditions apply, the system SHALL set the Flight state to `SOLD_OUT`.
- [ ] IF a client attempts to create a booking for a Flight in state `DONE`, THEN the system SHALL reject the request with a conflict outcome.

- [ ] WHEN an admin calls the cancel operation for an existing Flight, the system SHALL transition the Flight state to `CANCELLED` AND SHALL trigger notification and refund workflows for existing bookings.
- [ ] WHEN an admin calls the perform operation for an existing Flight, the system SHALL transition the Flight state to `DONE`.
- [ ] IF an admin calls cancel or perform for a non-existent Flight, THEN the system SHALL return a not-found outcome.

> End of Feature Specification for 006-automatic_flight_state_transitions, last updated 2025-12-16.
