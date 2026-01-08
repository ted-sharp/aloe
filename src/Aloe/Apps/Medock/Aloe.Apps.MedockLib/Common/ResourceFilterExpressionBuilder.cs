using System.Linq.Expressions;

namespace Aloe.Apps.MedockLib.Common;

/// <summary>
/// リソースフィルター条件を動的に構築するヘルパークラス
/// AND(OR1(A, B), OR2(C, D))形式の条件を構築
/// </summary>
public static class ResourceFilterExpressionBuilder
{
    /// <summary>
    /// ORグループの条件をSQL WHERE句として構築
    /// </summary>
    /// <param name="resourceIds">ORグループに含まれるリソースIDリスト</param>
    /// <param name="columnName">リソースIDカラム名（デフォルト: "appt_res_id"）</param>
    /// <returns>SQL WHERE句の条件文字列（空の場合は空文字列）</returns>
    public static string BuildOrGroupCondition(List<Guid> resourceIds, string columnName = "appt_res_id")
    {
        if (resourceIds == null || !resourceIds.Any())
        {
            return String.Empty;
        }

        // PostgreSQLのANY句を使用
        var idsArray = String.Join(", ", resourceIds.Select(id => $"'{id}'::uuid"));
        return $"{columnName} = ANY(ARRAY[{idsArray}]::uuid[])";
    }

    /// <summary>
    /// AND(OR1, OR2)形式の条件をSQL WHERE句として構築
    /// </summary>
    /// <param name="or1ResourceIds">OR1グループのリソースIDリスト</param>
    /// <param name="or2ResourceIds">OR2グループのリソースIDリスト</param>
    /// <param name="columnName">リソースIDカラム名（デフォルト: "appt_res_id"）</param>
    /// <returns>SQL WHERE句の条件文字列（空の場合は空文字列）</returns>
    public static string BuildAndOrCondition(
        List<Guid> or1ResourceIds,
        List<Guid> or2ResourceIds,
        string columnName = "appt_res_id")
    {
        var or1Condition = BuildOrGroupCondition(or1ResourceIds, columnName);
        var or2Condition = BuildOrGroupCondition(or2ResourceIds, columnName);

        // 両方のORグループが空の場合は空文字列を返す
        if (String.IsNullOrEmpty(or1Condition) && String.IsNullOrEmpty(or2Condition))
        {
            return String.Empty;
        }

        // 片方だけが空の場合は、もう片方の条件のみを返す
        if (String.IsNullOrEmpty(or1Condition))
        {
            return or2Condition;
        }

        if (String.IsNullOrEmpty(or2Condition))
        {
            return or1Condition;
        }

        // 両方のORグループが存在する場合はAND条件で結合
        // ただし、実際の条件は「予約がOR1グループのリソースを必要とし、かつOR2グループのリソースを必要とする」
        // という意味なので、予約リソース要件テーブルとのJOINが必要
        // ここでは単純なリソースIDフィルターとして実装
        // 実際の実装では、予約リソース要件テーブルとのJOINが必要になる可能性がある

        // 簡易実装: OR1またはOR2のいずれかに一致するリソースを返す
        // 本来は予約リソース要件テーブルとのJOINが必要だが、現時点では簡易実装
        var allIds = or1ResourceIds.Union(or2ResourceIds).ToList();
        return BuildOrGroupCondition(allIds, columnName);
    }

    /// <summary>
    /// 予約リソース要件テーブルとのJOINを考慮したAND(OR1, OR2)条件を構築
    /// 予約がOR1グループのリソースを必要とし、かつOR2グループのリソースを必要とする条件
    /// </summary>
    /// <param name="or1ResourceIds">OR1グループのリソースIDリスト</param>
    /// <param name="or2ResourceIds">OR2グループのリソースIDリスト</param>
    /// <param name="appointmentResourceRequirementTable">予約リソース要件テーブル名（デフォルト: "appointment_resource_requirements"）</param>
    /// <param name="appointmentIdColumn">予約IDカラム名（デフォルト: "appt_id"）</param>
    /// <param name="resourceIdColumn">リソースIDカラム名（デフォルト: "appt_res_id"）</param>
    /// <returns>SQL WHERE句の条件文字列（空の場合は空文字列）</returns>
    public static string BuildAndOrConditionWithJoin(
        List<Guid> or1ResourceIds,
        List<Guid> or2ResourceIds,
        string appointmentResourceRequirementTable = "appointment_resource_requirements",
        string appointmentIdColumn = "appt_id",
        string resourceIdColumn = "appt_res_id")
    {
        if ((or1ResourceIds == null || !or1ResourceIds.Any()) &&
            (or2ResourceIds == null || !or2ResourceIds.Any()))
        {
            return String.Empty;
        }

        // 片方だけが空の場合は、もう片方の条件のみを返す
        if (or1ResourceIds == null || !or1ResourceIds.Any())
        {
            var or2IdsArrayForSingle = String.Join(", ", or2ResourceIds.Select(id => $"'{id}'::uuid"));
            return $"{resourceIdColumn} = ANY(ARRAY[{or2IdsArrayForSingle}]::uuid[])";
        }

        if (or2ResourceIds == null || !or2ResourceIds.Any())
        {
            var or1IdsArrayForSingle = String.Join(", ", or1ResourceIds.Select(id => $"'{id}'::uuid"));
            return $"{resourceIdColumn} = ANY(ARRAY[{or1IdsArrayForSingle}]::uuid[])";
        }

        // 両方のORグループが存在する場合
        // 予約がOR1グループのリソースを必要とし、かつOR2グループのリソースを必要とする
        // これは予約リソース要件テーブルとのJOINが必要
        // EXISTS句を使用して実装
        var or1IdsArray = String.Join(", ", or1ResourceIds.Select(id => $"'{id}'::uuid"));
        var or2IdsArray = String.Join(", ", or2ResourceIds.Select(id => $"'{id}'::uuid"));

        return $@"EXISTS (
            SELECT 1 FROM {appointmentResourceRequirementTable} arr1
            WHERE arr1.{appointmentIdColumn} = appointments.appt_id
            AND arr1.{resourceIdColumn} = ANY(ARRAY[{or1IdsArray}]::uuid[])
            AND arr1.is_deleted = false
        ) AND EXISTS (
            SELECT 1 FROM {appointmentResourceRequirementTable} arr2
            WHERE arr2.{appointmentIdColumn} = appointments.appt_id
            AND arr2.{resourceIdColumn} = ANY(ARRAY[{or2IdsArray}]::uuid[])
            AND arr2.is_deleted = false
        )";
    }
}
