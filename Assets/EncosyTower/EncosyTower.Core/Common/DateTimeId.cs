namespace EncosyTower.Common
{
    using System;
    using System.Buffers;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using EncosyTower.Conversion;
    using EncosyTower.Serialization;

    [StructLayout(LayoutKind.Explicit)]
    [TypeConverter(typeof(TypeConverter))]
    public readonly partial record struct DateTimeId
        : IEquatable<DateTimeId>
        , IComparable<DateTimeId>
        , ITryParse<DateTimeId>
        , ITryParseSpan<DateTimeId>
    {
        public static readonly DateTimeId MaxValue = DateTime.MaxValue;
        public static readonly DateTimeId MinValue = DateTime.MinValue;
        public static readonly DateTimeId UnixEpoch = DateTime.UnixEpoch;

        [FieldOffset(0)]
        private readonly ulong _raw;

        [FieldOffset(0)]
        private readonly sbyte _zone;

        [FieldOffset(1)]
        private readonly byte _second;

        [FieldOffset(2)]
        private readonly byte _minute;

        [FieldOffset(3)]
        private readonly byte _hour;

        [FieldOffset(4)]
        private readonly byte _day;

        [FieldOffset(5)]
        private readonly byte _month;

        [FieldOffset(6)]
        private readonly ushort _year;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTimeId(ulong value) : this()
        {
            _raw = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId(ushort year, byte month, byte day) : this()
        {
            _year = year;
            _month = month;
            _day = day;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId(ushort year, byte month, byte day, byte hour, byte minute, byte second) : this()
        {
            _year = year;
            _month = month;
            _day = day;
            _hour = hour;
            _minute = minute;
            _second = second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId(in DateTime value) : this()
        {
            _second = (byte)value.Second;
            _minute = (byte)value.Minute;
            _hour = (byte)value.Hour;
            _day = (byte)value.Day;
            _month = (byte)value.Month;
            _year = (ushort)value.Year;
        }

        public static DateTimeId UtcNow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(DateTime.UtcNow);
        }

        public static DateTimeId Now
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(DateTime.Now);
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _year < UnixEpoch.Year;
        }

        public sbyte Zone
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _zone;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _zone = value;
        }

        public byte Second
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _second;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _second = value;
        }

        public byte Minute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _minute;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _minute = value;
        }

        public byte Hour
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hour;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _hour = value;
        }

        public byte Day
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _day;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _day = value;
        }

        public byte Month
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _month;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _month = value;
        }

        public ushort Year
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _year;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _year = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(
              out ushort year
            , out byte month
            , out byte day
            , out byte hour
            , out byte minute
            , out byte second
        )
        {
            year = _year;
            month = _month;
            day = _day;
            hour = _hour;
            minute = _minute;
            second = _second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ToDateTime()
            => (DateTime)this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _raw.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DateTimeId other)
            => _raw == other._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(DateTimeId other)
            => _raw.CompareTo(other._raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddDays(double value)
            => ((DateTime)this).AddDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddHours(double value)
            => ((DateTime)this).AddHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddMilliseconds(double value)
            => ((DateTime)this).AddMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddMinutes(double value)
            => ((DateTime)this).AddMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddMonths(int value)
            => ((DateTime)this).AddMonths(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddSeconds(double value)
            => ((DateTime)this).AddSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddTicks(long value)
            => ((DateTime)this).AddTicks(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeId AddYears(int value)
            => ((DateTime)this).AddYears(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse(
              string str
            , out DateTimeId result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            return TryParse(str.AsSpan(), out result, ignoreCase, allowMatchingMetadataAttribute);
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out DateTimeId result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            if (str.IsEmpty)
            {
                goto FAILED;
            }

            var datetimeRanges = str.Split('T');
            Range? dateRange = default;
            Range? timeRange = default;

            foreach (var range in datetimeRanges)
            {
                if (dateRange.HasValue == false)
                {
                    dateRange = range;
                    continue;
                }

                if (timeRange.HasValue == false)
                {
                    timeRange = range;
                    continue;
                }

                goto FAILED;
            }

            if (dateRange.HasValue == false || timeRange.HasValue == false)
            {
                goto FAILED;
            }

            var dateSpan = str[dateRange.Value];
            var dateRanges = dateSpan.Split('-');
            Range? yearRange = default;
            Range? monthRange = default;
            Range? dayRange = default;

            var timeSpan = str[timeRange.Value];
            var timeRanges = timeSpan.Split(':');
            Range? hourRange = default;
            Range? minuteRange = default;
            Range? secondRange = default;

            foreach (var range in dateRanges)
            {
                if (yearRange.HasValue == false)
                {
                    yearRange = range;
                    continue;
                }

                if (monthRange.HasValue == false)
                {
                    monthRange = range;
                    continue;
                }

                if (dayRange.HasValue == false)
                {
                    dayRange = range;
                    continue;
                }

                goto FAILED;
            }

            foreach (var range in timeRanges)
            {
                if (hourRange.HasValue == false)
                {
                    hourRange = range;
                    continue;
                }

                if (minuteRange.HasValue == false)
                {
                    minuteRange = range;
                    continue;
                }

                if (secondRange.HasValue == false)
                {
                    secondRange = range;
                    continue;
                }

                goto FAILED;
            }

            if (yearRange.HasValue == false
                || monthRange.HasValue == false
                || dayRange.HasValue == false
                || hourRange.HasValue == false
                || minuteRange.HasValue == false
                || secondRange.HasValue == false
            )
            {
                goto FAILED;
            }

            var yearStr = dateSpan[yearRange.Value];
            var monthStr = dateSpan[monthRange.Value];
            var dayStr = dateSpan[dayRange.Value];
            var hourStr = timeSpan[hourRange.Value];
            var minuteStr = timeSpan[minuteRange.Value];
            var secondStr = timeSpan[secondRange.Value];

            if (ushort.TryParse(yearStr, out var year)
                && byte.TryParse(monthStr, out var month)
                && byte.TryParse(dayStr, out var day)
                && byte.TryParse(hourStr, out var hour)
                && byte.TryParse(minuteStr, out var minute)
                && byte.TryParse(secondStr, out var second)
            )
            {
                result = new(year, month, day, hour, minute, second);
                return true;
            }

        FAILED:
            result = UnixEpoch;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ulong(in DateTimeId id)
            => id._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator DateTimeId(in ulong value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator DateTimeId(in DateTime value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator DateTime(in DateTimeId id)
            => new(id._year, id._month, id._day, id._hour, id._minute, id._second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in DateTimeId left, in DateTimeId right)
            => left._raw == right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in DateTimeId left, in DateTimeId right)
            => left._raw != right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in DateTimeId left, in DateTimeId right)
            => left._raw > right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in DateTimeId left, in DateTimeId right)
            => left._raw >= right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in DateTimeId left, in DateTimeId right)
            => left._raw < right._raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in DateTimeId left, in DateTimeId right)
            => left._raw <= right._raw;

        public sealed class TypeConverter : ParsableStructConverter<DateTimeId>
        {
            public override bool IgnoreCase => false;

            public override bool AllowMatchingMetadataAttribute => false;
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    partial record struct DateTimeId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        public FixedString128Bytes ToFixedString()
        {
            var fs = new FixedString128Bytes();
            fs.Append(_year);
            fs.Append('-');
            if (_month < 10) fs.Append('0');
            fs.Append(_month);
            fs.Append('-');
            if (_day < 10) fs.Append('0');
            fs.Append(_day);
            fs.Append('T');
            if (_hour < 10) fs.Append('0');
            fs.Append(_hour);
            fs.Append(':');
            if (_minute < 10) fs.Append('0');
            fs.Append(_minute);
            fs.Append(':');
            if (_second < 10) fs.Append('0');
            fs.Append(_second);
            return fs;
        }
    }
}

#else

namespace EncosyTower.Common
{
    using System.Runtime.CompilerServices;
    using System.Text;

    partial record struct DateTimeId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            var fs = new StringBuilder(32);
            fs.Append(_year);
            fs.Append('-');
            if (_month < 10) fs.Append('0');
            fs.Append(_month);
            fs.Append('-');
            if (_day < 10) fs.Append('0');
            fs.Append(_day);
            fs.Append('T');
            if (_hour < 10) fs.Append('0');
            fs.Append(_hour);
            fs.Append(':');
            if (_minute < 10) fs.Append('0');
            fs.Append(_minute);
            fs.Append(':');
            if (_second < 10) fs.Append('0');
            fs.Append(_second);
            return fs.ToString();
        }
    }
}

#endif
