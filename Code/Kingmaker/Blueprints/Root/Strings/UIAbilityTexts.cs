using System;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIAbilityTexts
{
	public LocalizedString SingleShot;

	public LocalizedString Burst;

	public LocalizedString Pattern;

	public LocalizedString Melee;

	public string GetAttackType(AttackAbilityType? type)
	{
		return type switch
		{
			AttackAbilityType.SingleShot => SingleShot, 
			AttackAbilityType.Scatter => Burst, 
			AttackAbilityType.Pattern => Pattern, 
			AttackAbilityType.Melee => Melee, 
			_ => string.Empty, 
		};
	}
}
