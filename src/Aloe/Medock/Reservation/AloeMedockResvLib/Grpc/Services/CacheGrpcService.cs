using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion;
using MessagePack;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

// 起動時にマスタデータをある程度キャッシュしておきたい
// キャッシュの仕組みはどうする？
// ログイン時にキャッシュを更新する処理をいれておけば、ログインしなおしでよいかも？

// DBマスタ関連は、そもそもDBがメモリキャッシュしているので、ここでキャッシュする意味はないのでは？
// ClientCache みたいな感じだったらあり
