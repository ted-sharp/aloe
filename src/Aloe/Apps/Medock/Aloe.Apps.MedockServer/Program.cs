using Aloe.Apps.MedockServer.Components;
using Aloe.Apps.MedockServer.Extensions;
using Aloe.Apps.MedockServer.Hubs;

// DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
ConfigureLogging(builder);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add Medock services via extension methods
builder.Services.AddMedockHttpClient();
builder.Services.AddMedockDbContext(builder.Configuration, builder.Environment);
builder.Services.AddMedockRepositories();
builder.Services.AddMedockServices(builder.Configuration);
builder.Services.AddMedockCalendarServices();
builder.Services.AddMedockAuthentication(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigurePipeline(app);

app.Run();

// ============================================================================
// Local Functions
// ============================================================================

/// <summary>
/// ログ設定 (パフォーマンス計測ログ表示用)
/// </summary>
static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

    // SQL ログのフィルタリング（ShowSqlLogs が false の場合）
    var showSqlLogs = builder.Configuration.GetValue<bool>("Features:ShowSqlLogs", false);
    var envValue = Environment.GetEnvironmentVariable("Features__ShowSqlLogs");
    if (!String.IsNullOrEmpty(envValue) && Boolean.TryParse(envValue, out var parsedValue))
    {
        showSqlLogs = parsedValue;
    }

    if (!showSqlLogs)
    {
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
    }
}

/// <summary>
/// ミドルウェアパイプライン設定
/// </summary>
static void ConfigurePipeline(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();
    app.UseAntiforgery();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllers();
    app.MapHub<AppointmentStatsHub>("/hubs/appointmentStats");

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
}
