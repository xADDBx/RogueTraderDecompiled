using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("4a9042200dad41bb95d4638b1b70329a")]
public class CastAbilityOnMultipleTargetsConsequently : AbilityCustomLogic, IAbilityAoEPatternProviderHolder
{
	private enum Area
	{
		Unrestricted,
		Pattern
	}

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	private Area m_Area;

	public ContextValue Count = new ContextValue();

	[SerializeField]
	[ShowIf("IsPattern")]
	private AbilityAoEPatternSettings m_PatternSettings;

	public RestrictionCalculator TargetRestriction = new RestrictionCalculator();

	public BlueprintAbility Ability => m_Ability;

	public IAbilityAoEPatternProvider PatternProvider => m_PatternSettings;

	private bool IsPattern => m_Area == Area.Pattern;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper mainTarget)
	{
		PartUnitCommands commands = context.Caster.GetCommandsOptional();
		if (commands == null)
		{
			yield break;
		}
		yield return new AbilityDeliveryTarget(mainTarget);
		AbilityData ability = CreateAbility(context);
		MechanicEntity[] targets = (from i in EnumerateTargets(context, mainTarget)
			where ability.CanTarget(i)
			select i).ToArray();
		int count = (Count.IsZero ? targets.Length : Count.Calculate(context));
		while (count > 0 && targets.Length != 0)
		{
			MechanicEntity[] array = targets;
			foreach (MechanicEntity mechanicEntity in array)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability, mechanicEntity)
				{
					FreeAction = true
				};
				UnitCommandHandle cmdHandle = commands.AddToQueue(cmdParams);
				while (!cmdHandle.IsFinished)
				{
					yield return null;
				}
				int num = count - 1;
				count = num;
				if (num < 1)
				{
					break;
				}
			}
		}
	}

	private AbilityData CreateAbility(AbilityExecutionContext context)
	{
		return new AbilityData(Ability, context.Caster)
		{
			OverrideWeapon = context.Ability.Weapon
		};
	}

	private IEnumerable<MechanicEntity> EnumerateTargets(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!IsPattern)
		{
			return Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity i) => TargetRestriction.IsPassed(context, i));
		}
		return from i in m_PatternSettings.GetOrientedPattern(context.Ability, context.Caster.CurrentUnwalkableNode, target.NearestNode, coveredTargetsOnly: true).Nodes.Select((CustomGridNodeBase i) => i.GetUnit()).NotNull()
			where TargetRestriction.IsPassed(context, i) && !i.IsDead
			select i;
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}
