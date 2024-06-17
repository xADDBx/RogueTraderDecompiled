using System;
using System.Linq;

namespace Kingmaker.QA.Arbiter;

public class MeasurementCounter
{
	private Func<float> m_MeasurementGetter;

	private const int ArrayLength = 1048576;

	private float[] m_Array;

	private int m_Index;

	private bool m_IsFull;

	public string? Name { get; private set; }

	public MeasurementCounter(Func<float> measurementGetter, string? name = null)
	{
		m_MeasurementGetter = measurementGetter ?? throw new ArgumentNullException("measurementGetter");
		Name = name;
		m_Array = new float[1048576];
		m_Index = 0;
		m_IsFull = false;
	}

	public void Reset()
	{
		m_Index = 0;
		Array.Clear(m_Array, 0, 1048576);
		m_IsFull = false;
	}

	public void Measure()
	{
		float num = m_MeasurementGetter();
		m_Array[m_Index++] = num;
		if (m_Index > 1048576)
		{
			m_Index = 0;
		}
	}

	public float GetPercentile(int percentile)
	{
		float num = (float)percentile / 100f;
		int num2 = 1048576;
		if (m_IsFull)
		{
			Array.Sort(m_Array);
		}
		else
		{
			Array.Sort(m_Array, 0, m_Index);
			num2 = m_Index;
		}
		float num3 = (float)(num2 - 1) * num + 1f;
		if (Math.Abs(num3 - 1f) < float.Epsilon)
		{
			return m_Array[0];
		}
		if (Math.Abs(num3 - (float)num2) < float.Epsilon)
		{
			return m_Array[num2 - 1];
		}
		int num4 = (int)num3;
		float num5 = num3 - (float)num4;
		return m_Array[num4 - 1] + num5 * (m_Array[num4] - m_Array[num4 - 1]);
	}

	public float GetAvgValue(int percentile = 100)
	{
		float percentileValue = GetPercentile(percentile);
		float[] source = (m_IsFull ? m_Array.Where((float v) => v < percentileValue).ToArray() : (from v in m_Array.Take(m_Index)
			where v < percentileValue
			select v).ToArray());
		if (!source.Any())
		{
			return m_Array[0];
		}
		return source.Average();
	}

	public float GetMaxValue(int percentile = 100)
	{
		float percentileValue = GetPercentile(percentile);
		float[] source = (m_IsFull ? m_Array.Where((float v) => v < percentileValue).ToArray() : (from v in m_Array.Take(m_Index)
			where v < percentileValue
			select v).ToArray());
		if (!source.Any())
		{
			return m_Array[0];
		}
		return source.Max();
	}

	public float GetMinExcludedValue(int percentile = 100)
	{
		float percentileValue = GetPercentile(percentile);
		float[] array = (m_IsFull ? m_Array.Where((float v) => v > percentileValue).ToArray() : (from v in m_Array.Take(m_Index)
			where v > percentileValue
			select v).ToArray());
		if (array.Any())
		{
			return array[0];
		}
		return m_Array[m_Index];
	}

	public float GetMinValue(int percentile = 100)
	{
		if (m_Index == 0)
		{
			return 0f;
		}
		if (!m_IsFull)
		{
			return m_Array.Take(m_Index).Min();
		}
		return m_Array.Min();
	}
}
