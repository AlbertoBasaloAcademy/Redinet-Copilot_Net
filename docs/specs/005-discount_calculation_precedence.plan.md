## Plan: Booking Discount Precedence

Add server-side `FinalPrice` calculation during booking creation, applying exactly one discount by strict precedence based on `newBookingCount` (existing + 1). Persist `FinalPrice` on the `Booking` domain model, ensure the in-memory repository round-trips it, and extend the booking response DTO so `finalPrice` is returned from `POST /flights/{flightId}/bookings`. Keep logic in `BookingService` and log the chosen rule and computed price at `Information` level.

### Steps
1. Create a feature branch and ensure a clean working tree before changes.
2. Add required `FinalPrice` to [lib/Models/Booking.cs](../../lib/Models/Booking.cs) and set it at creation time.
3. Update [lib/Persistence/InMemoryBookingRepository.cs](../../lib/Persistence/InMemoryBookingRepository.cs) cloning/persistence to include `FinalPrice`.
4. Implement precedence logic in [lib/Business/BookingService.cs](../../lib/Business/BookingService.cs) `CreateBookingAsync`, using `newBookingCount` to select one rule.
5. Compute `finalPrice = basePrice * (1 - discountRate)` with `decimal` rates and log rule + price at `Information`.
6. Extend [lib/Dtos/BookingResponseDto.cs](../../lib/Dtos/BookingResponseDto.cs) and mapping in [lib/Presentation/BookingEndpoints.cs](../../lib/Presentation/BookingEndpoints.cs) to return `FinalPrice`.

### Further Considerations
1. Rounding: simplest is no rounding (preserve `decimal` precision); alternatively round to 2 decimals for currency.
2. Rule structure: private method returning `(rule, discountRate)` vs private `enum` inside `BookingService`.
3. Precedence collisions: keep `LastSeat` check first to guarantee strict precedence.