using System;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility;

[Serializable]
[JsonObject(IsReference = false)]
[HashRoot]
public struct Rounds : IHashable
{
	public static readonly Rounds Infinity = new Rounds(int.MaxValue);

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	private int m_Value;

	public int Value => m_Value;

	public TimeSpan Seconds => ((float)m_Value * 5f).Seconds();

	public Rounds(int value)
	{
		m_Value = value;
	}

	public bool Equals(Rounds other)
	{
		return m_Value == other.m_Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is Rounds other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Value;
	}

	public override string ToString()
	{
		return m_Value.ToString();
	}

	public static Rounds operator +(Rounds v1, Rounds v2)
	{
		return new Rounds(v1.m_Value + v2.m_Value);
	}

	public static Rounds operator -(Rounds v1, Rounds v2)
	{
		return new Rounds(v1.m_Value - v2.m_Value);
	}

	public static Rounds operator *(Rounds v1, int multiplier)
	{
		return new Rounds(v1.m_Value * multiplier);
	}

	public static Rounds operator /(Rounds v1, int divider)
	{
		return new Rounds(v1.m_Value / divider);
	}

	public static bool operator ==(Rounds v1, Rounds v2)
	{
		return v1.Equals(v2);
	}

	public static bool operator !=(Rounds v1, Rounds v2)
	{
		return !(v1 == v2);
	}

	public static bool operator <=(Rounds v1, Rounds v2)
	{
		return v1.m_Value <= v2.m_Value;
	}

	public static bool operator >=(Rounds v1, Rounds v2)
	{
		return v1.m_Value >= v2.m_Value;
	}

	public static bool operator <(Rounds v1, Rounds v2)
	{
		return v1.m_Value < v2.m_Value;
	}

	public static bool operator >(Rounds v1, Rounds v2)
	{
		return v1.m_Value > v2.m_Value;
	}

	public static Rounds Max(Rounds v1, Rounds v2)
	{
		if (!(v1 > v2))
		{
			return v2;
		}
		return v1;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Value);
		return result;
	}
}
