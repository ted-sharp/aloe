using Aloe.Apps.MedockMcp.Extensions;
using Aloe.Apps.MedockMcp.Tools;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Information);

// Services
builder.Services.AddMcpDbContext(builder.Configuration);
builder.Services.AddMcpCoreServices(builder.Configuration);
builder.Services.AddMcpRepositories();
builder.Services.AddMcpAppServices();

// MCP
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<AppointmentQueryTools>()
    .WithTools<AppointmentStatsTools>()
    .WithTools<FacilityTools>()
    .WithTools<SystemTools>();

var app = builder.Build();
app.MapMcp();
await app.RunAsync();
