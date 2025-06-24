using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class UIIcons
{
	[Header("DefaultIcons")]
	public Sprite DefaultItemIcon;

	public Sprite DefaultAbilityIcon;

	public Sprite DefaultColonyProjectIcon;

	[Header("Tooltips")]
	public TooltipIcons TooltipIcons;

	[Header("Tooltips")]
	public TooltipInspectIcons TooltipInspectIcons;

	[Header("CombatMessage")]
	public Sprite CultAmbush;

	[Header("Cargo")]
	public CargoIcons CargoIcons;

	public CargoTooltipIcons CargoTooltipIcons;

	[Header("SoulMarks")]
	public SoulMarkIcons SoulMarkIcons;

	[Header("CharScores")]
	public Sprite HP;

	public Sprite Damage;

	public Sprite Crit;

	public Sprite RateOfFireMelee;

	public Sprite Attack;

	public Sprite Melee;

	public Color32 MeleeColor;

	public Sprite Range;

	public Color32 RangeColor;

	public Sprite Speed;

	public Sprite Bab;

	public Color32 BabColor;

	public Sprite Penetration;

	public Sprite StatBackground;

	[Header("DamageForms")]
	public Sprite Slashing;

	public Color32 SlashingColor;

	[Header("AbilityTarget")]
	public Sprite TargetPersonal;

	public Sprite TargetAnyLine;

	public Sprite TargetCharge;

	public Sprite TargetAnyOne;

	public Sprite TargetAnyAll;

	public Sprite TargetAllyOne;

	public Sprite TargetAllyAll;

	public Sprite TargetEnemyOne;

	public Sprite TargetEnemyAll;

	public Sprite SpellTargetPoint;

	[Header("SpellTimer")]
	public Sprite TimeIcon;

	[Header("Ability Placeholder Icon")]
	public Sprite EmptyAbilityIcon;

	public Sprite[] AbilityPlaceholderIcon;

	public Sprite DiceD100;

	[Header("Other")]
	public Sprite Male;

	public Sprite Female;

	public Sprite Recommended;

	public Sprite NotRecommended;

	public Sprite Yes;

	public Sprite No;

	public Sprite Attention;

	[Header("Colony")]
	public Sprite ProfitFactor;

	public Sprite Contentment;

	public Sprite Efficiency;

	public Sprite Security;

	public Sprite Reward;

	public Sprite Colony;

	public Sprite ColonyEvent;

	[Header("Factions")]
	public Sprite Drusians;

	public Sprite Explorators;

	public Sprite Kasballica;

	public Sprite Pirates;

	public Sprite ShipVendor;

	[Header("Planets")]
	public Sprite Arid;

	public Sprite Ash;

	public Sprite Barren;

	public Sprite Boreal;

	public Sprite Burning;

	public Sprite Continental;

	public Sprite Frozen;

	public Sprite Ice;

	public Sprite GasGiant;

	public Sprite Lava;

	public Sprite MoonLike;

	public Sprite Planetoid;

	public Sprite Ocean;

	public Sprite Rocky;

	public Sprite Sand;

	public Sprite Savannah;

	public Sprite Snow;

	public Sprite Steppes;

	public Sprite Toxic;

	public Sprite Tropical;

	public Sprite Tundra;

	public Sprite Mined;

	public Sprite Dead;

	public Sprite Cracked;

	[Header("ContextMenu")]
	public Sprite Check;

	public Sprite NotCheck;

	[Header("QuestTypes")]
	public QuestTypeIcons QuestTypesIcons;

	[Space]
	[Header("PetPartyIcons")]
	public Sprite SelectorFrameNormal;

	public Sprite SelectorFramePet;

	[Space]
	public Sprite PetNumberI;

	public Sprite PetNumberII;

	public Sprite PetNumberIII;

	public Sprite PetNumberIV;

	public Sprite PetNumberV;

	public Sprite PetNumberVI;

	public Sprite PetNumberVII;

	public Sprite PetNumberVIII;

	public Sprite PetNumberIX;

	public Sprite PetNumberX;

	public Sprite PetNumberXI;

	public Sprite PetNumberXII;

	public Sprite PetNumberXIII;

	public Sprite PetNumberXIV;

	public Sprite PetNumberXV;

	public Sprite PetNumberXVI;

	public Sprite GetFactionIcon(FactionType factionType)
	{
		return factionType switch
		{
			FactionType.Drusians => Drusians, 
			FactionType.Explorators => Explorators, 
			FactionType.Kasballica => Kasballica, 
			FactionType.Pirates => Pirates, 
			FactionType.ShipVendor => ShipVendor, 
			_ => null, 
		};
	}

	public Sprite GetPlanetIcon(BlueprintPlanet.PlanetType type)
	{
		return type switch
		{
			BlueprintPlanet.PlanetType.Arid => Arid, 
			BlueprintPlanet.PlanetType.Ash => Ash, 
			BlueprintPlanet.PlanetType.Barren => Barren, 
			BlueprintPlanet.PlanetType.Boreal => Boreal, 
			BlueprintPlanet.PlanetType.Burning => Burning, 
			BlueprintPlanet.PlanetType.Continental => Continental, 
			BlueprintPlanet.PlanetType.Frozen => Frozen, 
			BlueprintPlanet.PlanetType.Ice => Ice, 
			BlueprintPlanet.PlanetType.GasGiant => GasGiant, 
			BlueprintPlanet.PlanetType.Lava => Lava, 
			BlueprintPlanet.PlanetType.MoonLike => MoonLike, 
			BlueprintPlanet.PlanetType.Planetoid => Planetoid, 
			BlueprintPlanet.PlanetType.Ocean => Ocean, 
			BlueprintPlanet.PlanetType.Rocky => Rocky, 
			BlueprintPlanet.PlanetType.Sand => Sand, 
			BlueprintPlanet.PlanetType.Savannah => Savannah, 
			BlueprintPlanet.PlanetType.Snow => Snow, 
			BlueprintPlanet.PlanetType.Steppes => Steppes, 
			BlueprintPlanet.PlanetType.Toxic => Toxic, 
			BlueprintPlanet.PlanetType.Tropical => Tropical, 
			BlueprintPlanet.PlanetType.Tundra => Tundra, 
			BlueprintPlanet.PlanetType.Mined => Mined, 
			BlueprintPlanet.PlanetType.Dead => Dead, 
			BlueprintPlanet.PlanetType.Cracked => Cracked, 
			_ => null, 
		};
	}

	public Sprite GetGenderIcon(Gender gender)
	{
		return gender switch
		{
			Gender.Male => Male, 
			Gender.Female => Female, 
			_ => null, 
		};
	}

	public Sprite GetSelectorFrame(bool isPet)
	{
		if (!isPet)
		{
			return SelectorFrameNormal;
		}
		return SelectorFramePet;
	}

	public Sprite GetPetNumberIcon(int number)
	{
		return number switch
		{
			1 => PetNumberI, 
			2 => PetNumberII, 
			3 => PetNumberIII, 
			4 => PetNumberIV, 
			5 => PetNumberV, 
			6 => PetNumberVI, 
			7 => PetNumberVII, 
			8 => PetNumberVIII, 
			9 => PetNumberIX, 
			10 => PetNumberX, 
			11 => PetNumberXI, 
			12 => PetNumberXII, 
			13 => PetNumberXIII, 
			14 => PetNumberXIV, 
			15 => PetNumberXV, 
			16 => PetNumberXVI, 
			_ => null, 
		};
	}
}
