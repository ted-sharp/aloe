using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// 最低限の項目を扱うための DTO(Data Transfer Object) クラスです。
/// gRPC通信でシリアライズ/デシリアライズを行うため、MessagePackの属性を付与しています。
/// gRPC以外でDtoを必要とする場合は ValueTuple を検討します。
/// </summary>
[MessagePackObject]
public class SampleDto
{
    [Key(0)]
    public required int SampleId { get; set; }

    [Key(1)]
    public required string Name { get; set; }
}
