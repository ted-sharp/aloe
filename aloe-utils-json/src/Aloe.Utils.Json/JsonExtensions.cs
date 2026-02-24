// <copyright file="JsonExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Aloe.Utils.Json;

/// <summary>
///     JSON操作のための拡張メソッドを提供します。
/// </summary>
public static class JsonExtensions
{
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "<Pending>")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver,
    };

    /// <summary>
    ///     オブジェクトをJSON文字列に変換します。
    /// </summary>
    /// <typeparam name="T">変換するオブジェクトの型。</typeparam>
    /// <param name="obj">変換するオブジェクト。</param>
    /// <param name="options">シリアル化オプション。nullの場合はDefaultOptionsが使用されます。</param>
    /// <returns>JSON文字列。</returns>
    /// <exception cref="ArgumentNullException">objがnullの場合。</exception>
    /// <exception cref="InvalidOperationException">シリアル化に失敗した場合。</exception>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation.")]
    public static string ToJson<T>(this T obj, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        try
        {
            return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to serialize object of type '{typeof(T).Name}' to JSON. " +
                $"Parameter: {nameof(obj)}. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    ///     オブジェクトをJSON文字列に変換します（AOT/トリミング対応版）。
    /// </summary>
    /// <typeparam name="T">変換するオブジェクトの型。</typeparam>
    /// <param name="obj">変換するオブジェクト。</param>
    /// <param name="jsonTypeInfo">シリアル化に使用する型情報。</param>
    /// <returns>JSON文字列。</returns>
    /// <exception cref="ArgumentNullException">objまたはjsonTypeInfoがnullの場合。</exception>
    /// <exception cref="InvalidOperationException">シリアル化に失敗した場合。</exception>
    public static string ToJson<T>(this T obj, JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        ArgumentNullException.ThrowIfNull(jsonTypeInfo, nameof(jsonTypeInfo));

        try
        {
            return JsonSerializer.Serialize(obj, jsonTypeInfo);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to serialize object of type '{typeof(T).Name}' to JSON using JsonTypeInfo. " +
                $"Parameters: {nameof(obj)}, {nameof(jsonTypeInfo)}. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換します。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <param name="options">デシリアル化オプション。nullの場合はDefaultOptionsが使用されます。</param>
    /// <returns>変換されたオブジェクト。</returns>
    /// <exception cref="ArgumentException">jsonがnullまたは空文字列の場合。</exception>
    /// <exception cref="InvalidOperationException">デシリアル化に失敗した場合。</exception>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation.")]
    public static T? ToObj<T>(this string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize JSON to object of type '{typeof(T).Name}'. " +
                $"Parameter: {nameof(json)}. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換します（AOT/トリミング対応版）。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <param name="jsonTypeInfo">デシリアル化に使用する型情報。</param>
    /// <returns>変換されたオブジェクト。</returns>
    /// <exception cref="ArgumentException">jsonがnullまたは空文字列の場合。</exception>
    /// <exception cref="ArgumentNullException">jsonTypeInfoがnullの場合。</exception>
    /// <exception cref="InvalidOperationException">デシリアル化に失敗した場合。</exception>
    public static T? ToObj<T>(this string json, JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));
        ArgumentNullException.ThrowIfNull(jsonTypeInfo, nameof(jsonTypeInfo));

        try
        {
            return JsonSerializer.Deserialize(json, jsonTypeInfo);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize JSON to object of type '{typeof(T).Name}' using JsonTypeInfo. " +
                $"Parameters: {nameof(json)}, {nameof(jsonTypeInfo)}. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換を試みます。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <param name="result">変換されたオブジェクト。変換に失敗した場合はdefault(T)。</param>
    /// <returns>変換に成功した場合はtrue、それ以外はfalse。</returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation.")]
    public static bool TryToObj<T>(this string json, [NotNullWhen(true)] out T? result)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            result = default;
            return false;
        }

        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return result is not null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換を試みます（AOT/トリミング対応版）。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <param name="jsonTypeInfo">デシリアル化に使用する型情報。</param>
    /// <param name="result">変換されたオブジェクト。変換に失敗した場合はdefault(T)。</param>
    /// <returns>変換に成功した場合はtrue、それ以外はfalse。</returns>
    public static bool TryToObj<T>(this string json, JsonTypeInfo<T> jsonTypeInfo, [NotNullWhen(true)] out T? result)
    {
        if (string.IsNullOrWhiteSpace(json) || jsonTypeInfo is null)
        {
            result = default;
            return false;
        }

        try
        {
            result = JsonSerializer.Deserialize(json, jsonTypeInfo);
            return result is not null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換します。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <returns>変換されたオブジェクト。</returns>
    /// <exception cref="ArgumentException">jsonがnullまたは空文字列の場合。</exception>
    /// <exception cref="InvalidOperationException">デシリアル化に失敗した場合。</exception>
    [Obsolete("Use ToObj<T>() instead for more consistent naming with ToJson<T>().")]
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation.")]
    public static T? FromJson<T>(this string json)
    {
        return json.ToObj<T>();
    }

    /// <summary>
    ///     JSON文字列をオブジェクトに変換します（AOT/トリミング対応版）。
    /// </summary>
    /// <typeparam name="T">変換先のオブジェクトの型。</typeparam>
    /// <param name="json">変換するJSON文字列。</param>
    /// <param name="jsonTypeInfo">デシリアル化に使用する型情報。</param>
    /// <returns>変換されたオブジェクト。</returns>
    /// <exception cref="ArgumentException">jsonがnullまたは空文字列の場合。</exception>
    /// <exception cref="ArgumentNullException">jsonTypeInfoがnullの場合。</exception>
    /// <exception cref="InvalidOperationException">デシリアル化に失敗した場合。</exception>
    [Obsolete("Use ToObj<T>(JsonTypeInfo<T>) instead for more consistent naming with ToJson<T>(JsonTypeInfo<T>).")]
    public static T? FromJson<T>(this string json, JsonTypeInfo<T> jsonTypeInfo)
    {
        return json.ToObj(jsonTypeInfo);
    }

    /// <summary>
    ///     JSON文字列を整形します。
    /// </summary>
    /// <param name="json">整形するJSON文字列。</param>
    /// <param name="options">シリアル化オプション。nullの場合はDefaultOptionsが使用されます。</param>
    /// <returns>整形されたJSON文字列。</returns>
    /// <exception cref="ArgumentException">jsonがnullまたは空文字列の場合。</exception>
    /// <exception cref="InvalidOperationException">JSON整形に失敗した場合。</exception>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation.")]
    public static string FormatJson(this string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, options ?? DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to format JSON string. " +
                $"Parameter: {nameof(json)}. See inner exception for details.",
                ex);
        }
    }
}
