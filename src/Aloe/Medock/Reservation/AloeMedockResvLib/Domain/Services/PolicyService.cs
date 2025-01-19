using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
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

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

public interface IPolicyService
{
    Task LoadPoliciesAsync();

    Task<Policy> GetPolicyAsync(string policyCode);

    Task<T> GetValueAsync<T>(string policyCode)
        where T : struct;
}

public class PolicyService : IPolicyService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IMemoryCache _cache;

    public PolicyService(
        ILogger<PolicyService> logger,
        IDbContextFactory<AppDbContext> factory,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._factory = factory;
        this._cache = cache;
    }

    public async Task LoadPoliciesAsync()
    {
        await Task.Run(() => this.FetchPolicies(false));
    }

    private async Task<Dictionary<string, Policy>> FetchPoliciesAsync()
    {
        return await Task.Run(() => this.FetchPolicies(true));
    }

    private Dictionary<string, Policy> FetchPolicies(bool useCache = true)
    {
        try
        {
            var key = $"{nameof(PolicyService)}_{nameof(this.FetchPolicies)}";
            if (useCache && this._cache.TryGetValue<Dictionary<string, Policy>>(
                    key, out var policies))
            {
                return policies ?? [];
            }

            // デフォルトをもとにする
            policies = PolicyService.CreateDefaultPolicies();

            // DBにある設定で上書きする
            using var context = this._factory.CreateDbContext();
            var policyList = context.Policies
                .AsNoTracking()
                .ToList();
            foreach (var policy in policyList)
            {
                policies[policy.PolicyCode] = policy;
            }

            // キャッシュを更新する
            this._cache.Set(key, policies, new MemoryCacheEntryOptions
            {
                // 朝のログイン時に集中するためしばらく保持しておく
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            });
            this._logger.LogDebug("Policy loaded count: {Count}", policies.Count);

            return policies;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during policy loading.");
        }

        return [];
    }

    public async Task<Policy> GetPolicyAsync(string policyCode)
    {
        var policies = await this.FetchPoliciesAsync();
        var policy = policies.GetValueOrDefault(policyCode);
        return policy ?? throw new Exception($"Policy not found. (PolicyCode: {policyCode})");
    }

    public async Task<T> GetValueAsync<T>(string policyCode)
        where T : struct
    {
        var policy = await this.GetPolicyAsync(policyCode);
        var result =  Constants.DataType.ConvertTo<T>(policy.DataType, policy.PolicyValue);
        return result ?? throw new Exception($"Invalid type {typeof(T).Name} for Policy {policyCode}.");
    }

    #region CreateDefaultPolicies

    public static Dictionary<string, Policy> CreateDefaultPolicies()
    {
        var policies = new Dictionary<string, Policy>
        {
            [PolicyCode.LoginLockingFailAtempts] = new()
            {
                PolicyCode = PolicyCode.LoginLockingFailAtempts,
                PolicyName = nameof(PolicyCode.LoginLockingFailAtempts),
                PolicyDesc = "ログイン失敗時にロックするための失敗回数です。",
                DataType = Constants.DataType.Int32,
                PolicyValue = "3",
            },

            [PolicyCode.LoginLockingSeconds] = new()
            {
                PolicyCode = PolicyCode.LoginLockingSeconds,
                PolicyName = nameof(PolicyCode.LoginLockingSeconds),
                PolicyDesc = "ログイン失敗時にロックする秒数です。",
                DataType = Constants.DataType.Int32,
                PolicyValue = "30",
            },

            [PolicyCode.ResvDefaultFloor1] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor1,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor1),
                PolicyDesc = "デフォルト呼び出すフロア1のID(FloorId)です。",
                DataType = Constants.DataType.Int32,
                PolicyValue = "1",
            },

            [PolicyCode.ResvDefaultFloor2] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor2,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor2),
                PolicyDesc = "デフォルト呼び出すフロア2のID(FloorId)です。",
                DataType = Constants.DataType.Int32,
                PolicyValue = "2",
            },
        };

        return policies;
    }

    #endregion CreateDefaultPolicies
}
