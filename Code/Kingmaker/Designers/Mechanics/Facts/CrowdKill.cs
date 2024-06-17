using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("3e01ea9c41024070ad41cd6c9f299e0b")]
public class CrowdKill : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	private enum FactionType
	{
		Any,
		Ally,
		Enemy
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	private ContextValue m_Value;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			return;
		}
		AbilityData ability = evt.Ability;
		if (ability == null)
		{
			return;
		}
		int num = 0;
		foreach (CustomGridNodeBase node in ability.GetPattern(evt.Reason.Context?.MainTarget ?? ((TargetWrapper)(evt.MaybeTarget ?? ability.Caster)), ability.Caster.Position).Nodes)
		{
			BaseUnitEntity unit = node.GetUnit();
			if (unit != null && unit != ability.Caster && (m_Faction != FactionType.Ally || unit.IsAlly(ability.Caster)) && (m_Faction != FactionType.Enemy || !unit.IsAlly(ability.Caster)))
			{
				num++;
			}
		}
		evt.MinValueModifiers.Add(m_Value.Calculate(base.Context) * num, base.Fact);
		evt.MaxValueModifiers.Add(m_Value.Calculate(base.Context) * num, base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
