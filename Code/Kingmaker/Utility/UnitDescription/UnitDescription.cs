using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Utility.UnitDescription;

[JsonObject]
public class UnitDescription
{
	[JsonObject]
	public class ArmorClass
	{
		[JsonProperty]
		public int Base;

		[JsonProperty]
		public int Touch;

		[JsonProperty]
		public int FlatFooted;
	}

	[JsonObject]
	public class AttackData
	{
		[JsonProperty]
		public BlueprintItemWeapon Weapon;

		[JsonProperty]
		public DamageDescription[] Damage = new DamageDescription[0];

		[JsonProperty]
		public int CriticalEdge;

		[JsonProperty]
		public int CriticalMultiplier;

		[JsonProperty]
		public int[] AttackBonuses;
	}

	[JsonObject]
	public class ClassData
	{
		[JsonProperty]
		public int Level;

		[JsonProperty]
		public BlueprintCharacterClass Class;
	}

	[JsonObject]
	public class DamageReduction
	{
		[JsonProperty]
		public bool Or;

		[JsonProperty]
		public int Value;

		[JsonProperty]
		public bool BypassedByMaterial;

		[JsonProperty]
		public bool BypassedByForm;

		[JsonProperty]
		public bool BypassedByMagic;

		[JsonProperty]
		public int MinEnhancementBonus;

		[JsonProperty]
		public bool BypassedByAlignment;

		[JsonProperty]
		public bool BypassedByReality;

		[CanBeNull]
		[JsonProperty]
		public BlueprintWeaponType BypassedByWeapon;

		[JsonProperty]
		public bool BypassedByMeleeWeapon;
	}

	[JsonObject]
	public class FeatureData
	{
		[JsonProperty]
		public FeatureParam Param;

		[JsonProperty]
		public BlueprintFeature Feature;
	}

	[JsonObject]
	public class ImmunitiesData
	{
		[JsonProperty]
		public bool SpellImmunity;

		[JsonProperty]
		public BlueprintAbility[] SpellImmunityExceptions = new BlueprintAbility[0];

		[JsonProperty]
		public bool SpellImmunityToSingleTargetSpells;

		[JsonProperty]
		public BlueprintAbility[] ImmuneToSpells = new BlueprintAbility[0];

		[JsonProperty]
		public bool NonMagicWeaponImmunity;

		[JsonProperty]
		public bool CriticalHitsImmunity;

		[JsonProperty]
		public bool PrecisionDamageImmunity;

		[JsonProperty]
		public bool AbilityScoreDamageImmunity;

		[JsonProperty]
		public bool AbilityScoreDrainImmunity;

		[JsonProperty]
		public SpellDescriptorWrapper SpellDescriptorImmunity;

		[JsonProperty]
		public UnitCondition[] ConditionImmunity = new UnitCondition[0];

		[JsonProperty]
		public BlueprintBuff[] ImmuneToBuffs = new BlueprintBuff[0];

		[JsonProperty]
		public bool PartialSwarmImmunity;

		[JsonProperty]
		public bool TotalSwarmImmunity;
	}

	[JsonObject]
	public class RegenerationData
	{
		[JsonProperty]
		public int Heal;

		[JsonProperty]
		public bool Unremovable;
	}

	[JsonObject]
	public class SavesData
	{
		[JsonProperty]
		public int Fort;

		[JsonProperty]
		public int Ref;

		[JsonProperty]
		public int Will;

		public string FortStringValue => UIUtility.AddSign(Fort);

		public string RefStringValue => UIUtility.AddSign(Ref);

		public string WillStringValue => UIUtility.AddSign(Will);
	}

	[JsonObject]
	public class SkillsData
	{
		[JsonProperty]
		public int Acrobatics;

		[JsonProperty]
		public int Physique;

		[JsonProperty]
		public int Diplomacy;

		[JsonProperty]
		public int Thievery;

		[JsonProperty]
		public int LoreNature;

		[JsonProperty]
		public int Perception;

		[JsonProperty]
		public int Stealth;

		[JsonProperty]
		public int UseMagicDevice;

		[JsonProperty]
		public int LoreReligion;

		[JsonProperty]
		public int KnowledgeWorld;

		[JsonProperty]
		public int KnowledgeArcana;
	}

	[JsonObject]
	public class SpellResistanceData
	{
		[JsonProperty]
		public int Value;

		[JsonProperty]
		public SpellDescriptor? OnlyAgainstSpellDescriptor;

		public bool HasCondition => OnlyAgainstSpellDescriptor.HasValue;
	}

