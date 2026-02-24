namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// 認証・認可エラーの場合にスローされます
/// </summary>
public class UnauthorizedException : MedockException
{
    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED")
    {
    }
}
