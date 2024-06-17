using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("547decea8a90414eb6dedb6ac9053ec9")]
public class ReplaceBaseStatValue : ReplaceStat, IHashable
{
	private StatType m_OriginalBaseStat;

	protected override void OnActivateOrPostLoad()
	{
		if (m_OriginalStat.IsAttribute())
		{
			return;
		}
		PartStatsContainer required = base.Owner.GetRequired<PartStatsContainer>();
		m_OriginalBaseStat = required.Container.GetBaseStatType(m_OriginalStat);
		StatType statType = ReplaceStat.GetStat(m_NewAttribute);
		if (m_OnlyIfHigher)
		{
			statType = (((int)base.Owner.GetStatOptional(base.PreviousAttributeToCompare) > (int)base.Owner.GetStatOptional(statType)) ? base.PreviousAttributeToCompare : statType);
		}
		if (m_OriginalBaseStat.IsAttribute() && (m_OriginalBaseStat != statType || m_OnlyIfHigher))
		{
			if (required.OverridenBaseStat.ContainsKey(m_OriginalStat))
			{
				required.OverridenBaseStat.Remove(m_OriginalStat);
				PFLog.Default.Log($"ReplaceBaseStatValue: overriding base stat of {m_OriginalStat} to {statType}");
			}
			required.OverridenBaseStat[m_OriginalStat] = statType;
			base.Owner.GetStatOptional(m_OriginalStat)?.UpdateValue();
		}
	}

	protected override void OnDeactivate()
	{
		if (!m_OriginalStat.IsAttribute() && m_OriginalBaseStat.IsAttribute() && m_OriginalBaseStat != ReplaceStat.GetStat(m_NewAttribute))
		{
			base.Owner.GetRequired<PartStatsContainer>().OverridenBaseStat.Remove(m_OriginalStat);
			base.Owner.GetStatOptional(m_OriginalStat)?.UpdateValue();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
