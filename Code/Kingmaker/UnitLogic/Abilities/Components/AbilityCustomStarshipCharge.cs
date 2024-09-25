using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("a5ec8787ce1f4324b91d96f107712b73")]
public class AbilityCustomStarshipCharge : AbilityCustomLogic
{
	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity target = targetWrapper.Entity;
		if (target == null)
		{
			PFLog.Default.Error("Target is missing");
			yield break;
		}
		if (!(context.Caster is StarshipEntity caster))
		{
			PFLog.Default.Error("Caster starship is missing");
			yield break;
		}
		_ = caster.Position;
		Vector3 position = target.Position;
		caster.View.StopMoving();
		ForcedPath forcedPath = caster.Navigation.FindPath(position);
		if (forcedPath.path.Count == 0)
		{
			PFLog.Default.Error("Path to target unit not found");
			yield break;
		}
		forcedPath.path.RemoveLast();
		forcedPath.vectorPath.RemoveLast();
		UnitMoveToProperParams cmd = new UnitMoveToProperParams(forcedPath, caster.Blueprint.WarhammerMovementApPerCell);
		UnitCommandHandle moveCmdHandle = caster.Commands.AddToQueueFirst(cmd);
		while (!moveCmdHandle.IsFinished)
		{
			yield return null;
		}
		caster.View.ViewTransform.LookAt(target.View.ViewTransform);
		yield return new AbilityDeliveryTarget(caster);
		yield return new AbilityDeliveryTarget(target);
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}
