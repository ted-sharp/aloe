namespace Aloe.Apps.MedockLib.Common;

/// <summary>
/// 汎用的な操作結果を表すクラス
/// </summary>
/// <typeparam name="T">結果の値の型</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public T? Value { get; init; }

    private Result(bool isSuccess, T? value, string? errorMessage, string? errorCode)
    {
        this.IsSuccess = isSuccess;
        this.Value = value;
        this.ErrorMessage = errorMessage;
        this.ErrorCode = errorCode;
    }

    /// <summary>
    /// 成功結果を作成します
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null);
    }

    /// <summary>
    /// 失敗結果を作成します
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new Result<T>(false, default, errorMessage, errorCode);
    }
}

/// <summary>
/// 値を返さない操作結果
/// </summary>
public class Result
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    private Result(bool isSuccess, string? errorMessage, string? errorCode)
    {
        this.IsSuccess = isSuccess;
        this.ErrorMessage = errorMessage;
        this.ErrorCode = errorCode;
    }

    public static Result Success()
    {
        return new Result(true, null, null);
    }

    public static Result Failure(string errorMessage, string? errorCode = null)
    {
        return new Result(false, errorMessage, errorCode);
    }
}
