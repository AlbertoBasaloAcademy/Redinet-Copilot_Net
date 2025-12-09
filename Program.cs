using NetAstroBookings.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NetAstroBookings.Persistence.InMemoryRocketRepository>();
builder.Services.AddScoped<NetAstroBookings.Business.RocketService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapRocketEndpoints();

app.Run();
