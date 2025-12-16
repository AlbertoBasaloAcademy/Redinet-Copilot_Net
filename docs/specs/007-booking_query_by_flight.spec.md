# 007-booking_query_by_flight Specification

## 1. 👔 Problem Specification

The API must allow API consumers to query all Bookings for a specific Flight. This supports client experiences where users can review who is booked on a flight and what each passenger paid.

### List Bookings for a Flight

- **As a** client app (API consumer)
- **I want to** list all bookings for a specific `flightId`
- **So that** I can display the passenger manifest for that flight

### See Passenger and Pricing Details

- **As a** client app (API consumer)
- **I want to** receive passenger information and pricing information for each booking
- **So that** I can show a booking summary per passenger

### Handle Unknown Flights

- **As a** client app (API consumer)
- **I want to** be informed when a `flightId` does not exist
- **So that** I can show a clear error instead of an empty or misleading result

## 2. 🧑‍💻 Solution Overview

Implement a read-only REST endpoint to list bookings by flight, following the existing 3-layer structure (Presentation → Business → Persistence). The endpoint is flight-scoped for consistency with booking creation.

### Data Models

- **Domain model**: `Booking`
  - `Id` (string)
  - `FlightId` (string)
  - `PassengerName` (string)
  - `PassengerEmail` (string)
  - `FinalPrice` (decimal)

- **Domain model**: `Flight` (used for existence checks)
  - `Id` (string)

- **DTOs**
  - Response: `BookingResponseDto`
    - includes at least `id`, `flightId`, `passengerName`, `passengerEmail`, `finalPrice`

Notes:
- No sorting, pagination, or filtering is required for this workshop version.

### Software Components

- **Presentation** (`lib/Presentation`)
  - Extend `BookingEndpoints` to map a read endpoint:
    - `GET /flights/{flightId}/bookings`
  - Convert domain bookings to `BookingResponseDto`.
  - Map expected outcomes to HTTP status codes (`200`, `404`).

- **Business** (`lib/Business`)
  - Extend `BookingService` (or introduce a query method) to:
    - Verify the flight exists (via `IFlightRepository`).
    - Retrieve bookings for the flight (via `IBookingRepository`).
    - Return explicit results for not-found vs success.

- **Persistence** (`lib/Persistence`)
  - Ensure `IBookingRepository` supports listing by flight:
    - `ListByFlightId(string flightId)` returning a snapshot list.
  - Implementation remains in-memory and thread-safe.

### User Interface

HTTP/JSON endpoint (Minimal APIs):

- `GET /flights/{flightId}/bookings`
  - Success: `200 OK` with JSON array of bookings for that flight (possibly empty).
  - Failure: `404 Not Found` if the referenced flight does not exist.

### Aspects

- **Validation**: no additional payload validation is required (path-only request).
- **Error handling**: do not throw for expected “not found”; return explicit business results.
- **Logging**:
  - Log successful list requests at `Information` (include `flightId` and count).
  - Log unknown `flightId` list attempts at `Warning`.
- **Concurrency**: repository returns a stable snapshot (safe under concurrent booking creation).
- **Security**: no authentication/authorization for this workshop version.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client calls `GET /flights/{flightId}/bookings` for an existing flight, the system SHALL return `200 OK` with a JSON array of that flight’s bookings.
- [ ] WHEN a client calls `GET /flights/{flightId}/bookings` for an existing flight with no bookings, the system SHALL return `200 OK` with an empty JSON array.
- [ ] IF a client calls `GET /flights/{flightId}/bookings` for a non-existent `flightId`, THEN the system SHALL return `404 Not Found`.

> End of Feature Specification for 007-booking_query_by_flight, last updated 2025-12-16.
