using Aloe.Apps.ExcelReportApi.Options;
using Aloe.Apps.ExcelReportApi.Services;
using Aloe.Apps.ExcelReportLib.Extensions;
using Aloe.Utils.Hosting.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogDefaults();

builder.WebHost.ConfigureKestrel((ctx, kestrel) =>
    kestrel.Configure(ctx.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMagicOnion();

builder.Services.Configure<ExcelReportApiOptions>(
    builder.Configuration.GetSection("ExcelReport"));

builder.Services.AddExcelReportWithWindowsPrinter();

builder.Services.AddSingleton<JobStore>();
builder.Services.AddSingleton<ReportJobQueue>();
builder.Services.AddHostedService<ReportWorkerService>();

builder.Services.AddSingleton<PrintJobStore>();
builder.Services.AddSingleton<PrintJobQueue>();
builder.Services.AddHostedService<PrintWorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.MapMagicOnionService();

app.Run();
