using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Arbitrator;

[TypeId("4d96565d80e44e17979881671676f9eb")]
public class LaunchActionOnClosestTargetHit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, IHashable
{
	public enum TargetTypes
	{
		All,
		Enemy,
		Ally
	}

	[SerializeField]
	private TargetTypes m_TargetType = TargetTypes.Enemy;

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_Actions;

	private bool m_Started;

	private List<EntityRef<BaseUnitEntity>> m_Targets = new List<EntityRef<BaseUnitEntity>>();

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!m_Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		BaseUnitEntity targetUnit = evt.TargetUnit;
		if (targetUnit != null && evt.ResultIsHit && (m_TargetType != TargetTypes.Enemy || base.Owner.IsEnemy(targetUnit)) && (m_TargetType != TargetTypes.Ally || base.Owner.IsAlly(targetUnit)))
		{
			m_Targets.Add(new EntityRef<BaseUnitEntity>(targetUnit));
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (!m_Started && context.MaybeCaster == base.Owner)
		{
			m_Started = true;
			m_Targets.Clear();
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (!m_Started || context.MaybeCaster != base.Owner)
		{
			return;
		}
		m_Started = false;
		MechanicEntity mechanicEntity = null;
		float num = float.MaxValue;
		foreach (EntityRef<BaseUnitEntity> target in m_Targets)
		{
			BaseUnitEntity baseUnitEntity = ((target == null || target.IsNull) ? null : target.Entity);
			if (baseUnitEntity != null)
			{
				float sqrMagnitude = (baseUnitEntity.Position - base.Owner.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					mechanicEntity = baseUnitEntity;
					num = sqrMagnitude;
				}
			}
		}
		if (mechanicEntity != null)
		{
			base.Fact.RunActionInContext(m_Actions, mechanicEntity.ToITargetWrapper());
		}
		m_Targets.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
