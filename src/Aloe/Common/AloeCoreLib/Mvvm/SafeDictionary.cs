using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Mvvm;

public class SafeDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly TValue _defaultValue;

    public SafeDictionary(TValue defaultValue = default!)
    {
        this._defaultValue = defaultValue;
    }

    public new TValue this[TKey key]
    {
        get => this.GetValueOrDefault(key, this._defaultValue);
        set => base[key] = value;
    }
}
