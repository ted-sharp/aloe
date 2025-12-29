using System.Text;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Repositories;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components;
using Aloe.Apps.MedockServer.Hubs;
using Aloe.Apps.MedockServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

// DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure Logging (パフォーマンス計測ログ表示用)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Controllers
builder.Services.AddControllers();

// Add HttpClient for server-side components to call API
// Blazor Serverでサーバー側コンポーネントから同じサーバーのAPIを呼び出す場合の設定
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var navigationManager = sp.GetRequiredService<NavigationManager>();

    httpClient.BaseAddress = new Uri(navigationManager.BaseUri);

    return httpClient;
});

// Configure Cookie Settings
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection(CookieSettings.SectionName));

// SQLログ表示設定（appsettings.jsonまたは環境変数で制御）
var showSqlLogs = builder.Configuration.GetValue<bool>("Features:ShowSqlLogs", false);
// 環境変数で上書き可能（例: Features__ShowSqlLogs=true）
if (Environment.GetEnvironmentVariable("Features__ShowSqlLogs") != null)
{
    if (Boolean.TryParse(Environment.GetEnvironmentVariable("Features__ShowSqlLogs"), out var envValue))
    {
        showSqlLogs = envValue;
    }
}

// SQLログのログレベルを動的に設定
if (!showSqlLogs)
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
}

// Add DbContext Factory (Blazor Server では各操作で短命なコンテキストを生成)
builder.Services.AddDbContextFactory<MedockDbContext>((services, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (!showSqlLogs)
    {
        // SQLログを出力しない
        options.UseLoggerFactory(NullLoggerFactory.Instance);
    }
    else if (builder.Environment.IsDevelopment())
    {
        // 開発環境でSQLログを有効にする場合、詳細な情報も表示
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}, ServiceLifetime.Scoped);

// Add DbContext (リポジトリで使用)
builder.Services.AddDbContext<MedockDbContext>((services, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (!showSqlLogs)
    {
        // SQLログを出力しない
        options.UseLoggerFactory(NullLoggerFactory.Instance);
    }
    else if (builder.Environment.IsDevelopment())
    {
        // 開発環境でSQLログを有効にする場合、詳細な情報も表示
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}, ServiceLifetime.Scoped);

// Add Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentStatsRepository, AppointmentStatsRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

// Add Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(PasswordHasher.Default);
builder.Services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<ICalendarDataService, CalendarDataService>();

// Add Calendar Services
builder.Services.AddScoped<Aloe.Apps.MedockServer.Components.Pages.CalendarState>();
builder.Services.AddScoped<Aloe.Apps.MedockServer.Components.Pages.CalendarFilterService>();
builder.Services.AddScoped<Aloe.Apps.MedockServer.Components.Pages.CalendarDataService>();

// Add SignalR
builder.Services.AddSignalR();

// Add AppointmentStats Notification Service
builder.Services.AddScoped<IAppointmentStatsNotificationService, AppointmentStatsNotificationService>();

// Add Authentication State Provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<ProtectedLocalStorage>();

// Configure Cookie Authentication
var cookieSettings = builder.Configuration.GetSection(CookieSettings.SectionName).Get<CookieSettings>() ?? new CookieSettings();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MedockAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieSettings.ExpireTimeSpanMinutes ?? 15);
        options.SlidingExpiration = cookieSettings.SlidingExpiration ?? true;
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/logout";
        options.AccessDeniedPath = "/login";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

// Map SignalR Hub
app.MapHub<AppointmentStatsHub>("/hubs/appointmentStats");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
