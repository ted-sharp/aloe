using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeExtImporter;

internal enum State
{
    /// <summary>
    /// 未処理
    /// </summary>
    None = 0,

    /// <summary>
    /// 処理中
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 完了
    /// </summary>
    Completed = 2,

    /// <summary>
    /// エラー
    /// </summary>
    Error = 3
}
