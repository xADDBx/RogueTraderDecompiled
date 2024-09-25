using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3ddcd0f2794fd3a4cb85f4ecd7ebe7ae")]
public class CooldownGetter : PropertyGetter
{
	private enum Type
	{
		Ability,
		AbilityGroup
	}

	[SerializeField]
	private Type m_Type;

	[SerializeField]
	[ShowIf("IsAbilityType")]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[ShowIf("IsAbilityGroupType")]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	private BlueprintAbility Ability => m_Ability?.Get();

	private BlueprintAbilityGroup AbilityGroup => m_AbilityGroup?.Get();

	private bool IsAbilityType => m_Type == Type.Ability;

	private bool IsAbilityGroupType => m_Type == Type.AbilityGroup;

	protected override int GetBaseValue()
	{
		PartAbilityCooldowns abilityCooldownsOptional = base.CurrentEntity.GetAbilityCooldownsOptional();
		if (abilityCooldownsOptional == null)
		{
			return 0;
		}
		if (!IsAbilityType)
		{
			return abilityCooldownsOptional.GroupCooldown(AbilityGroup);
		}
		return abilityCooldownsOptional.GetCooldown(Ability);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!IsAbilityType)
		{
			return $"{AbilityGroup} cooldown";
		}
		return $"{Ability} cooldown";
	}
}
