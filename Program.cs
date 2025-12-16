using NetAstroBookings.Presentation;
using NetAstroBookings.Persistence;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

builder.Services.AddSingleton<IRocketRepository, InMemoryRocketRepository>();
builder.Services.AddSingleton<IFlightRepository, InMemoryFlightRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

builder.Services.AddScoped<NetAstroBookings.Business.RocketService>();
builder.Services.AddScoped<NetAstroBookings.Business.FlightService>();
builder.Services.AddSingleton<NetAstroBookings.Business.BookingService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapRocketEndpoints();
app.MapFlightEndpoints();
app.MapBookingEndpoints();

app.Run();
