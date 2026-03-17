using Aloe.Apps.Medock.MdLauncherLib.Extensions;
using Aloe.Utils.Hosting.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogDefaults();

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

var connectionString = builder.Configuration.GetConnectionString("MdLauncher")
    ?? throw new InvalidOperationException("ConnectionStrings:MdLauncher が設定されていません。");
builder.Services.AddMdLauncher(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapMagicOnionService();

app.Run();
