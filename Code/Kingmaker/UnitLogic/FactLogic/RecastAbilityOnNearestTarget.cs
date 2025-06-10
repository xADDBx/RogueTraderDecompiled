using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
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

	[SerializeField]
	private bool UseOnMainTargetLast;

	[SerializeField]
	private bool m_ExecuteOnNearestToCaster;

	public RestrictionCalculator TargetRestriction;

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (UseOnMainTargetLast)
		{
			ExecuteRecast(context);
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (!UseOnMainTargetLast)
		{
			ExecuteRecast(context);
		}
	}

	private void ExecuteRecast(AbilityExecutionContext context)
	{
		if (context.AbilityBlueprint == base.Fact.Blueprint && Restriction.IsPassed(base.Fact, context, null, context.Ability))
		{
			MechanicEntity mechanicEntity = SelectTarget(context);
			if (mechanicEntity == null)
			{
				PFLog.EntityFact.ErrorWithReport("Can't find suitable target");
				return;
			}
			AbilityExecutionContext abilityExecutionContext = context.Ability.CreateExecutionContext(mechanicEntity);
			BlueprintComponentsEnumerator<AbilityApplyEffect> components = abilityExecutionContext.AbilityBlueprint.GetComponents<AbilityApplyEffect>();
			AbilityExecutionProcess.ApplyEffectHit(selectTargets: abilityExecutionContext.AbilityBlueprint.GetComponent<AbilitySelectTarget>(), context: abilityExecutionContext, deliveryTarget: new AbilityDeliveryTarget(mechanicEntity), applyEffects: components, instant: true);
		}
	}

	[CanBeNull]
	private MechanicEntity SelectTarget(AbilityExecutionContext context)
	{
		Vector3 mainPosition = ((!m_ExecuteOnNearestToCaster) ? context.MainTarget.Point : (context.MaybeCaster?.Position ?? context.MainTarget.Point));
		return (from i in Game.Instance.State.AllBaseAwakeUnits.InCombat()
			where i != context.MainTarget.Entity
			where (m_TargetType == TargetType.Ally && base.Owner.IsAlly(i)) || (m_TargetType == TargetType.Enemy && base.Owner.IsEnemy(i)) || m_TargetType == TargetType.Any
			where context.Ability.IsValid(i)
			where TargetRestriction.IsPassed(new MechanicsContext(context.Caster, context.MaybeOwner, context.AbilityBlueprint, context, i), i, base.Fact)
			select i).MinBy((BaseUnitEntity i) => (i.Position - mainPosition).sqrMagnitude);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
