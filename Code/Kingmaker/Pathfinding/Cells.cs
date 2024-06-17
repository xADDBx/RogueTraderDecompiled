using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[Serializable]
public struct Cells
{
	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	private int m_Value;

	public static float ToMetersRatio => GraphParamsMechanicsCache.GridCellSize;

	public float Meters => (float)m_Value * ToMetersRatio;

	public int Value => m_Value;

	public Cells(int value)
	{
		m_Value = value;
	}

	public Cells(float value)
	{
		m_Value = Mathf.RoundToInt(value);
	}

	public override string ToString()
	{
		return $"{Value}c";
	}

	public bool Equals(Cells other)
	{
		return m_Value.Equals(other.m_Value);
	}

	public override bool Equals(object obj)
	{
		if (obj is Cells other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Value.GetHashCode();
	}

	public static Cells operator +(Cells f1, Cells f2)
	{
		return new Cells(f1.m_Value + f2.m_Value);
	}

	public static Cells operator -(Cells f1, Cells f2)
	{
		return new Cells(f1.m_Value - f2.m_Value);
	}

	public static Cells operator *(Cells f1, float multiplier)
	{
		return new Cells((float)f1.m_Value * multiplier);
	}

	public static Cells operator /(Cells f1, float divider)
	{
		return new Cells((float)f1.m_Value / divider);
	}

	public static Cells operator *(Cells f1, int multiplier)
	{
		return new Cells(f1.m_Value * multiplier);
	}

	public static Cells operator /(Cells f1, int divider)
	{
		return new Cells(f1.m_Value / divider);
	}

	public static bool operator ==(Cells f1, Cells f2)
	{
		return f1.Equals(f2);
	}

	public static bool operator !=(Cells f1, Cells f2)
	{
		return !(f1 == f2);
	}

	public static bool operator <=(Cells f1, Cells f2)
	{
		return f1.m_Value <= f2.m_Value;
	}

	public static bool operator >=(Cells f1, Cells f2)
	{
		return f1.m_Value >= f2.m_Value;
	}

	public static bool operator <(Cells f1, Cells f2)
	{
		return f1.m_Value < f2.m_Value;
	}

	public static bool operator >(Cells f1, Cells f2)
	{
		return f1.m_Value > f2.m_Value;
	}
}
