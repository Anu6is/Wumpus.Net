﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Utf8;

namespace Wumpus.Serialization
{
    public static class BufferExtensions
    {
        private static readonly ParsedFormat _numberFormat = new ParsedFormat('D');

        public static sbyte ParseInt8(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseSByte(text, out sbyte result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse Int8");
        }
        public static short ParseInt16(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseInt16(text, out short result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse Int16");
        }
        public static int ParseInt32(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseInt32(text, out int result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse Int32");
        }
        public static long ParseInt64(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseInt64(text, out long result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse Int64");
        }

        public static byte ParseUInt8(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseByte(text, out byte result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse UInt8");
        }
        public static ushort ParseUInt16(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseUInt16(text, out ushort result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse UInt16");
        }
        public static uint ParseUInt32(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseUInt32(text, out uint result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse UInt32");
        }
        public static ulong ParseUInt64(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseUInt64(text, out ulong result, out int ignored, _numberFormat, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse UInt64");
        }

        /*public static char ParseChar(this ReadOnlySpan<byte> text)
        {
            string str = ParseString(text);
            if (char.TryParse(str, out char c))
                return c;
            throw new SerializationException("Failed to parse Char");
        }*/
        public static string ParseString(this ReadOnlySpan<byte> text) => new Utf8String(text).ToString();

        public static float ParseSingle(this ReadOnlySpan<byte> text)
        {
            //TODO: Allocs a string
            if (float.TryParse(ParseString(text), out float result))
                return result;
            throw new SerializationException("Failed to parse Single");
        }
        public static double ParseDouble(this ReadOnlySpan<byte> text)
        {
            //TODO: Allocs a string
            if (double.TryParse(ParseString(text), out double result))
                return result;
            throw new SerializationException("Failed to parse Double");
        }
        public static decimal ParseDecimal(this ReadOnlySpan<byte> text)
        {
            //TODO: Allocs a string
            if (decimal.TryParse(ParseString(text), out decimal result))
                return result;
            throw new SerializationException("Failed to parse Decimal");
        }

        public static DateTime ParseDateTime(this ReadOnlySpan<byte> text)
        {
            if (TryParseDateTime(text, out var result, out int ignored))
                return result;
            throw new SerializationException("Failed to parse DateTime");
        }
        public static DateTimeOffset ParseDateTimeOffset(this ReadOnlySpan<byte> text)
        {
            if (TryParseDateTimeOffset(text, out var result, out int ignored))
                return result;
            throw new SerializationException("Failed to parse DateTimeOffset");
        }

        private static bool TryParseDateTime(ReadOnlySpan<byte> text, out DateTime value, out int bytesConsumed)
        {
            int index = 0;
            bytesConsumed = 0;
            if (!TryParseDateParts(text, ref index, out int year, out int month, out int day) ||
                !TryParseTimeParts(text, ref index, out int hour, out int min, out int sec, out int milli, out int milliLength) ||
                !TryParseTimezoneParts(text, ref index, out var offset))
            {
                value = default;
                return false;
            }

            if (milliLength == 6)
                milli /= 1000;

            value = new DateTime(year, month, day, hour, min, sec, milli, DateTimeKind.Utc);
            if (offset != TimeSpan.Zero)
                value -= offset;
            return true;
        }
        private static bool TryParseDateTimeOffset(ReadOnlySpan<byte> text, out DateTimeOffset value, out int bytesConsumed)
        {
            int index = 0;
            bytesConsumed = 0;
            if (!TryParseDateParts(text, ref index, out int year, out int month, out int day) ||
                !TryParseTimeParts(text, ref index, out int hour, out int min, out int sec, out int milli, out int milliLength) ||
                !TryParseTimezoneParts(text, ref index, out var offset))
            {
                value = default;
                return false;
            }

            if (milliLength == 6)
                milli /= 1000;

            value = new DateTimeOffset(year, month, day, hour, min, sec, milli, offset);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseDateParts(ReadOnlySpan<byte> text, ref int index, 
            out int year, out int month, out int day)
        {
            year = 0;
            month = 0;
            day = 0;

            //Format: YYYY-MM-DD
            if (text.Length < 10 ||
                !TryParseNumericPart(text, ref index, out year, out var ignored, 4) ||
                text[index++] != (byte)'-' ||
                !TryParseNumericPart(text, ref index, out month, out ignored, 2) ||
                text[index++] != (byte)'-' ||
                !TryParseNumericPart(text, ref index, out day, out ignored, 2))
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseTimeParts(ReadOnlySpan<byte> text, ref int index,
            out int hour, out int minute, out int second, out int millisecond, out int milliLength)
        {
            hour = 0;
            minute = 0;
            second = 0;
            millisecond = 0;
            milliLength = 0;

            //Time (hh:mm)
            if (text.Length < 16 || text[index] != (byte)'T')  //0001-01-01T01:01
                return true;
            index++;

            if (!TryParseNumericPart(text, ref index, out hour, out var ignored, 2) ||
                text[index++] != (byte)':' ||
                !TryParseNumericPart(text, ref index, out minute, out ignored, 2))
                return false;

            //Time (hh:mm:ss)
            if (text.Length < 19 || text[index] != (byte)':')  //0001-01-01T01:01:01
                return true;
            index++;

            if (!TryParseNumericPart(text, ref index, out second, out ignored, 2))
                return false;

            //Time (hh:mm:ss.sss)
            if (text.Length < 21 || text[index] != (byte)'.')  //0001-01-01T01:01:01.1
                return true;
            index++;
            
            if (!TryParseNumericPart(text, ref index, out millisecond, out milliLength, 6))
                return false;

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseTimezoneParts(ReadOnlySpan<byte> text, ref int index,
            out TimeSpan offset)
        {
            offset = default;

            int remaining = text.Length - index;
            if (remaining == 1) //Z
            {
                if (text[index] != 'Z')
                    return false;
                return true;
            }
            else if (remaining == 6) //+00:00
            {
                bool isNegative = text[index] == (byte)'-';
                if (!isNegative && text[index] != (byte)'+')
                    return false;
                index++;
                
                if (!TryParseNumericPart(text, ref index, out int hours, out var ignored, 2) ||
                    text[index++] != (byte)':' ||
                    !TryParseNumericPart(text, ref index, out int minutes, out ignored, 2))
                    return false;
                offset = new TimeSpan(hours, minutes, 0);
                if (isNegative) offset = -offset;
                return true;
            }
            else
                return false;
        }

        //From https://github.com/dotnet/corefxlab/blob/master/src/System.Text.Primitives/System/Text/Parsing/Unsigned.cs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseNumericPart(ReadOnlySpan<byte> text, ref int index, out int value, out int valueLength, int maxLength)
        {
            // Parse the first digit separately. If invalid here, we need to return false.
            uint firstDigit = text[index++] - 48u; // '0'
            if (firstDigit > 9)
            {
                valueLength = 0;
                value = default;
                return false;
            }
            valueLength = 1;
            uint parsedValue = firstDigit;
            
            for (int i = 1; i < maxLength && index < text.Length; i++, index++)
            {
                uint nextDigit = text[index] - 48u; // '0'
                if (nextDigit > 9)
                {
                    value = (int)(parsedValue);
                    return true;
                }
                valueLength++;
                parsedValue = parsedValue * 10 + nextDigit;
            }
            
            value = (int)(parsedValue);
            return true;
        }

        public static bool ParseBool(this ReadOnlySpan<byte> text)
        {
            if (PrimitiveParser.TryParseBoolean(text, out bool result, out int ignored, SymbolTable.InvariantUtf8))
                return result;
            throw new SerializationException("Failed to parse Boolean");
        }
        public static Guid ParseGuid(this ReadOnlySpan<byte> text)
        {
            //TODO: Allocs a string
            if (Guid.TryParse(text.ParseString(), out var result))
                return result;
            throw new SerializationException("Failed to parse Guid");
        }
    }
}
