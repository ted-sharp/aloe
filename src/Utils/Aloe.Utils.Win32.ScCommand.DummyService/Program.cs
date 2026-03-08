using Aloe.Utils.Win32.ScCommand.DummyService;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
