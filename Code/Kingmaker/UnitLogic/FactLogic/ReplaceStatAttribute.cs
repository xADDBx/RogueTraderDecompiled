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
[TypeId("abc6ff48520749c0b4d161484dd6486f")]
public class ReplaceStatAttribute : ReplaceStat, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		StatType statType = ReplaceStat.GetStat(m_NewAttribute);
		if (m_OnlyIfHigher)
		{
			statType = (((int)base.Owner.GetStatOptional(base.PreviousAttributeToCompare) > (int)base.Owner.GetStatOptional(statType)) ? base.PreviousAttributeToCompare : statType);
		}
		if (m_OriginalStat != statType || m_OnlyIfHigher)
		{
			PartStatsContainer required = base.Owner.GetRequired<PartStatsContainer>();
			if (required.OverridenStats.ContainsKey(statType))
			{
				required.OverridenStats.Remove(statType);
				PFLog.Default.Log($"ReplaceStatAttribute: re-overriding stat {m_OriginalStat} to {statType}");
			}
			required.OverridenStats[m_OriginalStat] = statType;
			base.Owner.GetStatOptional(m_OriginalStat)?.UpdateValue();
		}
	}

	protected override void OnDeactivate()
	{
		StatType stat = ReplaceStat.GetStat(m_NewAttribute);
		if (m_OriginalStat != stat)
		{
			base.Owner.GetRequired<PartStatsContainer>().OverridenStats.Remove(m_OriginalStat);
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
