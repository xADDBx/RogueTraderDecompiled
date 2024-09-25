using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("686b4249a3c54e3b9539d2efeef970d4")]
public class BuffStacksForOverheal : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleHealDamage>, ITargetRulebookSubscriber, IHashable
{
	public class ComponentData : IEntityFactComponentTransientData
	{
		public int OldWounds { get; set; }
	}

	public bool ReplaceLowStacks;

	public bool BuffStacksTemporaryWounds;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public void OnEventAboutToTrigger(RuleHealDamage evt)
	{
		if (evt.TargetHealth != null)
		{
			RequestTransientData<ComponentData>().OldWounds = evt.TargetHealth.HitPointsLeft;
		}
	}

	public void OnEventDidTrigger(RuleHealDamage evt)
	{
		UnitEntity unitEntity = evt.Target as UnitEntity;
		if (evt.TargetHealth == null || unitEntity == null || !unitEntity.IsInCombat)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		int valueWithoutReduction = evt.CalculateHealRule.ValueWithoutReduction;
		int num = valueWithoutReduction - (evt.TargetHealth.MaxHitPoints - componentData.OldWounds);
		if (valueWithoutReduction <= 0 || num <= 0)
		{
			return;
		}
		BuffDuration duration = new BuffDuration(null, BuffEndCondition.CombatEnd);
		Buff buff = unitEntity.Buffs.GetBuff(Buff);
		if (buff == null)
		{
			buff = unitEntity.Buffs.Add(Buff, base.Context, duration);
			if (num > 1)
			{
				buff?.AddRank(num - 1);
			}
		}
		else if (!ReplaceLowStacks)
		{
			buff.AddRank(num);
		}
		else if (buff.Rank <= num || (BuffStacksTemporaryWounds && evt.TargetHealth.GetTemporaryHitPointFromBuff(Buff) <= num))
		{
			buff.Remove();
			buff = unitEntity.Buffs.Add(Buff, base.Context, duration);
			if (num > 1)
			{
				buff?.AddRank(num - 1);
			}
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
