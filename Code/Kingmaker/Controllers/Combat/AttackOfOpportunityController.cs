using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.Controllers.Combat;

public class AttackOfOpportunityController : IController, IUnitRunCommandHandler, ISubscriber, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandEndHandler, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, IDirectMovementHandler
{
	public void HandleDirectMovementStarted(ForcedPath path, bool disableAttacksOfOpportunity)
	{
		if (!disableAttacksOfOpportunity)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			if (baseUnitEntity != null)
			{
				IEnumerable<AttackOfOpportunityData> attacks = baseUnitEntity.CalculateAttackOfOpportunity(path);
				baseUnitEntity.GetOrCreate<PartIncomingAttacksOfOpportunity>().SetAttacks(attacks);
			}
		}
	}

	public void HandleDirectMovementEnded()
	{
		EventInvokerExtensions.BaseUnitEntity?.Remove<PartIncomingAttacksOfOpportunity>();
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper { DisableAttackOfOpportunity: false } unitMoveToProper)
		{
			IEnumerable<AttackOfOpportunityData> attacks = unitMoveToProper.CalculateAttackOfOpportunity();
			unitMoveToProper.Executor.GetOrCreate<PartIncomingAttacksOfOpportunity>().SetAttacks(attacks);
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility unitUseAbility))
		{
			return;
		}
		foreach (AttackOfOpportunityData item in unitUseAbility.CalculateAttackOfOpportunity())
		{
			MakeAttackOfOpportunity(item.Attacker, unitUseAbility.Executor, item.Reason, canUseInRange: false, canMove: true);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper)
		{
			command.Executor.Remove<PartIncomingAttacksOfOpportunity>();
		}
	}

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (!TurnController.IsInTurnBasedCombat() || !(unit is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		PartIncomingAttacksOfOpportunity optional = unit.GetOptional<PartIncomingAttacksOfOpportunity>();
		if (optional == null)
		{
			return;
		}
		while (optional.NextAttack.HasValue)
		{
			AttackOfOpportunityData value = optional.NextAttack.Value;
			float magnitude = (baseUnitEntity.Position - optional.StartPosition).magnitude;
			float magnitude2 = (value.Position - optional.StartPosition).magnitude;
			if (!(magnitude < magnitude2))
			{
				optional.AcceptNextAttack();
				UnitCommandHandle unitCommandHandle = null;
				UnitPartAttackOfOpportunityModifier optional2 = value.Attacker.Parts.GetOptional<UnitPartAttackOfOpportunityModifier>();
				if (optional2 != null && optional2.EnableAndPrioritizeRangedAttack)
				{
					unitCommandHandle = MakeRangedAttackOfOpportunity(value.Attacker, baseUnitEntity, null);
				}
				if (unitCommandHandle == null)
				{
					unitCommandHandle = MakeAttackOfOpportunity(value.Attacker, baseUnitEntity, null, canUseInRange: false, canMove: false);
				}
				UpdateAttackOfOpportunityMadeThisTurnCount(value.Attacker, unitCommandHandle != null);
				continue;
			}
			break;
		}
	}

	private static UnitCommandHandle MakeRangedAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target, BlueprintFact reason)
	{
		if (attacker.GetThreatHandRangedAnyHand()?.MaybeWeapon != null)
		{
			UnitAttackOfOpportunityParams cmdParams = new UnitAttackOfOpportunityParams(target, reason, isRanged: true);
			return attacker.Commands.Run(cmdParams);
		}
		return null;
	}

	private static UnitCommandHandle MakeAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target, BlueprintFact reason, bool canUseInRange, bool canMove)
	{
		if (attacker.GetThreatHand()?.MaybeWeapon != null)
		{
			CustomGridNodeBase customGridNodeBase = FindSuitablePositionForAttackOfOpportunity(attacker, target);
			if (customGridNodeBase != null)
			{
				if (customGridNodeBase == attacker.CurrentUnwalkableNode)
				{
					UnitAttackOfOpportunityParams cmdParams = new UnitAttackOfOpportunityParams(target, reason, isRanged: false);
					return attacker.Commands.Run(cmdParams);
				}
				if (canMove)
				{
					WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(attacker.MovementAgent, customGridNodeBase.Vector3Position);
					using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, attacker))
					{
						if (warhammerPathPlayer.CompleteState == PathCompleteState.Complete)
						{
							UnitAttackOfOpportunityParams cmd = new UnitAttackOfOpportunityParams(target, reason, isRanged: false);
							UnitMoveToProperParams cmdParams2 = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f, null)
							{
								SlowMotionRequired = true
							};
							attacker.Commands.Run(cmdParams2);
							return attacker.Commands.AddToQueueFirst(cmd);
						}
					}
				}
			}
		}
		if (canUseInRange)
		{
			return MakeRangedAttackOfOpportunity(attacker, target, reason);
		}
		return null;
	}

	public UnitCommandHandle Provoke(BaseUnitEntity target, BaseUnitEntity attacker, BlueprintFact reason, bool canUseInRange, bool canMove)
	{
		if (!attacker.CanMakeAttackOfOpportunity(target))
		{
			return null;
		}
		UnitCommandHandle unitCommandHandle = MakeAttackOfOpportunity(attacker, target, reason, canUseInRange, canMove);
		UpdateAttackOfOpportunityMadeThisTurnCount(attacker, unitCommandHandle != null);
		return unitCommandHandle;
	}

	private static void UpdateAttackOfOpportunityMadeThisTurnCount(BaseUnitEntity attacker, bool made)
	{
		if (made)
		{
			PartUnitCombatState combatStateOptional = attacker.GetCombatStateOptional();
			if (combatStateOptional != null)
			{
				combatStateOptional.AttacksOfOpportunityMadeThisTurnCount++;
			}
		}
	}

	public UnitCommandHandle Provoke(BaseUnitEntity target, BaseUnitEntity attacker, EntityFact reason, bool canUseInRange, bool canMove)
	{
		return Provoke(target, attacker, reason.Blueprint, canUseInRange, canMove);
	}

	public void Provoke(BaseUnitEntity target, BlueprintFact reason, bool canUseInRange, bool canMove)
	{
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			Provoke(target, engagedByUnit, reason, canUseInRange, canMove);
		}
	}

	public void Provoke(BaseUnitEntity target, EntityFact reason)
	{
		Provoke(target, reason.Blueprint, canUseInRange: false, canMove: true);
	}

	[CanBeNull]
	private static CustomGridNodeBase FindSuitablePositionForAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target)
	{
		CustomGridNode customGridNode = attacker?.CurrentUnwalkableNode;
		if (customGridNode == null)
		{
			return null;
		}
		int attackOfOpportunityThreatingRange = attacker.GetThreatHand().GetAttackOfOpportunityThreatingRange(attacker);
		int num = attacker.DistanceToInCells(target);
		if (attackOfOpportunityThreatingRange >= num)
		{
			return customGridNode;
		}
		int width = target.SizeRect.Width;
		int height = target.SizeRect.Height;
		int num2 = num + Math.Max(width, height) + Math.Min(width, height) / 2 + 1;
		foreach (var item in (from i in PathfindingService.Instance.FindAllReachableTiles_Blocking(attacker.MovementAgent, customGridNode.Vector3Position, num2).Values
			select (Node: i.Node, Len: i.Length, Distance: target.DistanceToInCells(i.Node.Vector3Position)) into i
			orderby i.Distance, i.Len
			select i).ToTempList())
		{
			if (attacker.IsThreat(target.CurrentUnwalkableNode, item.Node.Vector3Position, target.SizeRect) && attacker.CanStandHere(item.Node))
			{
				return (CustomGridNodeBase)item.Node;
			}
		}
		return null;
	}
}
