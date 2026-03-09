using Aloe.Apps.RazorReportServer.Components;
using Aloe.Apps.RazorReportServer.Extensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration for papers and components
builder.Configuration.AddJsonFile("appsettings.papers.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.components.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddRazorReportServices(builder.Configuration);

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
