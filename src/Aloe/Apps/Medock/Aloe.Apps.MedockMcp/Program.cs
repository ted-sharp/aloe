using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

// DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = Host.CreateApplicationBuilder(args);

// MCP Server のログは stderr に出力（stdout は MCP プロトコル通信で使用）
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// MCP Server 設定
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// DB接続
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MedockDbContext>(options =>
{
    options.UseNpgsql(connectionString);
}, ServiceLifetime.Scoped);

builder.Services.AddDbContextFactory<MedockDbContext>(options =>
{
    options.UseNpgsql(connectionString);
}, ServiceLifetime.Scoped);

// MedockLib サービス登録
builder.Services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();
builder.Services.AddSingleton<IVoiceCommandParser, VoiceCommandParser>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentStatsRepository, AppointmentStatsRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAppointmentFormService, AppointmentFormService>();
builder.Services.AddScoped<IAppointmentResourceAssignmentService, AppointmentResourceAssignmentService>();
builder.Services.AddScoped<IAppointmentStatsUpdateService, AppointmentStatsUpdateService>();
builder.Services.AddScoped<ICalendarDataService, CalendarDataService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// MCP環境ではHTTPコンテキストが無いため、NullHttpContextAccessor を登録
builder.Services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor>(
    new Microsoft.AspNetCore.Http.HttpContextAccessor());

await builder.Build().RunAsync();
