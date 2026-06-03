using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public sealed class PartUnitInvisibleInCombat : UnitPart, IHashable
{
	public int RevealRadius = 1;

	public RevealReason RevealReason;

	public Buff SourceBuff;

	[JsonProperty]
	private List<InvisibleRevealData> m_RevealDataList = new List<InvisibleRevealData>();

	[JsonProperty]
	public bool IsGhosted { get; set; }

	public IReadOnlyList<InvisibleRevealData> RevealDataList => m_RevealDataList;

	public void AddRevealSource(IEntity entity, bool interruptMovement)
	{
		m_RevealDataList.Add(new InvisibleRevealData(entity, interruptMovement));
	}

	public void ClearRevealSources()
	{
		m_RevealDataList.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<InvisibleRevealData> revealDataList = m_RevealDataList;
		if (revealDataList != null)
		{
			for (int i = 0; i < revealDataList.Count; i++)
			{
				InvisibleRevealData obj = revealDataList[i];
				Hash128 val2 = StructHasher<InvisibleRevealData>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		bool val3 = IsGhosted;
		result.Append(ref val3);
		return result;
	}
}
