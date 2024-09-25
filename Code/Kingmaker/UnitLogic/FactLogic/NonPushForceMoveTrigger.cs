using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("20cd904e35bf478fbc8d294361266f39")]
public class NonPushForceMoveTrigger : UnitBuffComponentDelegate, IUnitAbilityNonPushForceMoveHandler, ISubscriber, IHashable
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void HandleUnitNonPushForceMove(int distanceInCells, MechanicsContext context, UnitEntity movedTarget)
	{
		if (OnlyFromOwner && context.MaybeCaster != base.Owner)
		{
			return;
		}
		base.Context[ContextPropertyName] = distanceInCells * ValueMultiplier.Calculate(base.Context) + ValueBonus.Calculate(base.Context);
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(Actions, movedTarget.ToITargetWrapper());
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
