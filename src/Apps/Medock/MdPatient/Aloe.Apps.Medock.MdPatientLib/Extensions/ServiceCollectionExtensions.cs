// <copyright file="ServiceCollectionExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Data;
using Aloe.Apps.Medock.MdPatientLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdPatientLib.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> へ MdPatient サービスを登録する拡張メソッド。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MdPatient サービス（EF Core + PatientService）を登録する。
    /// </summary>
    public static IServiceCollection AddMdPatient(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MdPatientDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
