using System;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class BonusSourceStrings : StringsContainer
{
	public LocalizedString ClassSkill;

	[Header("Attack Roll")]
	public LocalizedString Concealment;

	public LocalizedString Flanking;

	public LocalizedString SecondaryWeapon;

	public LocalizedString ShootIntoCombat;

	[Header("Armor Class")]
	public LocalizedString ArmorClassBase;

	public LocalizedString Blindness;

	[Header("Combat Maneuver")]
	public LocalizedString CMDBaseValue;

	public LocalizedString TargetStunned;

	public string GetText(BonusType bonusType)
	{
		return bonusType switch
		{
			BonusType.None => string.Empty, 
			BonusType.Concealment => Concealment, 
			BonusType.Flanking => Flanking, 
			BonusType.SecondaryWeapon => SecondaryWeapon, 
			BonusType.ShootIntoCombat => ShootIntoCombat, 
			BonusType.Blindness => Blindness, 
			BonusType.TargetStunned => TargetStunned, 
			_ => throw new ArgumentOutOfRangeException("bonusType", bonusType, null), 
		};
	}
}
