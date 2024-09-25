using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("c025c21f456147f79c8b48b24c5e1da3")]
public class AbilityCustomBattleDance : AbilityCustomLogic, IAbilityCustomAnimation
{
	public override bool IsEngageUnit => true;

	public override bool IsMoveUnit => true;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity target = targetWrapper.Entity;
		if (target == null)
		{
			PFLog.Default.Error("Target unit is missing");
			yield break;
		}
		if (!(context.Caster is UnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		if (caster.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			yield break;
		}
		if (caster.View.AnimationManager?.CurrentAction is UnitAnimationActionHandle unitAnimationActionHandle)
		{
			unitAnimationActionHandle.DoesNotPreventMovement = true;
		}
		else
		{
			PFLog.Default.Error("No animation handle found");
		}
		caster.View.StopMoving();
		caster.View.MovementAgent.IsCharging = true;
		List<(GraphNode node, int direction)> possibleNodsWithDirection = new List<(GraphNode, int)>();
		foreach (CustomGridNodeBase item in target.GetOccupiedNodes().ToList())
		{
			for (int i = 0; i < 4; i++)
			{
				CustomGridNodeBase neighbourAlongDirection = item.GetNeighbourAlongDirection(i);
				if (neighbourAlongDirection != null && neighbourAlongDirection != null && neighbourAlongDirection.Walkable && !neighbourAlongDirection.ContainsUnit())
				{
					possibleNodsWithDirection.Add((neighbourAlongDirection, i));
				}
			}
		}
		while (possibleNodsWithDirection.Count > 0 && target.IsConscious && caster.IsConscious)
		{
			WarhammerPathPlayer shortestPath = null;
			int bestDirection = -1;
			int shortestPathLen = int.MaxValue;
			foreach (var item2 in possibleNodsWithDirection)
			{
				WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(caster.MovementAgent, item2.node.Vector3Position);
				warhammerPathPlayer.Claim(this);
				int num = warhammerPathPlayer.LengthInCells();
				if (num < shortestPathLen)
				{
					shortestPathLen = num;
					shortestPath?.Release(this);
					shortestPath = warhammerPathPlayer;
					bestDirection = item2.direction;
				}
				yield return null;
			}
			UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(shortestPath), 0f);
			shortestPath?.Release(this);
			UnitCommandHandle movement = caster.Commands.Run(cmdParams);
			while (!movement.IsFinished)
			{
				yield return null;
			}
			while (caster.View.MovementAgent.IsReallyMoving)
			{
				yield return null;
			}
			AbilityDeliverAttackWithWeapon abilityDeliverAttackWithWeapon = new AbilityDeliverAttackWithWeapon();
			IEnumerator<AbilityDeliveryTarget> attackEnum = abilityDeliverAttackWithWeapon.Deliver(context, target);
			while (attackEnum.MoveNext())
			{
				yield return attackEnum.Current;
			}
			possibleNodsWithDirection.RemoveAll(((GraphNode node, int direction) e) => e.direction == bestDirection);
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		if (context.Caster is UnitEntity unitEntity)
		{
			unitEntity.View.MovementAgent.IsCharging = false;
			unitEntity.View.MovementAgent.MaxSpeedOverride = null;
			unitEntity.State.IsCharging = false;
		}
	}

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		return null;
	}
}
