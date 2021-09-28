﻿using System;
using System.Text.RegularExpressions;

namespace Calculator
{
    /// <summary>
    /// Save and arithmetics for big numbers.
    /// </summary>
    public class BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>
    {
        private static readonly BigNumber zero = new BigNumber("0");

        private bool _sign;

        private string _integer;

        private string _decimal;

        public string Value
        {
            get
            {
                string val = "";

                if (!_sign) { val += '-'; }
                val += _integer;
                if (_decimal != "") { val += '.' + _decimal; }

                return val;
            }
            set
            {
                if (!Regex.IsMatch(value, @"-?\d+\.*\d*"))
                {
                    throw new FormatException("String is not a rational number.");
                }

                _sign = value[0] != '-';

                int pointIndex = value.IndexOf('.');
                if (pointIndex > -1)
                {
                    _integer = value[(_sign ? 0 : 1)..pointIndex];
                    _decimal = value[(pointIndex + 1)..];
                }
                else
                {
                    _integer = value[(_sign ? 0 : 1)..];
                    _decimal = "";
                }

                _integer = _integer.TrimStart('0');
                if (_integer == "") { _integer = "0"; }

                _decimal = _decimal.TrimEnd('0');

                if (_integer == "0" && _decimal == "") { _sign = true; }
            }
        }

        public BigNumber(string Value)
        {
            this.Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Compares this instance to a specified BigNumber and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A BigNumber to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and <c>n</c>. Returns
        /// -1 if this instance is less than <c>n</c>, 0 if this instance is equal to <c>n</c> and
        /// 1 if this instance is greater than <c>n</c>.
        /// </returns>
        public int CompareTo(BigNumber other)
        {
            if (_sign != other._sign)
            {
                return _sign ? 1 : -1;
            }

            int result = 0;
            string i0 = _integer;
            string d0 = _decimal;
            string i1 = other._integer;
            string d1 = other._decimal;

            // Make lenght of numbers equal by adding non-significant 0s
            while (i1.Length < i0.Length) { i1 = '0' + i1; }
            while (i0.Length < i1.Length) { i0 = '0' + i0; }
            while (d1.Length < d0.Length) { d1 += '0'; }
            while (d0.Length < d1.Length) { d0 += '0'; }

            for (int i = 0; i < i0.Length; i++)
            {
                // No need to cast
                if (i0[i] > i1[i]) { result = 1; }
                else if (i0[i] < i1[i]) { result = -1; }
                if (result != 0) { break; }
            }
            if (result != 0) { return result; }

            for (int i = 0; i < d0.Length; i++)
            {
                // No need to cast
                if (d0[i] > d1[i]) { result = 1; }
                else if (d0[i] < d1[i]) { result = -1; }
                if (result != 0) { break; }
            }

            return _sign ? result : -result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BigNumber);
        }

