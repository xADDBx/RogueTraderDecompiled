using System;
using Newtonsoft.Json;

namespace Kingmaker.Utility.FlagCountable;

public class ObservableCountFlag
{
	[JsonProperty]
	private CountableFlag m_Flag = new CountableFlag();

	public CountableFlag Flag => m_Flag;

	public int Count => m_Flag.Count;

	public bool Value => m_Flag.Value;

	public event Action OnFlagRetainEvent = delegate
	{
	};

	public event Action OnFlagReleaseEvent = delegate
	{
	};

	public void Retain()
	{
		m_Flag.Retain();
		this.OnFlagRetainEvent();
	}

	public void Release()
	{
		m_Flag.Release();
		this.OnFlagReleaseEvent();
	}

	public static implicit operator bool(ObservableCountFlag flag)
	{
		if (flag != null && flag.m_Flag != null)
		{
			return flag.m_Flag.Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Flag.Count})";
	}
}
