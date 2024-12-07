using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion.Server;
using MagicOnion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

// 複数のアプリケーションロジックから参照される、共通のビジネスロジックを定義します。
public interface ISampleDomainService
{
}

public class SampleDomainService : ISampleDomainService
{
}
