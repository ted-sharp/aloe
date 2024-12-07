using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;

[MessagePackObject]
public class PolicyDto
{
    [Key(0)]
    public required string PolicyCode { get; set; } = String.Empty;

    [Key(2)]
    public required string PolicyName { get; set; } = String.Empty;

    [Key(3)]
    public required string DataType { get; set; } = String.Empty;

    [Key(4)]
    public required string PolicyValue { get; set; } = String.Empty;

    [Key(5)]
    public required string PolicyDesc { get; set; } = String.Empty;

    [Key(6)]
    public bool IsActive { get; set; } = false;
}

public static class PolicyExtensions
{
    public static PolicyDto ToPolicyDto(this Policy policy)
    {
        return new PolicyDto
        {
            PolicyCode = policy.PolicyCode,
            PolicyName = policy.PolicyName,
            DataType = policy.DataType,
            PolicyValue = policy.PolicyValue,
            PolicyDesc = policy.PolicyDesc,
            IsActive = policy.IsActive,
        };
    }
}
