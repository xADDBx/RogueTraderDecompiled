using System;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIAbilityTexts
{
	public LocalizedString SingleShot;

	public LocalizedString Burst;

	public LocalizedString Pattern;

	public LocalizedString Melee;

	public string GetAttackType(AbilityData abilityData)
	{
		if (abilityData.IsMelee)
		{
			return Melee;
		}
		return abilityData.Blueprint.AttackType switch
		{
			AttackAbilityType.SingleShot => SingleShot, 
			AttackAbilityType.Scatter => Burst, 
			AttackAbilityType.Pattern => Pattern, 
			_ => string.Empty, 
		};
	}
}