	[JsonObject]
	public class StatsData
	{
		[JsonProperty]
		public int Str;

		[JsonProperty]
		public int Dex;

		[JsonProperty]
		public int Con;

		[JsonProperty]
		public int Int;

		[JsonProperty]
		public int Wis;

		[JsonProperty]
		public int Cha;
	}

	[JsonObject]
	public class UIDamageInfo
	{
		[JsonProperty]
		public DamageDescription Damage;

		[JsonProperty]
		public bool Half;

		[JsonProperty]
		public bool IsBlast;

		public int warhammerMin;

		public int warhammerMax;

		public bool warhammerEndsTurn;

		public string ToNumString()
		{
			return Damage.Dice.ToNumString(Damage.Bonus, Half);
		}

		public int MinValue()
		{
			return warhammerMin;
		}

		public int MaxValue()
		{
			return warhammerMax;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Damage.Dice);
			if (Damage.Bonus > 0 && IsBlast)
			{
				stringBuilder.Append($"+{Damage.Bonus}");
			}
			return stringBuilder.ToString();
		}
	}

	[JsonProperty]
	public BlueprintUnit Blueprint;

	[JsonProperty]
	public BlueprintRace Race;

	[CanBeNull]
	[JsonProperty]
	public BlueprintFeature Type;

	[JsonProperty]
	public ClassData[] Classes = new ClassData[0];

	[JsonProperty]
	public Size Size;

	[JsonProperty]
	public Alignment Alignment;

	[JsonProperty]
	public int CR;

	[JsonProperty]
	public int Experience;

	[JsonProperty]
	public int HD;

	[JsonProperty]
	public int Initiative;

	[JsonProperty]
	public ArmorClass AC = new ArmorClass();

	[JsonProperty]
	public int HP;

	[JsonProperty]
	public int TemporaryHP;

	[JsonProperty]
	public int FastHealing;

	[JsonProperty]
	public SavesData Saves = new SavesData();

	[JsonProperty]
	public SpellResistanceData[] SR = new SpellResistanceData[0];

	[JsonProperty]
	public DamageReduction[] DR = new DamageReduction[0];

	[JsonProperty]
	public RegenerationData Regeneration = new RegenerationData();

	[JsonProperty]
	public ImmunitiesData Immunities = new ImmunitiesData();

	[JsonProperty]
	public Feet Speed;

	[JsonProperty]
	public Feet Reach;

	[JsonProperty]
	public AttackData[] Attacks = new AttackData[0];

	[JsonProperty]
	public StatsData Stats = new StatsData();

	[JsonProperty]
	public SkillsData Skills = new SkillsData();

	[JsonProperty]
	public int BAB;

	[JsonProperty]
	public int CMB;

	[JsonProperty]
	public int CMD;

	[JsonProperty]
	public BlueprintAbility[] Abilities = new BlueprintAbility[0];

	[JsonProperty]
	public BlueprintActivatableAbility[] ActivatableAbilities = new BlueprintActivatableAbility[0];

	[JsonProperty]
	public FeatureData[] Features = new FeatureData[0];

	[JsonProperty]
	public FeatureUIData[] UIFeatures = new FeatureUIData[0];

	[JsonProperty]
	public BlueprintBuff[] Buffs = new BlueprintBuff[0];

	[JsonProperty]
	public BlueprintItem[] Equipment = new BlueprintItem[0];

	[JsonProperty]
	public BlueprintUnitFact[] Facts = new BlueprintUnitFact[0];

	[JsonProperty]
	public BlueprintAbility[] Spells = new BlueprintAbility[0];

	public BlueprintUnitType TypeBlueprint => Blueprint.Type;

	public string TypeDescription
	{
		get
		{
			LocalizedString localizedString = SimpleBlueprintExtendAsObject.Or(TypeBlueprint, null)?.Description;
			if (localizedString == null)
			{
				return "";
			}
			return localizedString;
		}
	}

	public string TypeName
	{
		get
		{
			LocalizedString localizedString = SimpleBlueprintExtendAsObject.Or(TypeBlueprint, null)?.Name;
			if (localizedString == null)
			{
				return "";
			}
			return localizedString;
		}
	}

	public Sprite TypeImage => SimpleBlueprintExtendAsObject.Or(TypeBlueprint, null)?.Image;

	public string TypeKnowledgeStat
	{
		get
		{
			if (!TypeBlueprint)
			{
				return "";
			}
			return LocalizedTexts.Instance.Stats.GetText(TypeBlueprint.KnowledgeStat);
		}
	}

	public string Name => Blueprint.CharacterName ?? "";
}
