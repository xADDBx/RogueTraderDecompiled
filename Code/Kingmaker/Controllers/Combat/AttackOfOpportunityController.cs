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
			MakeAttackOfOpportunity(item.Attacker, unitUseAbility.Executor, item.Reason);
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
				bool made = MakeAttackOfOpportunity(value.Attacker, baseUnitEntity, null);
				UpdateAttackOfOpportunityMadeThisTurnCount(value.Attacker, made);
				continue;
			}
			break;
		}
	}

	private static bool MakeAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target, BlueprintFact reason)
	{
		UnitAttackOfOpportunityParams unitAttackOfOpportunityParams = new UnitAttackOfOpportunityParams(target, reason);
		CustomGridNodeBase customGridNodeBase = FindSuitablePositionForAttackOfOpportunity(attacker, target);
		if (customGridNodeBase == null)
		{
			return false;
		}
		if (customGridNodeBase == attacker.CurrentUnwalkableNode)
		{
			attacker.Commands.Run(unitAttackOfOpportunityParams);
			return true;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(attacker.MovementAgent, customGridNodeBase.Vector3Position);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, attacker))
		{
			UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f, null)
			{
				SlowMotionRequired = true
			};
			attacker.Commands.Run(cmdParams);
			attacker.Commands.AddToQueueFirst(unitAttackOfOpportunityParams);
			return true;
		}
	}

	public bool Provoke(BaseUnitEntity target, BaseUnitEntity attacker, BlueprintFact reason)
	{
		if (!attacker.CanMakeAttackOfOpportunity(target))
		{
			return false;
		}
		bool flag = MakeAttackOfOpportunity(attacker, target, reason);
		UpdateAttackOfOpportunityMadeThisTurnCount(attacker, flag);
		return flag;
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

	public bool Provoke(BaseUnitEntity target, BaseUnitEntity attacker, EntityFact reason)
	{
		return Provoke(target, attacker, reason.Blueprint);
	}

	public void Provoke(BaseUnitEntity target, BlueprintFact reason)
	{
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			Provoke(target, engagedByUnit, reason);
		}
	}

	public void Provoke(BaseUnitEntity target, EntityFact reason)
	{
		Provoke(target, reason.Blueprint);
	}

	[CanBeNull]
	private static CustomGridNodeBase FindSuitablePositionForAttackOfOpportunity(BaseUnitEntity attacker, BaseUnitEntity target)
	{
		CustomGridNode customGridNode = attacker?.CurrentUnwalkableNode;
		if (customGridNode == null)
		{
			return null;
		}
		int attackRange = attacker.GetThreatHand().Weapon.AttackRange;
		int num = attacker.DistanceToInCells(target);
		if (attackRange >= num)
		{
			return customGridNode;
		}
		int width = target.SizeRect.Width;
		int height = target.SizeRect.Height;
		int num2 = num + Math.Max(width, height) + Math.Min(width, height) / 2 + 1;
		List<(GraphNode Node, int Distance)> list = PathfindingService.Instance.FindAllReachableTiles_Blocking(attacker.MovementAgent, customGridNode.Vector3Position, num2).Keys.Select((GraphNode i) => (Node: i, Distance: target.DistanceToInCells(i.Vector3Position))).ToTempList();
		list.Sort(((GraphNode Node, int Distance) n1, (GraphNode Node, int Distance) n2) => n1.Distance.CompareTo(n2.Distance));
		foreach (var item in list)
		{
			if (attacker.IsThreat(target.CurrentUnwalkableNode, item.Node.Vector3Position, target.SizeRect) && attacker.CanStandHere(item.Node))
			{
				return (CustomGridNodeBase)item.Node;
			}
		}
		return null;
	}
}
