using Aloe.Apps.Medock.MdLauncherLib.Extensions;
using Aloe.Utils.Hosting.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogDefaults();

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

var configFilePath = Path.Combine(AppContext.BaseDirectory, "launcher-config.json");
builder.Services.AddMdLauncher(configFilePath);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapMagicOnionService();

app.Run();
