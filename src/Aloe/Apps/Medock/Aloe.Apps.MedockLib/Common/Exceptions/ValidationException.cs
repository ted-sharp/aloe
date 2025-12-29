namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// バリデーションエラーの場合にスローされます
/// </summary>
public class ValidationException : MedockException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base(message, "VALIDATION_ERROR")
    {
        this.Errors = errors ?? new Dictionary<string, string[]>();
    }
}
