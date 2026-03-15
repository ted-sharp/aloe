using Aloe.Apps.Medock.MdXadesApi.Options;
using Aloe.Apps.Medock.MdXadesApi.Services;
using Aloe.Apps.Medock.MdXadesLib.Extensions;
using Aloe.Apps.Medock.MdXadesLib.Options;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

builder.Services.Configure<XadesOptions>(
    builder.Configuration.GetSection("XAdES"));
builder.Services.Configure<MdXadesApiOptions>(
    builder.Configuration.GetSection("XAdES"));

builder.Services.AddMdXades();
builder.Services.AddSingleton<SignatureStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.MapMagicOnionService();

app.Run();
