using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("26e7b647b85b4b8ebeaeff8e9b6395b9")]
public class AbilityDeliverTrigger : UnitFactComponentDelegate, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public enum FactionType
	{
		Any,
		Ally,
		Enemy
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_ActionsToCaster;

	[SerializeField]
	private ActionList m_ActionsToTargets;

	[SerializeField]
	private FactionType m_Faction;

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		if (base.Context.MainTarget.Entity is UnitEntity item)
		{
			list.Add(item);
		}
		if (context.MainTarget.Entity is UnitEntity item2)
		{
			list2.Add(item2);
		}
		foreach (CustomGridNodeBase node in context.Pattern.Nodes)
		{
			BaseUnitEntity unit = node.GetUnit();
			if (unit != null && unit != context.Caster && (m_Faction != FactionType.Ally || unit.IsAlly(context.Caster)) && (m_Faction != FactionType.Enemy || !unit.IsAlly(context.Caster)))
			{
				list.Add(unit);
				list2.Add(unit);
			}
		}
		foreach (BaseUnitEntity item3 in list2)
		{
			if (!Restrictions.IsPassed(base.Fact, context, null, context.Ability, item3))
			{
				continue;
			}
			using (ContextData<TargetsInPatternData>.Request().Setup(list.Count))
			{
				foreach (BaseUnitEntity item4 in list)
				{
					using (ContextData<MechanicsContext.Data>.Request().Setup(base.Fact.MaybeContext, item4))
					{
						m_ActionsToTargets.Run();
					}
				}
			}
			using (ContextData<TargetsInPatternData>.Request().Setup(list.Count))
			{
				using (ContextData<MechanicsContext.Data>.Request().Setup(base.Fact.MaybeContext, context.Caster))
				{
					m_ActionsToCaster.Run();
				}
			}
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
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
