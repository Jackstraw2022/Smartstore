﻿#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Smartstore.Http;
using Smartstore.Utilities;

namespace Smartstore
{
    public static partial class StringExtensions
    {
        private readonly static TypeConverter _int32Converter = TypeDescriptor.GetConverter(typeof(int));
        private readonly static TypeConverter _singleConverter = TypeDescriptor.GetConverter(typeof(float));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this string? value, int defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return CommonHelper.TryAction(() => (int)_int32Converter.ConvertFromInvariantString(value)!);
            }

            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToChar(this string? value, bool unescape = false, char defaultValue = '\0')
        {
            if (!string.IsNullOrEmpty(value) && char.TryParse(unescape ? Regex.Unescape(value!) : value, out char result))
            {
                return result;
            }

            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(this string? value, float defaultValue = 0)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return CommonHelper.TryAction(() => (float)_singleConverter.ConvertFromInvariantString(value)!);
            }

            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBool(this string? value, bool defaultValue = false)
        {
            if (ConvertUtility.TryConvert(value, typeof(bool), out object? result))
            {
                return (bool)result!;
            }

            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTime(this string? value, DateTime? defaultValue)
        {
            return ToDateTime(value.AsSpan(), null, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTime(this ReadOnlySpan<char> value, DateTime? defaultValue)
        {
            return ToDateTime(value, null, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTime(this string? value, string[]? formats, DateTime? defaultValue)
        {
            return ToDateTime(value.AsSpan(), formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTime(this ReadOnlySpan<char> value, string[]? formats, DateTime? defaultValue)
        {
            return ToDateTime(value, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTime(this string? value, string[]? formats, IFormatProvider provider, DateTimeStyles styles, DateTime? defaultValue)
        {
            return ToDateTime(value.AsSpan(), formats, provider, styles, defaultValue);
        }

        public static DateTime? ToDateTime(this ReadOnlySpan<char> value, string[]? formats, IFormatProvider? provider, DateTimeStyles styles, DateTime? defaultValue)
        {
            DateTime result;

            if (formats.IsNullOrEmpty())
            {
                if (DateTime.TryParse(value, provider, styles, out result))
                {
                    return result;
                }
            }

            if (DateTime.TryParseExact(value, formats, provider, styles, out result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Parse ISO-8601 UTC timestamp including milliseconds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? ToDateTimeIso8601(this string? value)
        {
            return ToDateTimeIso8601(value.AsSpan());
        }

        /// <summary>
        /// Parse ISO-8601 UTC timestamp including milliseconds.
        /// </summary>
        public static DateTime? ToDateTimeIso8601(this ReadOnlySpan<char> value)
        {
            if (!value.IsWhiteSpace())
            {
                if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dt))
                    return dt;

                if (DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt))
                    return dt;
            }

            return null;
        }

        [DebuggerStepThrough]
        public static Version ToVersion(this string? value, Version? defaultVersion = null)
        {
            return ToVersion(value.AsSpan(), defaultVersion);
        }

        [DebuggerStepThrough]
        public static Version ToVersion(this ReadOnlySpan<char> value, Version? defaultVersion = null)
        {
            if (Version.TryParse(value, out var version))
            {
                return version;
            }
            
            return defaultVersion ?? new Version("1.0");
        }

        /// <summary>
        /// Encodes all the characters in the specified string into a sequence of bytes.
        /// </summary>
        /// <param name="value">String value to encode</param>
        /// <param name="encoding">The encoder to use. Default: <see cref="Encoding.UTF8"/></param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetBytes(this string value, Encoding? encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetBytes(value);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToEnum<T>(this string? value, T defaultValue)
            where T : struct
        {
            return Enum.TryParse<T>(value, out var enumValue) ? enumValue : defaultValue;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToEnum<T>(this ReadOnlySpan<char> value, T defaultValue)
            where T : struct
        {
            return Enum.TryParse<T>(value, out var enumValue) ? enumValue : defaultValue;
        }

        /// <summary>
        /// Computes the 32-bit XxHash32 of the input string.
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="seed">The seed value for this hash computation.</param>
        /// <returns>The XxHash32 hash of the input string in HEX.</returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? XxHash32(this string? value, int seed = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var hashCode = System.IO.Hashing.XxHash32.HashToUInt32(Encoding.UTF8.GetBytes(value), seed);
            return $"{hashCode:X}";
        }

        /// <summary>
        /// Computes the 64-bit XxHash3 of the input string.
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="seed">The seed value for this hash computation.</param>
        /// <returns>The XxHash3 hash of the input string in HEX.</returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? XxHash3(this string? value, long seed = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var hashCode = System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(value), seed);
            return $"{hashCode:X}";
        }

        /// <summary>
        /// Computes the 64-bit XxHash64 of the input string.
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="seed">The seed value for this hash computation.</param>
        /// <returns>The XxHash64 hash of the input string in HEX.</returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? XxHash64(this string? value, long seed = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var hashCode = System.IO.Hashing.XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(value), seed);
            return $"{hashCode:X}";
        }


        [DebuggerStepThrough]
        [Obsolete("Microsoft recommends SHA256 or SHA512 class instead of MD5. Use MD5 only for compatibility with legacy applications and data.")]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? Hash(this string? value, Encoding encoding, bool toBase64 = false)
        {
            if (value.IsEmpty())
            {
                return value;
            }
            byte[] data = encoding.GetBytes(value!);

            if (toBase64)
            {
                byte[] hash = MD5.HashData(data);
                return Convert.ToBase64String(hash);
            }
            else
            {
                return MD5.HashData(data).ToHexString();
            }
        }

        /// <summary>
        /// Converts an uri component to its escaped representation.
        /// Don't pass full uris to this method because it will also escape reserved chars (/?=#.&:;$@),
        /// pass only fragments like query key/value, filename etc.
        /// </summary>
        /// <param name="value">The uri component to escape.</param>
        /// <returns>The escaped uri component.</returns>
        /// <remarks>This method internally calls <see cref="Uri.EscapeDataString(string)"/>.</remarks>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? UrlEncode(this string? value)
            => value.IsEmpty() ? value : Uri.EscapeDataString(value!);

        /// <summary>
        /// Converts an escaped uri component to its unescaped representation.
        /// </summary>
        /// <param name="value">The uri component to unescape.</param>
        /// <returns>The unescaped uri component.</returns>
        /// <remarks>This method internally calls <see cref="Uri.UnescapeDataString(string)"/>.</remarks>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? UrlDecode(this string? value)
            => value.IsEmpty() ? value : Uri.UnescapeDataString(value!);

        /// <summary>
        /// Encodes a string for use in HTML attributes.
        /// </summary>
        /// <param name="useDefaultEncoder">
        /// Whether to use the system default HTML attribute encoder. 
        /// If true, the method will use <see cref="HttpUtility.HtmlAttributeEncode(string?)"/> for encoding.
        /// If false, it will use a custom implementation that replaces only specific characters:
        /// " and ' by ’, \ by /, & by +, &lt; is removed.
        /// </param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? AttributeEncode(this string? value, bool useDefaultEncoder = true)
        {
            if (useDefaultEncoder)
            {
                return HttpUtility.HtmlAttributeEncode(value);
            }
            
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Don't create string writer if we don't have nothing to encode
            int pos = value.AsSpan().IndexOfAny("<\"'&");
            if (pos < 0)
            {
                return value;
            }

            var output = new StringWriter(CultureInfo.InvariantCulture);

            // Write up to first encodable char
            output.Write(value.AsSpan(0, pos));

            var remaining = value.AsSpan(pos);
            for (int i = 0; i < remaining.Length; i++)
            {
                char ch = remaining[i];
                if (ch <= '<')
                {
                    switch (ch)
                    {
                        case '<':
                            break;
                        case '"':
                            output.Write('’');
                            break;
                        case '\'':
                            output.Write('/');
                            break;
                        case '&':
                            output.Write('+');
                            break;
                        default:
                            output.Write(ch);
                            break;
                    }
                }
                else
                {
                    output.Write(ch);
                }
            }

            return output.ToString();
        }

        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? HtmlEncode(this string? value)
        {
            if (value is null)
            {
                return null;
            }
            
            return WebHelper.HtmlEncoder.Encode(value!);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(value))]
        public static string? HtmlDecode(this string? value)
            => HttpUtility.HtmlDecode(value);

        /// <inheritdoc cref="EncodeJsString(string?, char, bool)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EncodeJsStringUnquoted(this string? value, bool stripLineBreaks = true)
            => EncodeJsString(value, null, stripLineBreaks);

        /// <summary>
        /// Encodes a string value for use in JavaScript string literals.
        /// </summary>
        /// <param name="value">The string value to encode.</param>
        /// <param name="quoteDelimiter">The character used as the quote delimiter. Default: '"'</param>
        /// <param name="stripLineBreaks">If true, line breaks will be stripped.</param>
        /// <returns>The encoded JavaScript string.</returns>
        public static string EncodeJsString(this string? value, char? quoteDelimiter = '"', bool stripLineBreaks = true)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value.EmptyNull();
            }

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\':  // Backslash
                        sb.Append("\\\\");
                        break;
                    case '"':   // Double quotes
                        sb.Append("\\\"");
                        break;
                    case '\'':  // Single quotes
                        sb.Append("\\'");
                        break;
                    case '\n':  // New line
                        if (!stripLineBreaks) sb.Append("\\n");
                        break;
                    case '\r':  // New line
                        if (!stripLineBreaks) sb.Append("\\r");
                        break;
                    case '<':   // XSS
                        sb.Append("\\x3C");
                        break;
                    case '>':   // XSS
                        sb.Append("\\x3E");
                        break;
                    case '&':   // XSS
                        sb.Append("\\x26");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            if (quoteDelimiter.HasValue && quoteDelimiter != '\0')
            {
                sb.Insert(0, quoteDelimiter);
                sb.Append(quoteDelimiter);
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SanitizeHtmlId(this string? value)
        {
            return TagBuilder.CreateSanitizedId(value, "_");
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToIntArray(this string? value)
        {
            return value.Convert<int[]>() ?? [];
        }
    }
}
