namespace Aloe.Apps.MedockLib.Common.Exceptions;

/// <summary>
/// エンティティが見つからない場合にスローされます
/// </summary>
public class NotFoundException : MedockException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
    }

    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found", "NOT_FOUND")
    {
    }
}
