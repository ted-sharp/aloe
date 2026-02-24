namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// データベース操作エラーの場合にスローされます
/// </summary>
public class DatabaseException : MedockException
{
    public DatabaseException(string message)
        : base(message, "DATABASE_ERROR")
    {
    }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException, "DATABASE_ERROR")
    {
    }
}
