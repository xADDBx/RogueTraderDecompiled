using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[TypeId("d6f5095e34f14990a98461b6cd77d321")]
public class BlueprintAttributeAdvancement : BlueprintStatAdvancement
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintAttributeAdvancement>
	{
	}

	public enum AttributeType
	{
		BallisticSkill,
		WeaponSkill,
		Strength,
		Toughness,
		Agility,
		Intelligence,
		Perception,
		Willpower,
		Fellowship
	}

	[SerializeField]
	private AttributeType Attribute;

	public override int ValuePerRank => 5;

	public override StatType Stat => GetStatTypeByAttribute(Attribute);

	public static StatType GetStatTypeByAttribute(AttributeType attribute)
	{
		return attribute switch
		{
			AttributeType.BallisticSkill => StatType.WarhammerBallisticSkill, 
			AttributeType.WeaponSkill => StatType.WarhammerWeaponSkill, 
			AttributeType.Strength => StatType.WarhammerStrength, 
			AttributeType.Toughness => StatType.WarhammerToughness, 
			AttributeType.Agility => StatType.WarhammerAgility, 
			AttributeType.Intelligence => StatType.WarhammerIntelligence, 
			AttributeType.Perception => StatType.WarhammerPerception, 
			AttributeType.Willpower => StatType.WarhammerWillpower, 
			AttributeType.Fellowship => StatType.WarhammerFellowship, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static AttributeType GetAttributeTypeFromStatType(StatType attribute)
	{
		return attribute switch
		{
			StatType.WarhammerBallisticSkill => AttributeType.BallisticSkill, 
			StatType.WarhammerWeaponSkill => AttributeType.WeaponSkill, 
			StatType.WarhammerStrength => AttributeType.Strength, 
			StatType.WarhammerToughness => AttributeType.Toughness, 
			StatType.WarhammerAgility => AttributeType.Agility, 
			StatType.WarhammerIntelligence => AttributeType.Intelligence, 
			StatType.WarhammerPerception => AttributeType.Perception, 
			StatType.WarhammerWillpower => AttributeType.Willpower, 
			StatType.WarhammerFellowship => AttributeType.Fellowship, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
