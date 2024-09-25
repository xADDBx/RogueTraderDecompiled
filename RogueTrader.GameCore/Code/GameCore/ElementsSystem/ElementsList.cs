using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Code.GameCore.ElementsSystem;

public abstract class ElementsList : IHashable
{
	private static int s_NextID = 1;

	private int m_ID;

	public abstract IEnumerable<Element> Elements { get; }

	public int GetID()
	{
		if (m_ID != 0)
		{
			return m_ID;
		}
		return m_ID = s_NextID++;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