        public bool Equals(BigNumber other)
        {
            return !(other is null) && (ReferenceEquals(this, other) || CompareTo(other) == 0);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static BigNumber operator +(BigNumber n)
        {
            return n;
        }

        public static BigNumber operator -(BigNumber n)
        {
            if (n.Value == "0") { return n; }

            BigNumber result = (BigNumber)n.MemberwiseClone();
            result._sign = !result._sign;
            return result;
        }

        public static bool operator ==(BigNumber n, BigNumber n1)
        {
            return ReferenceEquals(n, n1) || (!(n is null) && !(n1 is null) && n.Equals(n1));
        }

        public static bool operator !=(BigNumber n, BigNumber n1)
        {
            return !(n == n1);
        }

        public static bool operator <(BigNumber n, BigNumber n1)
        {
            return n.CompareTo(n1) == -1;
        }

        public static bool operator >(BigNumber n, BigNumber n1)
        {
            return n.CompareTo(n1) == 1;
        }

        public static bool operator <=(BigNumber n, BigNumber n1)
        {
            return n.CompareTo(n1) < 1;
        }

        public static bool operator >=(BigNumber n, BigNumber n1)
        {
            return n.CompareTo(n1) > -1;
        }

        private static BigNumber AbsSum(BigNumber n, BigNumber n1)
        {
            string i0 = n._integer;
            string d0 = n._decimal;
            string i1 = n1._integer;
            string d1 = n1._decimal;

            // Make lenght of numbers equal by adding non-significant 0s
            while (i1.Length < i0.Length) { i1 = '0' + i1; }
            while (i0.Length < i1.Length) { i0 = '0' + i0; }
            while (d1.Length < d0.Length) { d1 += '0'; }
            while (d0.Length < d1.Length) { d0 += '0'; }

            string rd = "";
            int carry = 0;
            int dlen = d0.Length;
            for (int i = dlen - 1; i >= 0; i--)
            {
                int sum = d0[i] - '0' + d1[i] - '0' + carry;
                if (sum > 9)
                {
                    sum -= 10;
                    carry = 1;
                }
                else { carry = 0; }
                rd = sum.ToString() + rd;
            }

            string ri = "";
            int ilen = i0.Length;
            for (int i = ilen - 1; i >= 0; i--)
            {
                int sum = i0[i] - '0' + i1[i] - '0' + carry;
                if (sum > 9)
                {
                    sum -= 10;
                    carry = 1;
                }
                else { carry = 0; }
                ri = sum.ToString() + ri;
            }

            if (carry == 1) { ri = '1' + ri; }

            return new BigNumber(ri + '.' + rd);
        }

        private static BigNumber AbsDif(BigNumber n, BigNumber n1)
        {
            string i0;
            string d0;
            string i1;
            string d1;

            // Put greater number in x0
            if (n.Abs() > n1.Abs())
            {
                i0 = n._integer;
                d0 = n._decimal;
                i1 = n1._integer;
                d1 = n1._decimal;
            }
            else
            {
                i0 = n1._integer;
                d0 = n1._decimal;
                i1 = n._integer;
                d1 = n._decimal;
            }

            // Make lenght of numbers equal by adding non-significant 0s
            while (i1.Length < i0.Length) { i1 = '0' + i1; }
            while (i0.Length < i1.Length) { i0 = '0' + i0; }
            while (d1.Length < d0.Length) { d1 += '0'; }
            while (d0.Length < d1.Length) { d0 += '0'; }

            string rd = "";
            int carry = 0;
            for (int i = d0.Length - 1; i >= 0; i--)
            {
                int dif = d0[i] - '0' - (d1[i] - '0') - carry;
                if (dif < 0)
                {
                    dif += 10;
                    carry = 1;
                }
                else { carry = 0; }
                rd = dif.ToString() + rd;
            }

            string ri = "";
            for (int i = i0.Length - 1; i >= 0; i--)
            {
                int dif = i0[i] - '0' - (i1[i] - '0') - carry;
                if (dif < 0)
                {
                    dif += 10;
                    carry = 1;
                }
                else { carry = 0; }
                ri = dif.ToString() + ri;
            }

            return new BigNumber(ri + '.' + rd);
        }

        public static BigNumber operator +(BigNumber n, BigNumber n1)
        {
            BigNumber rv;
            bool rs = true;

            rv = n._sign != n1._sign ? AbsDif(n, n1) : AbsSum(n, n1);

            if ((!n._sign && -n > n1) || (!n1._sign && -n1 > n))
            {
                rs = false;
            }

            return rs ? rv : -rv;
        }

        public static BigNumber operator -(BigNumber n, BigNumber n1)
        {
            BigNumber rv;
            bool rs = true;

            rv = n._sign == n1._sign ? AbsDif(n, n1) : AbsSum(n, n1);

            if ((!n._sign && n.Abs() > n1.Abs()) || (n1._sign && n1.Abs() > n.Abs()))
            {
                rs = false;
            }

            return rs ? rv : -rv;
        }

        public static BigNumber operator ++(BigNumber n)
        {
            return n + new BigNumber("1");
        }

        public static BigNumber operator --(BigNumber n)
        {
            return n - new BigNumber("1");
        }

        public static BigNumber operator *(BigNumber n, BigNumber n1)
        {
            BigNumber result = new BigNumber("0");
            BigNumber nAbs = n.Abs();
            BigNumber n1Abs = n1.Abs();
            string lev;
            string gtv;

            // Put number with smaller absolute value in lev
            if (nAbs > n1Abs)
            {
                lev = n1Abs.Value.Replace(".", "");
                gtv = nAbs.Value.Replace(".", "");
            }
            else
            {
                lev = nAbs.Value.Replace(".", "");
                gtv = n1Abs.Value.Replace(".", "");
            }

            int llen = lev.Length;
            int glen = gtv.Length;
            int carry = 0;
            for (int i = llen - 1; i >= 0; i--)
            {
                if (lev[i] == '0') { continue; }

                string mul = "";
                for (int j = 0; j < llen - 1 - i; j++) { mul += '0'; }

                for (int j = glen - 1; j >= 0; j--)
                {
                    int m = ((gtv[j] - '0') * (lev[i] - '0')) + carry;
                    if (m > 9)
                    {
                        carry = m / 10;
                        m %= 10;
                    }
                    else { carry = 0; }

                    mul = m.ToString() + mul;
                }
                if (carry > 0)
                {
                    mul = carry.ToString() + mul;
                    carry = 0;
                }

                result += new BigNumber(mul);
            }

            int pointIndex = nAbs._decimal.Length + n1Abs._decimal.Length;
            result = new BigNumber(result.Value.Insert(result.Value.Length - pointIndex, "."));
            return (n._sign ^ n1._sign) ? -result : result;
        }

        /// <summary>
        /// Returns the absolute value of this instance of BigNumber.
        /// </summary>
        /// <returns>A bigNumber.</returns>
        public BigNumber Abs()
        {
            return _sign ? this : -this;
        }
    }
}