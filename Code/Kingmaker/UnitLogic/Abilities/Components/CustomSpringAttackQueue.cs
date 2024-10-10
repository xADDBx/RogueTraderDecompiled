using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a03ec6f0e58b4a2e8339d3e2471854bb")]
public class CustomSpringAttackQueue : AbilityCustomLogic
{
	public BlueprintAbilityReference DeathWaltz;

	public BlueprintAbilityReference MoveAbility;

	public BlueprintBuffReference TemporaryBuff;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity caster = context.Caster;
		UnitPartSpringAttack sprintAttackPart = caster.GetOptional<UnitPartSpringAttack>();
		PartUnitCommands commands = caster.GetCommandsOptional();
		if (sprintAttackPart == null || commands == null)
		{
			yield break;
		}
		Vector3 turnStartPosition = sprintAttackPart.TurnStartPosition;
		Buff temporaryBuff = caster.Buffs.Add(TemporaryBuff, context);
		List<SpringAttackEntry> springAttackEntries = sprintAttackPart.Entries;
		int index = springAttackEntries.Count;
		float errorTimer = 0f;
		for (; index > 0; index--)
		{
			SpringAttackEntry entry = Enumerable.FirstOrDefault(springAttackEntries, (SpringAttackEntry p) => p.Index == index);
			if (entry == null || !(entry.NewPosition != entry.OldPosition))
			{
				continue;
			}
			if (caster.Position != entry.NewPosition)
			{
				BaseUnitEntity unit = entry.NewPosition.GetNearestNodeXZUnwalkable().GetUnit();
				if (!(unit is UnitEntity) || !unit.IsDeadOrUnconscious)
				{
					UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(CreateAbility(MoveAbility, context), entry.NewPosition)
					{
						FreeAction = true
					};
					commands.AddToQueue(cmdParams);
					while ((caster.Position - entry.NewPosition).magnitude > 0.003f && errorTimer < 5f)
					{
						errorTimer += Game.Instance.TimeController.DeltaTime;
						yield return null;
					}
					errorTimer = 0f;
				}
			}
			UnitUseAbilityParams cmdParams2 = new UnitUseAbilityParams(CreateAbility(DeathWaltz, context), entry.OldPosition)
			{
				FreeAction = true
			};
			commands.AddToQueue(cmdParams2);
			while ((caster.Position - entry.OldPosition).magnitude > 0.003f && errorTimer < 5f)
			{
				errorTimer += Game.Instance.TimeController.DeltaTime;
				yield return null;
			}
			errorTimer = 0f;
			if (entry.AreaMark.Entity != null)
			{
				entry.AreaMark.Entity.ForceEnded = true;
			}
			springAttackEntries.Remove(entry);
		}
		sprintAttackPart.RemoveEntries();
		AbilityData ability = CreateAbility(MoveAbility, context);
		if (turnStartPosition.GetNearestNodeXZUnwalkable().GetUnit() is UnitEntity unitEntity && unitEntity != caster && !unitEntity.IsDeadOrUnconscious)
		{
			List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
			for (int i = 0; i < 8; i++)
			{
				CustomGridNodeBase customGridNodeBase = turnStartPosition.GetNearestNodeXZUnwalkable()?.GetNeighbourAlongDirection(i);
				if (customGridNodeBase != null && (customGridNodeBase.GetUnit()?.IsDeadOrUnconscious ?? true))
				{
					list.Add(customGridNodeBase);
				}
			}
			turnStartPosition = ((!list.Any()) ? caster.Position : list.Random(PFStatefulRandom.Mechanics).Vector3Position);
		}
		UnitUseAbilityParams cmdParams3 = new UnitUseAbilityParams(ability, turnStartPosition)
		{
			FreeAction = true
		};
		UnitCommandHandle lastMoveHandle = commands.AddToQueue(cmdParams3);
		while (!lastMoveHandle.IsFinished)
		{
			yield return null;
		}
		temporaryBuff?.Remove();
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private AbilityData CreateAbility(BlueprintAbilityReference ability, AbilityExecutionContext context)
	{
		return new AbilityData(ability, context.Caster);
	}
}
