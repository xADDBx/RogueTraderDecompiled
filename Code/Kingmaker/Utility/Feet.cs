using System;
using Kingmaker.Blueprints.Root.Strings;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Utility;

[Serializable]
[JsonObject(IsReference = false)]
public struct Feet
{
	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	private float m_Value;

	public const float FeetToMetersRatio = 0.3048f;

	public float Meters => m_Value * 0.3048f;

	public int Value => (int)m_Value;

	public Feet(float feet)
	{
		m_Value = feet;
	}

	public override string ToString()
	{
		return $"{Value} {UIStrings.Instance.CommonTexts.Ft}.";
	}

	public bool Equals(Feet other)
	{
		return m_Value.Equals(other.m_Value);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Feet)
		{
			return Equals((Feet)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Value.GetHashCode();
	}

	public static Feet operator +(Feet f1, Feet f2)
	{
		return new Feet(f1.m_Value + f2.m_Value);
	}

	public static Feet operator -(Feet f1, Feet f2)
	{
		return new Feet(f1.m_Value - f2.m_Value);
	}

	public static Feet operator *(Feet f1, float multiplier)
	{
		return new Feet(f1.m_Value * multiplier);
	}

	public static Feet operator /(Feet f1, float divider)
	{
		return new Feet(f1.m_Value / divider);
	}

	public static bool operator ==(Feet f1, Feet f2)
	{
		return f1.Equals(f2);
	}

	public static bool operator !=(Feet f1, Feet f2)
	{
		return !(f1 == f2);
	}

	public static bool operator <=(Feet f1, Feet f2)
	{
		return f1.m_Value <= f2.m_Value;
	}

	public static bool operator >=(Feet f1, Feet f2)
	{
		return f1.m_Value >= f2.m_Value;
	}

	public static bool operator <(Feet f1, Feet f2)
	{
		return f1.m_Value < f2.m_Value;
	}

	public static bool operator >(Feet f1, Feet f2)
	{
		return f1.m_Value > f2.m_Value;
	}
}
