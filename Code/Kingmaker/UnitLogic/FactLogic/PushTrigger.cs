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
[TypeId("4a7549cdf2c944898ee3e790b36e47b1")]
public class PushTrigger : UnitBuffComponentDelegate, IUnitGetAbilityPush, ISubscriber, IHashable
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		if (OnlyFromOwner && caster != base.Owner)
		{
			return;
		}
		base.Context[ContextPropertyName] = distanceInCells * ValueMultiplier.Calculate(base.Context) + ValueBonus.Calculate(base.Context);
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(Actions, target.ToITargetWrapper());
		}
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
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
