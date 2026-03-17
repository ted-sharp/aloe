// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Diagnostics;
using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Extensions;
using Aloe.Apps.Medock.MdPatientLib.Data;
using Aloe.Apps.Medock.MdPatientLib.Extensions;
using Aloe.Apps.Medock.MdSeeder.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Error);

var launcherCs = builder.Configuration["ConnectionStrings:MdLauncher"]
    ?? throw new InvalidOperationException("ConnectionStrings:MdLauncher is not configured.");
var patientCs = builder.Configuration["ConnectionStrings:MdPatient"]
    ?? throw new InvalidOperationException("ConnectionStrings:MdPatient is not configured.");

builder.Services.AddMdLauncher(launcherCs);
builder.Services.AddMdPatient(patientCs);

using var host = builder.Build();

var totalSw = Stopwatch.StartNew();

try
{
    using var scope = host.Services.CreateScope();
    var launcherDb = scope.ServiceProvider.GetRequiredService<MdLauncherDbContext>();
    var patientDb = scope.ServiceProvider.GetRequiredService<MdPatientDbContext>();

    await launcherDb.Database.EnsureCreatedAsync();
    await patientDb.Database.EnsureCreatedAsync();

    Console.WriteLine("=== MdSeeder Start ===");
    Console.WriteLine();

    // medock DB
    await RunSeederAsync("[1/7] Apps", () => AppSeeder.SeedAsync(launcherDb));
    await RunSeederAsync("[2/7] Menus", () => MenuSeeder.SeedAsync(launcherDb));
    var userIds = await RunSeederWithResultAsync("[3/7] Users", () => UserSeeder.SeedAsync(launcherDb));
    var facilityUserIds = await RunSeederWithResultAsync("[4/7] FacilityUsers", () => FacilityUserSeeder.SeedAsync(launcherDb, userIds));
    await RunSeederAsync("[5/7] FacilityUserRoles", () => FacilityUserRoleSeeder.SeedAsync(launcherDb, facilityUserIds));
    await RunSeederAsync("[6/7] RoleMenus", () => RoleMenuSeeder.SeedAsync(launcherDb));

    // mdpatient DB
    await RunSeederAsync("[7/7] Patients", () => PatientSeeder.SeedAsync(patientDb));

    Console.WriteLine();
    Console.WriteLine($"=== MdSeeder Complete ({totalSw.Elapsed.TotalSeconds:F1}s) ===");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static async Task RunSeederAsync(string label, Func<Task> seeder)
{
    var sw = Stopwatch.StartNew();
    Console.Write($"  {label} ... ");
    await seeder();
    Console.WriteLine($"({sw.ElapsedMilliseconds}ms)");
}

static async Task<T> RunSeederWithResultAsync<T>(string label, Func<Task<T>> seeder)
{
    var sw = Stopwatch.StartNew();
    Console.Write($"  {label} ... ");
    var result = await seeder();
    Console.WriteLine($"({sw.ElapsedMilliseconds}ms)");
    return result;
}
