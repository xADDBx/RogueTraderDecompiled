using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[TypeId("dffda8080d276d94581a59e62ef57a90")]
public class AbilityCustomStarshipFocusedEfforts : AbilityCustomLogic, IAbilityCasterRestriction
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private ActionList Actions;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.Caster is StarshipEntity starshipEntity))
		{
			yield break;
		}
		PartAbilityCooldowns abilityCooldownsOptional = starshipEntity.GetAbilityCooldownsOptional();
		if (abilityCooldownsOptional == null)
		{
			yield break;
		}
		if (abilityCooldownsOptional.GroupIsOnCooldown(AffectedGroup))
		{
			starshipEntity.GetAbilityCooldownsOptional()?.RemoveGroupCooldown(AffectedGroup);
		}
		foreach (Ability affectedAbility in GetAffectedAbilities(starshipEntity))
		{
			int? autonomousCooldown = abilityCooldownsOptional.GetAutonomousCooldown(affectedAbility.Blueprint);
			if (autonomousCooldown.HasValue)
			{
				int valueOrDefault = autonomousCooldown.GetValueOrDefault();
				abilityCooldownsOptional.RemoveAbilityCooldown(affectedAbility.Blueprint);
				abilityCooldownsOptional.StartAutonomousCooldown(affectedAbility.Blueprint, valueOrDefault - 1);
			}
		}
		using (context.GetDataScope(target))
		{
			Actions.Run();
		}
		yield return new AbilityDeliveryTarget(starshipEntity);
	}

	private List<Ability> GetAffectedAbilities(StarshipEntity starship)
	{
		return starship.Abilities.RawFacts.FindAll((Ability ab) => ab.Blueprint.AbilityGroups.Contains(AffectedGroup));
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (caster is StarshipEntity starshipEntity)
		{
			PartAbilityCooldowns cooldowns = starshipEntity.GetAbilityCooldownsOptional();
			if (cooldowns != null)
			{
				if (cooldowns.GroupIsOnCooldown(AffectedGroup))
				{
					return true;
				}
				return GetAffectedAbilities(starshipEntity).Any((Ability ab) => cooldowns.GetAutonomousCooldown(ab.Blueprint).HasValue);
			}
		}
		return false;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NoRequiredCondition;
	}
}
