using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("b4a24c9904e04c63bab0b702ab76099e")]
public class RecastAbilityOnNearestTarget : MechanicEntityFactComponentDelegate, IAbilityExecutionProcessHandler<EntitySubscriber>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, EntitySubscriber>, IHashable
{
	public RestrictionCalculator Restriction;

	[SerializeField]
	private TargetType m_TargetType;

	public RestrictionCalculator TargetRestriction;

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (Restriction.IsPassed(base.Fact, context, null, context.Ability))
		{
			MechanicEntity mechanicEntity = SelectTarget(context);
			if (mechanicEntity == null)
			{
				PFLog.EntityFact.ErrorWithReport("Can't find suitable target");
				return;
			}
			AbilityExecutionContext context2 = context.Ability.CreateExecutionContext(mechanicEntity);
			Game.Instance.AbilityExecutor.Execute(context2);
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
	}

	[CanBeNull]
	private MechanicEntity SelectTarget(AbilityExecutionContext context)
	{
		return (from i in Game.Instance.State.AllBaseAwakeUnits.InCombat()
			where i != context.MainTarget.Entity
			where (m_TargetType == TargetType.Ally && base.Owner.IsAlly(i)) || (m_TargetType == TargetType.Enemy && base.Owner.IsEnemy(i)) || m_TargetType == TargetType.Any
			where context.Ability.IsValid(i)
			where TargetRestriction.IsPassed(context, i, base.Fact)
			select i).MinBy((BaseUnitEntity i) => (i.Position - context.MainTarget.Point).sqrMagnitude);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
