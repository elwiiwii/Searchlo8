// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Searchlo8
{
    public struct Pdouble(double value)
    {
        private const int DecimalPlaces = 4;
        private readonly double _value = Math.Round(value, DecimalPlaces, MidpointRounding.ToEven);

        //
        // Public Constants
        //
        public const double MinValue = -32768.0;
        public const double MaxValue = 32767.9999;

        /// <summary>Represents the additive identity (0).</summary>
        internal const double AdditiveIdentity = 0.0;

        /// <summary>Represents the multiplicative identity (1).</summary>
        internal const double MultiplicativeIdentity = 1.0;

        /// <summary>Represents the number one (1).</summary>
        internal const double One = 1.0;

        /// <summary>Represents the number zero (0).</summary>
        internal const double Zero = 0.0;

        /// <summary>Represents the number negative one (-1).</summary>
        internal const double NegativeOne = -1.0;

        /// <summary>Represents the number negative zero (-0).</summary>
        public const double NegativeZero = -0.0;

        /// <summary>Represents the natural logarithmic base, specified by the constant, e.</summary>
        /// <remarks>Euler's number is approximately 2.7182818284590452354.</remarks>
        public const double E = 2.7183;

        /// <summary>Represents the ratio of the circumference of a circle to its diameter, specified by the constant, PI.</summary>
        /// <remarks>Pi is approximately 3.1415926535897932385.</remarks>
        public const double Pi = 3.1416;

        /// <summary>Represents the number of radians in one turn, specified by the constant, Tau.</summary>
        /// <remarks>Tau is approximately 6.2831853071795864769.</remarks>
        public const double Tau = 6.2832;

        // conversion from double
        public static implicit operator Pdouble(double value) => new(value);

        // conversion to double
        public static explicit operator double(Pdouble p) => p._value;

        public static Pdouble operator +(Pdouble left, Pdouble right) => new(left._value + right._value);
        public static Pdouble operator -(Pdouble left) => new(-left._value);
        public static Pdouble operator -(Pdouble left, Pdouble right) => new(left._value - right._value);
        public static Pdouble operator *(Pdouble left, Pdouble right) => new(left._value * right._value);
        public static Pdouble operator /(Pdouble left, Pdouble right) => new(left._value / right._value);

        public static bool operator ==(Pdouble left, Pdouble right) => left._value.Equals(right._value);
        public static bool operator !=(Pdouble left, Pdouble right) => !left._value.Equals(right._value);
        public static bool operator <(Pdouble left, Pdouble right) => left._value < right._value;
        public static bool operator >(Pdouble left, Pdouble right) => left._value > right._value;
        public static bool operator <=(Pdouble left, Pdouble right) => left._value <= right._value;
        public static bool operator >=(Pdouble left, Pdouble right) => left._value >= right._value;

        public override readonly string ToString() => _value.ToString($"F{DecimalPlaces}");
        public override readonly bool Equals(object? obj) => obj is Pdouble other && Equals(other);
        public override readonly int GetHashCode() => _value.GetHashCode();

    }
}
