using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using MagicOnion.Server;
using MagicOnion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;
using static System.Reflection.Metadata.BlobBuilder;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Defaults;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

public interface IPolicyService
{
    Task<Policy> GetOrFetchPolicyAsync(string policyCode);

    Task<T> GetOrFetchValueAsync<T>(string policyCode)
        where T : struct;
}

public class PolicyService : IPolicyService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly string _cacheKeyPrefix = "policy_";

    public PolicyService(
        ILogger<PolicyService> logger,
        IDbContextFactory<AppDbContext> factory,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._factory = factory;
        this._cache = cache;

        this._cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        };
    }

    public async Task<Policy> GetOrFetchPolicyAsync(string policyCode)
    {
        try
        {
            var cacheKey = this._cacheKeyPrefix + policyCode;

            // キャッシュから取得
            if (this._cache.TryGetValue<Policy>(cacheKey, out var cachedPolicy) &&
                cachedPolicy is not null)
            {
                return cachedPolicy;
            }

            // DBから取得
            await using var context = await this._factory.CreateDbContextAsync();
            var policy = await context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PolicyCode == policyCode);

            if (policy != null)
            {
                // DBから取得したPolicyをキャッシュに保存
                this._cache.Set(cacheKey, policy, this._cacheOptions);
                return policy;
            }

            // デフォルトを使用
            var policies = DefaultPolicy.CreateDefaultPolicies();
            if (policies.TryGetValue(policyCode, out var defaultPolicy))
            {
                return defaultPolicy;
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during policy loading.");
            throw;
        }

        throw new InvalidOperationException($"Policy not found. (PolicyCode: {policyCode})");
    }

    public async Task<T> GetOrFetchValueAsync<T>(string policyCode)
        where T : struct
    {
        var policy = await this.GetOrFetchPolicyAsync(policyCode);
        var result =  Constants.DataType.ConvertTo<T>(policy.DataType, policy.PolicyValue);
        return result ?? throw new Exception($"Invalid type {typeof(T).Name} for Policy {policyCode}.");
    }
}
