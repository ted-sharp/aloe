namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// Medockシステムの基底例外クラス
/// </summary>
public class MedockException : Exception
{
    public string ErrorCode { get; }

    public MedockException(string message, string errorCode = "MEDOCK_ERROR")
        : base(message)
    {
        this.ErrorCode = errorCode;
    }

    public MedockException(string message, Exception innerException, string errorCode = "MEDOCK_ERROR")
        : base(message, innerException)
    {
        this.ErrorCode = errorCode;
    }
}
