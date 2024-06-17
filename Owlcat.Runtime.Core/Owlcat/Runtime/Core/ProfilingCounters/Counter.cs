using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.Core.ProfilingCounters;

public class Counter
{
	private class MeasurementScope : IDisposable
	{
		private Counter m_Counter;

		private Stopwatch m_Stopwatch;

		public bool Started => m_Stopwatch.IsRunning;

		public MeasurementScope(Counter c)
		{
			m_Counter = c;
			m_Stopwatch = new Stopwatch();
		}

		public void Start()
		{
			m_Stopwatch.Restart();
		}

		public void Dispose()
		{
			m_Stopwatch.Stop();
			m_Counter.AddMeasurement((double)m_Stopwatch.ElapsedTicks * MsPerTick);
		}
	}

	private const int BufferFrames = 30;

	private readonly double[] m_Measurements = new double[30];

	private readonly double[] m_MeasurementsSorted = new double[30];

	private int m_MeasuredCount;

	private int m_CurrentMeasurement = -1;

	private int m_LastMeasurementTimeFrame;

	private MeasurementScope m_Scope;

	public readonly string Name;

	public readonly double WarningLevel;

	private static readonly double MsPerTick = 1000f / (float)Stopwatch.Frequency;

	public Counter(string name, double warningLevel)
	{
		Name = name;
		WarningLevel = warningLevel;
		m_Scope = new MeasurementScope(this);
	}

	public void AddMeasurement(double dt)
	{
		int frameCount = Time.frameCount;
		if (frameCount > m_LastMeasurementTimeFrame || m_CurrentMeasurement == -1)
		{
			m_CurrentMeasurement = (m_CurrentMeasurement + 1) % 30;
			if (m_MeasuredCount < 30)
			{
				m_MeasuredCount++;
			}
			m_Measurements[m_CurrentMeasurement] = 0.0;
			m_LastMeasurementTimeFrame = frameCount;
		}
		m_Measurements[m_CurrentMeasurement] += dt;
	}

	public double GetMedian()
	{
		if (m_MeasuredCount == 0)
		{
			return 0.0;
		}
		Array.Copy(m_Measurements, m_MeasurementsSorted, m_MeasuredCount);
		Array.Sort(m_MeasurementsSorted, 0, m_MeasuredCount);
		return m_MeasurementsSorted[m_MeasuredCount / 2];
	}

	public double GetMax()
	{
		if (m_MeasuredCount == 0)
		{
			return 0.0;
		}
		return m_Measurements.Take(m_MeasuredCount).Max();
	}

	public IDisposable Measure()
	{
		if (m_Scope.Started)
		{
			return null;
		}
		m_Scope.Start();
		return m_Scope;
	}
}
