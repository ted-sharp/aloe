using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// 設備
/// </summary>
[MessagePackObject]
public class ReservationEquipmentDto
{
    [Key(0)]
    public required int EquipId { get; set; }

    [Key(1)]
    public required string EquipName { get; set; }

    [Key(2)]
    public required int Seq { get; set; }
}

public static class ReservationEquipmentExtensions
{
    public static ReservationEquipmentDto ToReservationEquipmentDto(this ReservationEquipment equip)
    {
        return new ReservationEquipmentDto
        {
            EquipId = equip.EquipId,
            EquipName = equip.EquipName,
            Seq = equip.Seq,
        };
    }
}
