using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class RespecInfo : IHashable
{
	[JsonProperty]
	private int m_RespecCount;

	[JsonProperty]
	private bool m_HasExtraRespec;

	public int GetRespecCost()
	{
		if (m_HasExtraRespec)
		{
			return 0;
		}
		if (m_RespecCount >= 3)
		{
			return m_RespecCount - 2;
		}
		return 0;
	}

	public void CountRespecIn()
	{
		if (m_HasExtraRespec)
		{
			m_HasExtraRespec = false;
		}
		else
		{
			m_RespecCount++;
		}
	}

	public void GiveExtraRespec()
	{
		m_HasExtraRespec = true;
	}

	public RespecInfo()
	{
		m_RespecCount = 0;
		m_HasExtraRespec = false;
	}

	public RespecInfo(int respecsHappened)
	{
		m_RespecCount = respecsHappened;
		m_HasExtraRespec = false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_RespecCount);
		result.Append(ref m_HasExtraRespec);
		return result;
	}
}
