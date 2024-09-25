using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AI.Learning;

[Serializable]
public class UnitDataStorage : IHashable
{
	[JsonProperty]
	private List<CollectedUnitData> m_Datas = new List<CollectedUnitData>();

	public CollectedUnitData this[BaseUnitEntity unit]
	{
		get
		{
			CollectedUnitData collectedUnitData = m_Datas.FirstOrDefault((CollectedUnitData d) => d.Unit == unit);
			if (collectedUnitData == null)
			{
				collectedUnitData = new CollectedUnitData(unit);
				m_Datas.Add(collectedUnitData);
			}
			return collectedUnitData;
		}
	}

	public void Clear()
	{
		foreach (CollectedUnitData data in m_Datas)
		{
			data.Clear();
		}
		m_Datas.Clear();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<CollectedUnitData> datas = m_Datas;
		if (datas != null)
		{
			for (int i = 0; i < datas.Count; i++)
			{
				Hash128 val = ClassHasher<CollectedUnitData>.GetHash128(datas[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
