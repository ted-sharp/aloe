namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// 同時実行制御の競合が発生した場合にスローされます
/// </summary>
public class ConcurrencyException : MedockException
{
    public ConcurrencyException(string message)
        : base(message, "CONCURRENCY_ERROR")
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException, "CONCURRENCY_ERROR")
    {
    }
}
