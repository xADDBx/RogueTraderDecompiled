using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class SummonedUnitsController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartSummonedMonster summonedMonsterOption = unit.GetSummonedMonsterOption();
		if (summonedMonsterOption != null && summonedMonsterOption.IsLinkedToSummoner)
		{
			TickUnitFollowMaster(unit, summonedMonsterOption);
			DestroyUnitIfCasterUnconsciousOrMissing(unit, summonedMonsterOption);
		}
	}

	private static void DestroyUnitIfCasterUnconsciousOrMissing(AbstractUnitEntity unit, UnitPartSummonedMonster part)
	{
		if (part.Summoner != null)
		{
			PartLifeState lifeStateOptional = part.Summoner.GetLifeStateOptional();
			if (lifeStateOptional == null || lifeStateOptional.IsConscious)
			{
				return;
			}
		}
		unit.LifeState.MarkedForDeath = true;
	}

	private static void TickUnitFollowMaster(AbstractUnitEntity unit, UnitPartSummonedMonster part)
	{
		if (!(part.Summoner is UnitEntity unitEntity))
		{
			return;
		}
		PartCombatGroup combatGroupOptional = unit.GetCombatGroupOptional();
		if (!unit.IsInCombat)
		{
			PartUnitState stateOptional = unit.GetStateOptional();
			if ((stateOptional == null || stateOptional.CanMove) && part.Summoner != null && (combatGroupOptional == null || unitEntity.CombatGroup.Group == combatGroupOptional.Group))
			{
				ForcedPath path2 = unitEntity.View.MovementAgent.Path;
				Vector3 vector = ((path2 != null && path2.CompleteState == PathCompleteState.Complete) ? path2.vectorPath.LastItem() : part.Summoner.Position);
				if ((vector - (part.MoveTo?.Params?.Target.Point ?? unit.Position)).magnitude > 22f)
				{
					float orientation = unitEntity.Commands.CurrentMoveTo?.Orientation ?? part.Summoner.Orientation;
					Vector3 vector2 = Quaternion.Euler(0f, orientation + (float)unitEntity.Random.Range(-45, 45), 0f) * Vector3.back * (6f + unitEntity.Random.Range(0f, 5f));
					Vector3 position = vector + vector2;
					PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, position, 0.3f, 1, delegate(ForcedPath path)
					{
						if (path.error)
						{
							PFLog.Pathfinding.Error("An error path was returned. Ignoring");
						}
						else
						{
							part.MoveTo?.Interrupt();
							UnitMoveToParams cmdParams = new UnitMoveToParams(path, position)
							{
								Orientation = orientation
							};
							part.MoveTo = unit.Commands.Run(cmdParams);
						}
					});
				}
				UnitCommandHandle moveTo = part.MoveTo;
				if (moveTo != null && moveTo.IsFinished)
				{
					part.MoveTo = null;
				}
				if ((bool)unit.View)
				{
					unit.View.MovementAgent.ForceRoaming = true;
				}
				return;
			}
		}
		unit.View.MovementAgent.ForceRoaming = false;
		part.MoveTo?.Interrupt();
		part.MoveTo = null;
	}
}
