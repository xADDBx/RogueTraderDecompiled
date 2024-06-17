using System;
using System.Text;
using UnityEngine;

namespace Kingmaker.AI.AreaScanning.Scoring;

public struct Score : IComparable<Score>
{
	public float[] values;

	public static readonly Score zero = new Score
	{
		values = new float[0]
	};

	public bool IsZero
	{
		get
		{
			if (values != null)
			{
				return values.Length == 0;
			}
			return true;
		}
	}

	public int Length
	{
		get
		{
			if (!IsZero)
			{
				return values.Length;
			}
			return 0;
		}
	}

	public Score(params float[] list)
	{
		values = new float[list.Length];
		list.CopyTo(values, 0);
	}

	public Score(Score sc, params float[] list)
	{
		values = new float[list.Length + sc.values.Length];
		sc.values.CopyTo(values, 0);
		list.CopyTo(values, sc.values.Length);
	}

	public Score(params Score[] list)
	{
		int num = 0;
		for (int i = 0; i < list.Length; i++)
		{
			Score score = list[i];
			num += score.values.Length;
		}
		values = new float[num];
		int num2 = 0;
		for (int j = 0; j < list.Length; j++)
		{
			list[j].values.CopyTo(values, num2);
			num2 += list[j].values.Length;
		}
	}

	public int CompareTo(Score other)
	{
		if (IsZero && other.IsZero)
		{
			return 0;
		}
		if (IsZero)
		{
			return -1;
		}
		if (other.IsZero)
		{
			return 1;
		}
		int num = Math.Min(values.Length, other.values.Length);
		for (int i = 0; i < num; i++)
		{
			if (Mathf.Abs(values[i] - other.values[i]) > float.Epsilon)
			{
				return values[i].CompareTo(other.values[i]);
			}
		}
		return values.Length.CompareTo(other.values.Length);
	}

	public override bool Equals(object obj)
	{
		if (obj is Score other)
		{
			return CompareTo(other) == 0;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(values, values.Length);
	}

	public static bool operator >(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) > 0;
	}

	public static bool operator <(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) < 0;
	}

	public static bool operator >=(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) >= 0;
	}

	public static bool operator <=(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) <= 0;
	}

	public static bool operator ==(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) == 0;
	}

	public static bool operator !=(Score lhs, Score rhs)
	{
		return lhs.CompareTo(rhs) != 0;
	}

	public static Score operator +(Score lhs, Score rhs)
	{
		if (lhs.IsZero && rhs.IsZero)
		{
			return default(Score);
		}
		Score score = ((lhs.Length < rhs.Length) ? lhs : rhs);
		Score sc = ((lhs.Length < rhs.Length) ? rhs : lhs);
		Score result = new Score(sc);
		for (int i = 0; i < score.Length; i++)
		{
			result.values[i] += score.values[i];
		}
		return result;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Score[");
		if (values != null)
		{
			for (int i = 0; i < values.Length; i++)
			{
				stringBuilder.Append(string.Format("{0}{1}", values[i], (i < values.Length - 1) ? ", " : ""));
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
