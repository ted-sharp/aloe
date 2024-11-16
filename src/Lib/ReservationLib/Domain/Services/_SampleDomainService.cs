using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion.Server;
using MagicOnion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Api.ReservationServer.Grpc.Services;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AloeReservationGrid.Lib.ReservationLib.Domain.Services;

// 複数のアプリケーションロジックから参照される、共通のビジネスロジックを定義します。
public interface ISampleDomainService
{
}

public class SampleDomainService : ISampleDomainService
{
}
