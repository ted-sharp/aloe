// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Utils.Hosting.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using global::Serilog;

// === パターン 1: Host を使用する場合 ===
Console.WriteLine("=== Pattern 1: Host + AddSerilogDefaults ===");

using var host = Host.CreateDefaultBuilder(args)
    .AddSerilogDefaults()
    .Build();

Log.Information("Host パターンで Serilog が設定されました");

// === パターン 2: Host 不使用の CLI の場合 ===
Console.WriteLine();
Console.WriteLine("=== Pattern 2: ConfigureSerilogFromSettings ===");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

using (SerilogExtensions.ConfigureSerilogFromSettings(configuration))
{
    Log.Information("CLI パターンで Serilog が設定されました");
    Log.Warning("これは警告メッセージです");
}

Console.WriteLine("完了");
