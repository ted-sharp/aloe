// <copyright file="ValueStringBuilder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System;
using System.Buffers;

namespace Aloe.Utils.Text;

/// <summary>
/// <para>
/// Stackalloc と ArrayPool を組み合わせた高速かつ低GCの StringBuilder 構造体です。<br/>
/// 必ず <see cref="Dispose"/> でリソース（ArrayPoolから借りたバッファ）を返却してください。<br/>
/// <b>※<see cref="ToString()"/> はリソース解放を行いません。Disposeを呼ぶまで再利用可能です。</b>
/// </para>
/// <para>
/// <b>使用例:</b>
/// <code>
/// using (var sb = new ValueStringBuilder(stackalloc char[256]))
/// {
///     sb.Append("abc");
///     var s = sb.ToString();
/// }
/// </code>
/// </para>
/// </summary>
public ref struct ValueStringBuilder : IDisposable
{
    /// <summary>プールに返却する配列</summary>
    private char[]? _arrayToReturnToPool;

    /// <summary>文字バッファ</summary>
    private Span<char> _chars;

    /// <summary>現在の位置</summary>
    private int _pos;

    /// <summary>解放済みならtrue</summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct.
    /// </summary>
    /// <param name="initialBuffer">初期バッファ（例: stackalloc char[256]）</param>
    public ValueStringBuilder(Span<char> initialBuffer)
    {
        this._arrayToReturnToPool = null;
        this._chars = initialBuffer;
        this._pos = 0;
        this._disposed = false;
    }

    /// <summary>Gets 現在の長さを取得します。</summary>
    public readonly int Length => this._pos;

    /// <summary>Gets バッファ容量を取得します。</summary>
    public readonly int Capacity => this._chars.Length;

    /// <summary>
    /// 指定インデックスの文字を取得します。
    /// </summary>
    /// <param name="index">取得位置（0～Length-1）</param>
    /// <exception cref="ArgumentOutOfRangeException">index が 0 未満または Length 以上の場合にスローされます。</exception>
    public readonly char this[int index]
    {
        get
        {
            if ((uint)index >= (uint)this._pos)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return this._chars[index];
        }
    }

    /// <summary>
    /// 文字を追加します。
    /// </summary>
    /// <param name="c">追加する文字</param>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    public void Append(char c)
    {
        this.EnsureNotDisposed();

        if (this._pos >= this._chars.Length)
        {
            this.Grow(this._pos + 1);
        }

        this._chars[this._pos++] = c;
    }

    /// <summary>
    /// 文字列を追加します。
    /// </summary>
    /// <param name="s">追加する文字列</param>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    public void Append(string? s)
    {
        this.EnsureNotDisposed();
        if (String.IsNullOrEmpty(s))
        {
            return;
        }

        int required = this._pos + s.Length;
        if (required > this._chars.Length)
        {
            this.Grow(required);
        }

        s.AsSpan().CopyTo(this._chars.Slice(this._pos));
        this._pos += s.Length;
    }

    /// <summary>
    /// Spanから文字列を追加します。
    /// </summary>
    /// <param name="span">追加する文字列（Span）</param>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    public void Append(ReadOnlySpan<char> span)
    {
        this.EnsureNotDisposed();
        if (span.Length == 0)
        {
            return;
        }

        int required = this._pos + span.Length;
        if (required > this._chars.Length)
        {
            this.Grow(required);
        }

        span.CopyTo(this._chars.Slice(this._pos));
        this._pos += span.Length;
    }

    /// <summary>
    /// 格納済みの文字列を返します。
    /// </summary>
    /// <returns>構築された文字列</returns>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    public override string ToString()
    {
        this.EnsureNotDisposed();
        return this._chars.Slice(0, this._pos).ToString();
    }

    /// <summary>
    /// バッファ内容をSpanで取得します（参照のみ）。
    /// </summary>
    /// <returns>現在のバッファ内容を表すSpan</returns>
    public readonly Span<char> AsSpan() => this._chars.Slice(0, this._pos);

    /// <summary>
    /// 現在の内容をクリアします。
    /// </summary>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    public void Clear()
    {
        this.EnsureNotDisposed();
        this._chars.Slice(0, this._pos).Clear();
        this._pos = 0;
    }

    /// <summary>
    /// ArrayPoolから借りた配列を返却し、再利用不可にします。
    /// </summary>
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        if (this._arrayToReturnToPool != null)
        {
            ArrayPool<char>.Shared.Return(this._arrayToReturnToPool);
            this._arrayToReturnToPool = null;
        }

        this._disposed = true;
    }

    /// <summary>
    /// バッファサイズを拡張します。
    /// </summary>
    /// <param name="requiredCapacity">必要なサイズ</param>
    private void Grow(int requiredCapacity)
    {
        int newCapacity = Math.Max(this._chars.Length * 2, requiredCapacity);
        char[] newArray = ArrayPool<char>.Shared.Rent(newCapacity);
        this._chars.Slice(0, this._pos).CopyTo(newArray);

        if (this._arrayToReturnToPool != null)
        {
            ArrayPool<char>.Shared.Return(this._arrayToReturnToPool);
        }

        this._chars = newArray;
        this._arrayToReturnToPool = newArray;
    }

    /// <summary>
    /// Dispose後の操作に対して例外を投げる
    /// </summary>
    /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されている場合にスローされます。</exception>
    private void EnsureNotDisposed()
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException(nameof(ValueStringBuilder));
        }
    }
}
