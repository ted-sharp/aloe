using Aloe.Apps.ExcelReportApi.Options;
using Aloe.Apps.ExcelReportApi.Services;
using Aloe.Apps.ExcelReportLib.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

builder.Services.Configure<ExcelReportApiOptions>(
    builder.Configuration.GetSection("ExcelReport"));

builder.Services.AddExcelReport();

builder.Services.AddSingleton<JobStore>();
builder.Services.AddSingleton<ReportJobQueue>();
builder.Services.AddHostedService<ReportWorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.MapMagicOnionService();

app.Run();
