using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockMcp.Services;
using Aloe.Apps.MedockMcp.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Information);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MedockDbContext>(
    o => o.UseNpgsql(connectionString).UseLoggerFactory(NullLoggerFactory.Instance),
    ServiceLifetime.Scoped);
builder.Services.AddDbContextFactory<MedockDbContext>(
    o => o.UseNpgsql(connectionString).UseLoggerFactory(NullLoggerFactory.Instance),
    ServiceLifetime.Scoped);

// Services
builder.Services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();
builder.Services.AddSingleton(_ => PasswordHasher.Default);
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection("Cookie"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IUserContextService, McpUserContextService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentStatsRepository, AppointmentStatsRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IAppointmentResourceAssignmentService, AppointmentResourceAssignmentService>();
builder.Services.AddScoped<IAppointmentStatsUpdateService, AppointmentStatsUpdateService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IAppointmentFormService, AppointmentFormService>();

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
