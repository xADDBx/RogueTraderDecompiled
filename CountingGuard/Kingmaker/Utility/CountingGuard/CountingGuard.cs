using System;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility.CountingGuard;

public class CountingGuard
{
	private class GuardScope : IDisposable
	{
		private CountingGuard m_Guard;

		public GuardScope(CountingGuard guard)
		{
			m_Guard = guard++;
		}

		public void Dispose()
		{
			--m_Guard;
		}
	}

	private static readonly LogChannel Log = LogChannelFactory.GetOrCreate("CountingGuard");

	private int m_GuardCount;

	private bool m_CanGoNegative;

	public bool Value
	{
		get
		{
			return m_GuardCount > 0;
		}
		set
		{
			if (value)
			{
				m_GuardCount++;
			}
			else if (m_GuardCount <= 0 && !m_CanGoNegative)
			{
				Log.Error("CountingGuard went negative: " + m_GuardCount);
			}
			else
			{
				m_GuardCount--;
			}
		}
	}

	public int GuardCount => m_GuardCount;

	public CountingGuard()
		: this(canGoNegative: false)
	{
	}

	public CountingGuard(bool canGoNegative)
	{
		m_CanGoNegative = canGoNegative;
	}

	public bool SetValue(bool value)
	{
		bool value2 = Value;
		Value = value;
		return Value != value2;
	}

	public static implicit operator bool(CountingGuard guard)
	{
		return guard.Value;
	}

	public static CountingGuard operator ++(CountingGuard guard)
	{
		guard.Value = true;
		return guard;
	}

	public static CountingGuard operator --(CountingGuard guard)
	{
		guard.Value = false;
		return guard;
	}

	public void Reset()
	{
		m_GuardCount = 0;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_GuardCount})";
	}
}
