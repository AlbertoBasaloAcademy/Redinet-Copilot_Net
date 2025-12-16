using NetAstroBookings.Presentation;
using NetAstroBookings.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRocketRepository, InMemoryRocketRepository>();
builder.Services.AddScoped<NetAstroBookings.Business.RocketService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapRocketEndpoints();

app.Run();
