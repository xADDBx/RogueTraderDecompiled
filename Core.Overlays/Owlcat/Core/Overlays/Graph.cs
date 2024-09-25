using System;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;

namespace Owlcat.Core.Overlays;

public class Graph : OverlayElement
{
	public string Caption;

	public Color Color;

	public double? MaxValue;

	public double? MinValue;

	public Func<double> GetValue;

	public Counter Counter;

	public const int MaxSamples = 500;

	private double[] m_Samples = new double[500];

	private int m_SampleCount;

	private int m_LastSample = -1;

	private double m_ObservedMinValue = double.MaxValue;

	private double m_ObservedMaxValue = double.MinValue;

	internal int LegendWidth => Caption.Length * 8;

	public Graph(string name, string caption = null)
		: base(name)
	{
		Caption = caption ?? name;
		Color = Color.green;
	}

	public Graph(Counter c)
		: base(c.Name)
	{
		Caption = base.Name;
		Color = ColorFromName(base.Name);
		Counter = c;
		MinValue = 0.0;
	}

	public static Color ColorFromName(string name)
	{
		int hashCode = name.GetHashCode();
		byte r = (byte)((uint)hashCode & 0xFFu);
		int num = hashCode >> 8;
		byte g = (byte)((uint)num & 0xFFu);
		byte b = (byte)((uint)(num >> 8) & 0xFFu);
		Color result = new Color32(r, g, b, 0);
		float maxColorComponent = result.maxColorComponent;
		result.r /= maxColorComponent;
		result.g /= maxColorComponent;
		result.b /= maxColorComponent;
		result.a = 1f;
		return result;
	}

	internal void Sample()
	{
		double num = GetValue?.Invoke() ?? Counter?.GetMedian() ?? 0.0;
		UpdateMinMax(num);
		m_LastSample = (m_LastSample + 1) % 500;
		m_Samples[m_LastSample] = num;
		if (m_SampleCount < 500)
		{
			m_SampleCount += 500;
		}
	}

	private void UpdateMinMax(double value)
	{
		if (!MinValue.HasValue)
		{
			if (value < m_ObservedMinValue)
			{
				m_ObservedMinValue = value;
			}
			else if (m_SampleCount == 500 && m_Samples[m_LastSample] <= m_ObservedMinValue)
			{
				m_ObservedMinValue = double.MaxValue;
				for (int i = 0; i < 500; i++)
				{
					if (i != m_LastSample && m_ObservedMinValue > m_Samples[i])
					{
						m_ObservedMinValue = m_Samples[i];
					}
				}
			}
		}
		if (MaxValue.HasValue)
		{
			return;
		}
		if (value > m_ObservedMaxValue)
		{
			m_ObservedMaxValue = value;
		}
		else
		{
			if (m_SampleCount != 500 || !(m_Samples[m_LastSample] >= m_ObservedMaxValue))
			{
				return;
			}
			m_ObservedMaxValue = double.MinValue;
			for (int j = 0; j < 500; j++)
			{
				if (j != m_LastSample && m_ObservedMaxValue < m_Samples[j])
				{
					m_ObservedMaxValue = m_Samples[j];
				}
			}
		}
	}

	internal void DrawLegend(float x, float y)
	{
		Rect position = new Rect(x, y, LegendWidth, 20f);
		Color color = GUI.color;
		if (OverlayService.Instance.DarkenBackground)
		{
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(position, Texture2D.whiteTexture);
		}
		GUI.color = Color;
		GUI.Label(position, Caption);
		GUI.color = color;
	}

	internal void DrawGraph(Rect r)
	{
		if (m_SampleCount != 0)
		{
			double minValue = GetMinValue();
			double maxValue = GetMaxValue();
			DrawGraph(r, minValue, maxValue);
		}
	}

	internal void DrawGraph(Rect r, double min, double max)
	{
		if (min >= max)
		{
			max = min + 1.0;
		}
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		GL.Begin(2);
		GL.Color(Color);
		int num = ((m_SampleCount >= 500) ? ((m_LastSample + 1) % 500) : 0);
		for (int i = 0; i < m_SampleCount; i++)
		{
			double num2 = m_Samples[(i + num) % 500];
			num2 = (num2 - min) / (max - min);
			GL.Vertex3((float)i * r.width / 500f + r.xMin, r.yMax - (float)(num2 * (double)r.height), 0f);
		}
		GL.End();
		GL.PopMatrix();
	}

	public double GetMinValue()
	{
		return MinValue ?? m_ObservedMinValue;
	}

	public double GetMaxValue()
	{
		return MaxValue ?? m_ObservedMaxValue;
	}
}
