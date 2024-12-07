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

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

public interface IPolicyService
{
    Task LoadPoliciesAsync();

    Policy? GetPolicy(string policyCode);

    T? GetValue<T>(string policyCode);
}

public class PolicyService : IPolicyService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _context;

    // 念の為スレッドセーフとします。
    private static ConcurrentDictionary<string, Policy> s_policies = new();

    public PolicyService(
        ILogger<PolicyService> logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    #region Load

    public async Task LoadPoliciesAsync()
    {
        await Task.Run(this.LoadPolicies);
    }

    public void LoadPolicies()
    {
        try
        {
            if (!PolicyService.s_policies.IsEmpty)
            {
                // TODO: キャッシュの有効期間とかないとDB直接書き換えたときに漏れそう
                // そうなってくると defaultPolicies は別で用意しとくのがよいか

                // すでにデータがある場合は何もしない
                return;
            }

            // デフォルトポリシーをもとにする
            var policies = PolicyService.CreateDefaultPolicies();

            // DBにある設定を上書きする
            var policyList = this._context.Policies
                .AsNoTracking()
                .ToList();
            foreach (var policy in policyList)
            {
                policies[policy.PolicyCode] = policy;
            }

            // キャッシュを更新する
            PolicyService.s_policies = policies;
            this._logger.LogDebug("Policy loaded count: {Count}", policies.Count);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during policy loading.");
        }
    }

    public static ConcurrentDictionary<string, Policy> CreateDefaultPolicies()
    {
        var policies = new ConcurrentDictionary<string, Policy>
        {
            [PolicyCode.LoginLockingFailAtempts] = new()
            {
                PolicyCode = PolicyCode.LoginLockingFailAtempts,
                PolicyName = nameof(PolicyCode.LoginLockingFailAtempts),
                DataType = Constants.DataType.Int32,
                PolicyValue = "3",
                PolicyDesc = "ログイン失敗時にロックするための失敗回数です。",
            },

            [PolicyCode.LoginLockingSeconds] = new()
            {
                PolicyCode = PolicyCode.LoginLockingSeconds,
                PolicyName = nameof(PolicyCode.LoginLockingSeconds),
                DataType = Constants.DataType.Int32,
                PolicyValue = "30",
                PolicyDesc = "ログイン失敗時にロックする秒数です。",
            },

            [PolicyCode.ResvDefaultFloor1] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor1,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor1),
                DataType = Constants.DataType.Int32,
                PolicyValue = "1",
                PolicyDesc = "デフォルト呼び出すフロア1のID(FloorId)です。",
            },

            [PolicyCode.ResvDefaultFloor2] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor2,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor2),
                DataType = Constants.DataType.Int32,
                PolicyValue = "2",
                PolicyDesc = "デフォルト呼び出すフロア2のID(FloorId)です。",
            },
        };

        return policies;
    }

    #endregion Load

    public Policy? GetPolicy(string policyCode)
    {
        var policy = PolicyService.s_policies.GetValueOrDefault(policyCode);

        if (policy == null)
        {
            this._logger.LogWarning("Policy not found: {PolicyCode}", policyCode);
        }

        return policy;
    }

    public T? GetValue<T>(string policyCode)
    {
        var policy = this.GetPolicy(policyCode);
        if (policy == null)
        {
            return default;
        }

        if (typeof(T) == typeof(bool) && policy.DataType == Constants.DataType.Boolean)
        {
            return Boolean.TryParse(policy.PolicyValue, out var result) ? (T)(object)result : default;
        }

        if (typeof(T) == typeof(int) && policy.DataType == Constants.DataType.Int32)
        {
            return Int32.TryParse(policy.PolicyValue, out var result) ? (T)(object)result : default;
        }

        if (typeof(T) == typeof(string) && policy.DataType == Constants.DataType.String)
        {
            return (T)(object)policy.PolicyValue;
        }

        var msg = $"Invalid type {typeof(T).Name} for Policy {policyCode}.";
        this._logger.LogError(msg);
        throw new InvalidOperationException(msg);
    }
}
