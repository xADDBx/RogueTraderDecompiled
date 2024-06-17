using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("8dd4b186d1504ef0a4c0165d3ee1c287")]
public class WarhammerConcentratedFireInitiator : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	private int? m_CountTargets;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (!(evt.Ability == null) && m_Restrictions.IsPassed(base.Fact, evt, evt.Ability) && evt.Initiator is BaseUnitEntity)
		{
			int num = m_CountTargets ?? ((ContextData<EnemyTargetsInPatternData>.Current != null) ? ContextData<EnemyTargetsInPatternData>.Current.EnemyTargetsInPattern : 0);
			int num2 = ((num > 0) ? (50 + 10 * GetBallisticSkillBonus(evt.ConcreteInitiator) / num) : 0);
			if (num2 != 0)
			{
				evt.ValueModifiers.Add(ModifierType.PctAdd, num2, base.Fact);
			}
		}
	}

	private static int GetBallisticSkillBonus(Entity entity)
	{
		return (entity.GetOptional<PartStatsContainer>()?.GetAttributeOptional(StatType.WarhammerBallisticSkill)?.Bonus).GetValueOrDefault();
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (!(context.Ability == null) && m_Restrictions.IsPassed(base.Fact, null, context.Ability) && context.Caster is BaseUnitEntity caster)
		{
			OrientedPatternData orientedPatternData = context.Pattern;
			m_CountTargets = GetEnemyTargetCountInPattern(in orientedPatternData, caster);
		}
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (!(context.Ability == null) && m_Restrictions.IsPassed(base.Fact, null, context.Ability) && context.Caster is BaseUnitEntity)
		{
			m_CountTargets = null;
		}
	}

	private static int GetEnemyTargetCountInPattern(in OrientedPatternData orientedPatternData, BaseUnitEntity caster)
	{
		int num = 0;
		foreach (CustomGridNodeBase node in orientedPatternData.Nodes)
		{
			BaseUnitEntity unit = node.GetUnit();
			if (unit != null && unit != caster && !unit.IsAlly(caster))
			{
				num++;
			}
		}
		return num;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
