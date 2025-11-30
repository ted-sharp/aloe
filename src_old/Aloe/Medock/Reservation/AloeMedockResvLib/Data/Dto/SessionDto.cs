using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

[MessagePackObject]
public class SessionDto
{
    [Key(0)]
    public required Guid SessionId { get; init; }

    [Key(1)]
    public required int UserId { get; init; }

    [Key(2)]
    public required string UserDisplayName { get; init; }

    [Key(3)]
    public required DateTime LoginAt { get; init; }
}

public static class SessionExtensions
{
    public static SessionDto ToSessionDto(this Session session)
    {
        return new SessionDto
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            UserDisplayName = session.UserDisplayName,
            LoginAt = session.LoginAt,
        };
    }
}
