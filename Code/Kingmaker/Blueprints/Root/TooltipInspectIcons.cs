using System;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TooltipInspectIcons
{
	[Header("Surface Inspect")]
	public Sprite Wounds;

	public Sprite Number;

	public Sprite DamageDeflection;

	public Sprite CoverMagnitude;

	public Sprite ParryChance;

	public Sprite Armor;

	public Sprite Dodge;

	public Sprite MovePoints;

	[Header("Space Inspect")]
	public Sprite HP;

	public Sprite Evasion;

	public Sprite HitChance;

	public Sprite CriticalChance;

	public Sprite ArmourFore;

	public Sprite ArmourAft;

	public Sprite ArmourPort;

	public Sprite ArmourStarboard;

	public Sprite ShieldFore;

	public Sprite ShieldAft;

	public Sprite ShieldPort;

	public Sprite ShieldStarboard;

	public Sprite PsyRating;

	public Sprite GetArmorIconByType(StarshipHitLocation type)
	{
		return type switch
		{
			StarshipHitLocation.Fore => ArmourFore, 
			StarshipHitLocation.Aft => ArmourAft, 
			StarshipHitLocation.Port => ArmourPort, 
			StarshipHitLocation.Starboard => ArmourStarboard, 
			_ => null, 
		};
	}

	public Sprite GetShieldIconByType(StarshipSectorShieldsType type)
	{
		return type switch
		{
			StarshipSectorShieldsType.Fore => ShieldFore, 
			StarshipSectorShieldsType.Aft => ShieldAft, 
			StarshipSectorShieldsType.Port => ShieldPort, 
			StarshipSectorShieldsType.Starboard => ShieldStarboard, 
			_ => null, 
		};
	}
}
