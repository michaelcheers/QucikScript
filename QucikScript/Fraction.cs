using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QucikScript
{
    public struct Fraction
    {
        public BigInteger numerator;
        public BigInteger denominator;
        public Fraction (BigInteger numerator, BigInteger denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;
            Simplify();
        }

        public static Fraction Parse (string value)
        {
            var indexOfSlash = value.IndexOf('/');
            if (indexOfSlash != -1)
                return new Fraction(
                    BigInteger.Parse(value.Substring(0, indexOfSlash)),
                    BigInteger.Parse(value.Substring(indexOfSlash + 1)));
            var indexOfColon = value.IndexOf(':');
            if (indexOfColon != -1)
            {
                var whole = BigInteger.Parse(value.Substring(0, indexOfColon));
                var Decimal = value.Substring(indexOfColon + 1);
                var DecimalLength = Decimal.Length;
                var denominator = BigInteger.Pow(10, DecimalLength);
                return new Fraction(whole + BigInteger.Parse(Decimal), denominator);
            }
            return new Fraction(BigInteger.Parse(value), 1);
        }

        public BigInteger Floor () => numerator / denominator;
        public static Fraction operator / (Fraction a, Fraction b) => new Fraction(a.numerator * b.denominator, a.denominator * b.numerator);
        public static Fraction operator * (Fraction a, Fraction b) => new Fraction(a.numerator * b.numerator, a.denominator * b.denominator);
        public static Fraction operator + (Fraction a, Fraction b) => new Fraction(a.numerator * b.denominator + a.denominator * b.numerator, a.denominator * b.denominator);
        public static Fraction operator - (Fraction a, Fraction b) => new Fraction(a.numerator * b.denominator - a.denominator * b.numerator, a.denominator * b.denominator);
        public static Fraction operator % (Fraction a, Fraction b) => new Fraction((a.numerator * b.denominator) % (a.denominator * b.numerator), a.denominator * b.denominator);
        public static bool     operator < (Fraction a, Fraction b) => a.numerator * b.denominator <  a.denominator * b.numerator;
        public static bool     operator <=(Fraction a, Fraction b) => a.numerator * b.denominator <= a.denominator * b.numerator;
        public static bool     operator >=(Fraction a, Fraction b) => a.numerator * b.denominator >= a.denominator * b.numerator;
        public static bool     operator > (Fraction a, Fraction b) => a.numerator * b.denominator >  a.denominator * b.numerator;
        public static Fraction operator - (Fraction a)             => new Fraction(-a.numerator, a.denominator);
        public void Simplify ()
        {
            var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
            numerator /= gcd;
            denominator /= gcd;
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
        }

        public override string ToString()
        {
            if (denominator == 1)
                return numerator.ToString();
            return numerator + "/" + denominator;
        }

        internal static Fraction Log(Fraction a, Fraction b) =>
            new Fraction(new BigInteger(Math.Floor(BigInteger.Log(a.numerator, (double)b.numerator / (double)b.denominator) - BigInteger.Log(a.denominator, (double)b.Floor()))), 1);
    }
}
