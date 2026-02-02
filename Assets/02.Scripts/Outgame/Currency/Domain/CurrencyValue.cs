using System;
using UnityEngine;

[Serializable]
public struct CurrencyValue : IEquatable<CurrencyValue>, IComparable<CurrencyValue>
{
    [SerializeField] private double _value;

    public double Value => _value;

    public CurrencyValue(double value)
    {
        _value = value;
    }

    // 명시적 변환 (double로 변환 시 의도적으로 해야 함)
    public static explicit operator double(CurrencyValue v) => v.Value;
    public double ToDouble() => Value;

    // 암시적 변환 (기본 타입 → CurrencyValue는 허용)
    public static implicit operator CurrencyValue(double v) => new(v);
    public static implicit operator CurrencyValue(int v) => new(v);
    public static implicit operator CurrencyValue(long v) => new(v);

    // 산술 연산자
    public static CurrencyValue operator +(CurrencyValue a, CurrencyValue b) => new(a.Value + b.Value);
    public static CurrencyValue operator -(CurrencyValue a, CurrencyValue b) => new(a.Value - b.Value);
    public static CurrencyValue operator *(CurrencyValue a, CurrencyValue b) => new(a.Value * b.Value);
    public static CurrencyValue operator /(CurrencyValue a, CurrencyValue b) => new(a.Value / b.Value);

    // 비교 연산자
    public static bool operator ==(CurrencyValue a, CurrencyValue b) => Math.Abs(a.Value - b.Value) < double.Epsilon;
    public static bool operator !=(CurrencyValue a, CurrencyValue b) => !(a == b);
    public static bool operator >(CurrencyValue a, CurrencyValue b) => a.Value > b.Value;
    public static bool operator <(CurrencyValue a, CurrencyValue b) => a.Value < b.Value;
    public static bool operator >=(CurrencyValue a, CurrencyValue b) => a.Value >= b.Value;
    public static bool operator <=(CurrencyValue a, CurrencyValue b) => a.Value <= b.Value;

    // IEquatable, IComparable
    public bool Equals(CurrencyValue other) => this == other;
    public int CompareTo(CurrencyValue other) => Value.CompareTo(other.Value);

    public override bool Equals(object obj) => obj is CurrencyValue other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("N0");

    // 상수
    public static CurrencyValue Zero => new(0);
    public static CurrencyValue One => new(1);

    // 상태 확인
    public bool IsZero => this == Zero;
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;

    // 수학 연산
    public CurrencyValue Floor() => new(Math.Floor(Value));
    public CurrencyValue Ceiling() => new(Math.Ceiling(Value));
    public CurrencyValue Round() => new(Math.Round(Value));
    public CurrencyValue Abs() => new(Math.Abs(Value));

    public CurrencyValue Pow(double exponent) => new(Math.Pow(Value, exponent));
    public static CurrencyValue Pow(CurrencyValue baseValue, double exponent) => baseValue.Pow(exponent);
    public static CurrencyValue Max(CurrencyValue a, CurrencyValue b) => a.Value >= b.Value ? a : b;
    public static CurrencyValue Min(CurrencyValue a, CurrencyValue b) => a.Value <= b.Value ? a : b;

    // 포맷팅 (UI용)
    public string ToFormattedString()
    {
        if (Value >= 1_000_000_000) return $"{Value / 1_000_000_000:F1}B";
        if (Value >= 1_000_000) return $"{Value / 1_000_000:F1}M";
        if (Value >= 1_000) return $"{Value / 1_000:F1}K";
        return Value.ToString("N0");
    }
}
