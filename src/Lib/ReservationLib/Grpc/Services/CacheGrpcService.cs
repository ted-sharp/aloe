using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

// 起動時にマスタデータをある程度キャッシュしておきたい
// キャッシュの仕組みはどうする？
// ログイン時にキャッシュを更新する処理をいれておけば、ログインしなおしでよいかも？
