using Aloe.Apps.DashboardServer.Components;
using Aloe.Apps.DashboardServer.Extensions;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Serilog設定
var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logsDirectory);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .WriteTo.File(
        new CompactJsonFormatter(),
        Path.Combine(logsDirectory, "dashboard_.json"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        encoding: System.Text.Encoding.UTF8));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

builder.Services.AddOtelViewer(builder.Configuration);

var app = builder.Build();

// ログディレクトリのパスをコンソールに出力
Console.WriteLine($"ログディレクトリ: {logsDirectory}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

app.Run();
