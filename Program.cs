using NetAstroBookings.Presentation;
using NetAstroBookings.Persistence;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

builder.Services.AddSingleton<IRocketRepository, InMemoryRocketRepository>();
builder.Services.AddSingleton<IFlightRepository, InMemoryFlightRepository>();

builder.Services.AddScoped<NetAstroBookings.Business.RocketService>();
builder.Services.AddScoped<NetAstroBookings.Business.FlightService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapRocketEndpoints();
app.MapFlightEndpoints();

app.Run();
