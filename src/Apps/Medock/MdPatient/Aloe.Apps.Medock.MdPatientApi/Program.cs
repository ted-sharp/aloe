using Aloe.Apps.Medock.MdPatientLib.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

var connectionString = builder.Configuration.GetConnectionString("MdPatient")
    ?? "Host=localhost;Database=mdpatient;Username=postgres;Password=postgres";
builder.Services.AddMdPatient(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.MapMagicOnionService();

app.Run();
