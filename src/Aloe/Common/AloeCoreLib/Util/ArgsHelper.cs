using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public static class ArgsHelper
{

    /// <summary>
    /// コマンドライン引数を前処理します。
    /// オプションが単独('--IsFlag')で bool を表す場合、値が指定されていなければ自動的に "true" を補完します。('--IsFlag' → '--IsFlag true')
    /// 短いオプション('-a')で、オプションと値が連結している場合は分割します。('-avalue' → '-a value')
    /// </summary>
    public static string[] PreprocessArgs(string[] args, IEnumerable<string> flagArgs, IEnumerable<string> shortArgs)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        ArgumentNullException.ThrowIfNull(flagArgs, nameof(flagArgs));
        ArgumentNullException.ThrowIfNull(shortArgs, nameof(shortArgs));

        var processedArgs = new List<string>();

        var flags = flagArgs.ToList();
        var shorts = shortArgs.ToList();

        for (var i = 0; i < args.Length; i++)
        {
            var currentArg = args[i];

            if (flags.Contains(currentArg))
            {
                processedArgs.Add(currentArg);

                // 次のトークンが存在しない、または '-' で始まるなら "true" を付与
                if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
                {
                    processedArgs.Add("true");
                }

                continue;
            }

            // Shorts に該当するかチェック
            var isShortOptionHandled = false;
            foreach (var shortOpt in shorts)
            {
                if (currentArg.StartsWith(shortOpt))
                {
                    // 短いオプションそのものだけの場合はそのまま追加
                    if (currentArg.Length == shortOpt.Length)
                    {
                        processedArgs.Add(currentArg);
                    }
                    else
                    {
                        // オプションと値が連結されている場合
                        // 例: "-uadmin" → "-u" "admin"
                        // 例: "-ppwd" → "-p" "pwd"
                        processedArgs.Add(shortOpt);
                        processedArgs.Add(currentArg.Substring(shortOpt.Length));
                    }
                    isShortOptionHandled = true;
                    break;
                }
            }

            if (!isShortOptionHandled)
            {
                // 上記に該当しない場合はそのまま追加
                processedArgs.Add(currentArg);
            }
        }

        return processedArgs.ToArray();
    }
}
