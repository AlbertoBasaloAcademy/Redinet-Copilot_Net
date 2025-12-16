# 005-discount_calculation_precedence Specification

## 1. 👔 Problem Specification

The API must calculate and persist the final Booking price derived from the Flight `basePrice` by applying exactly one discount, chosen using a strict precedence order.

### Calculate Final Booking Price During Booking Creation

- **As a** client app (API consumer)
- **I want to** have the API calculate the Booking `finalPrice` from the Flight `basePrice`
- **So that** the price is determined server-side and returned consistently

### Apply Exactly One Discount Using Precedence

- **As a** client app (API consumer)
- **I want to** have exactly one discount applied using defined precedence
- **So that** discount behavior is predictable and cannot be stacked

### Persist and Return the Computed Final Price

- **As a** client app (API consumer)
- **I want to** receive the computed `finalPrice` in the Booking response
- **So that** I can display the charged price without re-implementing discount logic

## 2. 🧑‍💻 Solution Overview

Implement discount calculation as part of booking creation in the Business layer, using existing repositories to determine booking counts and rocket capacity. Persist the computed `finalPrice` on the Booking entity.

### Data Models

- **Domain model**: `Booking` (extend existing)
  - `Id` (string)
  - `FlightId` (string)
  - `PassengerName` (string)
  - `PassengerEmail` (string)
  - `FinalPrice` (decimal, required; computed at creation time)

- **Domain model**: `Flight` (existing)
  - `Id` (string)
  - `BasePrice` (decimal)
  - `MinimumPassengers` (int)

- **Domain model**: `Rocket` (existing)
  - `Id` (string)
  - `Capacity` (int)

- **DTOs**
  - Create request: `CreateBookingDto` (unchanged)
    - `passengerName`, `passengerEmail`
  - Response: `BookingResponseDto` (extend existing)
    - `id`, `flightId`, `passengerName`, `passengerEmail`, `finalPrice`

### Software Components

- **Presentation** (`lib/Presentation`)
  - Keep `POST /flights/{flightId}/bookings` as the booking creation entrypoint.
  - Include `finalPrice` in the created booking response.

- **Business** (`lib/Business`)
  - Extend `BookingService` booking creation flow to compute discount and final price.
  - Use existing booking count + rocket capacity checks from booking creation to determine which discount applies.
  - Apply exactly one discount using this precedence (highest first):
    1. **Last seat**: 0% discount.
    2. **One passenger away from reaching minimum passengers**: 30% discount.
    3. **Otherwise**: 10% discount.

  Discount decision should be based on the booking count *after* the new booking is included:
  - Let `newBookingCount = existingBookingCount + 1`.
  - “Last seat” applies WHEN `newBookingCount == rocketCapacity`.
  - “One passenger away from reaching minimum passengers” applies WHEN `newBookingCount == (minimumPassengers - 1)`.
  - Otherwise, apply 10%.

  Computation:
  - `finalPrice = basePrice * (1 - discountRate)` where `discountRate ∈ {0.0, 0.3, 0.1}`.

- **Persistence** (`lib/Persistence`)
  - Ensure `Booking` persistence stores `FinalPrice`.

### User Interface

HTTP/JSON behavior (Minimal APIs):

- `POST /flights/{flightId}/bookings`
  - Response includes computed `finalPrice`.
  - No new endpoints are required for this feature.

### Aspects

- **Validation**:
  - Booking creation continues to validate passenger fields as defined in prior booking rules.
  - Flight `basePrice` is assumed valid due to flight creation validation.

- **Error handling**:
  - Discount calculation must not introduce new error states; it runs only after the Flight and Rocket are found and booking creation rules pass.

- **Logging**:
  - Log the chosen discount rule (e.g., `LastSeat`, `OneAwayFromMinimumPassengers`, `Standard`) and the computed `finalPrice` at `Information` level during successful booking creation.

- **Security**:
  - No authentication/authorization for this workshop version.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client creates a Booking successfully, the system SHALL compute and store `finalPrice` from the Flight `basePrice` using exactly one discount.

- [ ] WHEN a client creates a Booking and that Booking consumes the last available seat for the Flight, the system SHALL apply a 0% discount (no discount) and SHALL NOT apply any other discount.

- [ ] WHEN a client creates a Booking and the resulting booking count is exactly one passenger away from the Flight `minimumPassengers`, the system SHALL apply a 30% discount AND SHALL NOT apply any other discount.

- [ ] WHEN a client creates a Booking and neither the “last seat” nor the “one away from minimum passengers” conditions apply, the system SHALL apply a 10% discount.

- [ ] WHEN a client receives the created Booking response, the system SHALL include the stored `finalPrice` value.

> End of Feature Specification for 005-discount_calculation_precedence, last updated 2025-12-16.
