using System;
using System.Globalization;

namespace GamePrototype.LuckyScratch.Core
{
    /// <summary>
    /// 방치형 큰 숫자 포맷터. double 기반.
    /// 단위: (없음), K, M, B, T, 이후 aa, ab, ... az, ba, ... zz (1000^5 = aa).
    /// 예) 1234 → "1.23K", 5_000_000 → "5M", 1e15 → "1aa"
    /// </summary>
    public static class BigNumberFormatter
    {
        private static readonly string[] NamedUnits = { "", "K", "M", "B", "T" };
        private const int AlphaStartExponent = 5; // 1000^5 부터 aa

        public static string Format(double value, int significantDecimals = 2)
        {
            if (double.IsNaN(value)) return "0";
            if (double.IsInfinity(value)) return value > 0 ? "∞" : "-∞";

            bool negative = value < 0;
            double abs = Math.Abs(value);

            if (abs < 1000d)
            {
                // 1000 미만은 정수부 그대로 (소수 골드는 표시상 버림)
                string small = Math.Floor(abs).ToString(CultureInfo.InvariantCulture);
                return negative ? "-" + small : small;
            }

            int exponent = (int)Math.Floor(Math.Log(abs, 1000d));
            double scaled = abs / Math.Pow(1000d, exponent);

            // 반올림으로 1000이 되는 경계 처리 (예: 999999 → 1M)
            scaled = RoundToDecimals(scaled, significantDecimals);
            if (scaled >= 1000d)
            {
                exponent++;
                scaled /= 1000d;
            }

            string unit = GetUnit(exponent);
            string body = TrimTrailingZeros(scaled, significantDecimals);
            return (negative ? "-" : "") + body + unit;
        }

        public static string GetUnit(int exponent)
        {
            if (exponent <= 0) return "";
            if (exponent < NamedUnits.Length) return NamedUnits[exponent];

            int index = exponent - AlphaStartExponent; // 0 → aa
            int first = index / 26;
            int second = index % 26;
            if (first >= 26) return "e" + (exponent * 3); // zz 초과 시 과학표기 폴백
            return string.Concat((char)('a' + first), (char)('a' + second));
        }

        private static double RoundToDecimals(double v, int decimals)
        {
            double p = Math.Pow(10, decimals);
            return Math.Floor(v * p) / p; // 버림 — 방치형 관례(보유량 과대표시 방지)
        }

        private static string TrimTrailingZeros(double v, int decimals)
        {
            string s = v.ToString("F" + decimals, CultureInfo.InvariantCulture);
            if (decimals <= 0) return s;
            s = s.TrimEnd('0').TrimEnd('.');
            return s.Length == 0 ? "0" : s;
        }
    }
}
