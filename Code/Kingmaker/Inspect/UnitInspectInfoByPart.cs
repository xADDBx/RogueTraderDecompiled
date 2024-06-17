using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnitDescription;
using UnityEngine;

namespace Kingmaker.Inspect;

public class UnitInspectInfoByPart
{
	public class AxiomaticPartData
	{
		public BlueprintUnitType UnitType;

		public string TypeDescription;

		public Sprite TypeImage;

		public string TypeName;

		public string TypeKnowledgeStat;

		public AxiomaticPartData(UnitDescription description)
		{
			UnitType = description.TypeBlueprint;
			TypeDescription = description.TypeDescription;
			TypeImage = description.TypeImage;
			TypeName = description.TypeName;
			TypeKnowledgeStat = description.TypeKnowledgeStat;
		}
	}

	public class BasePartData
	{
		public BlueprintRace Race;

		public BlueprintFeature Type;

		public UnitDescription.ClassData[] Classes;

		public Size Size;

		public Alignment Alignment;

		public int Level;

		public int Initiative;

		public Feet Speed;

		public UnitDescription.StatsData Stats;

		public UnitDescription.SkillsData Skills;

		public int Experience;

		public BasePartData(UnitDescription description)
		{
			Race = description.Race;
			Type = description.Type;
			Classes = description.Classes;
			Size = description.Size;
			Alignment = description.Alignment;
			Level = description.HD;
			Initiative = description.Initiative;
			Speed = description.Speed;
			Stats = description.Stats;
			Skills = description.Skills;
			Experience = description.Experience;
		}
	}

	public class DefencePartData
	{
		public UnitDescription.ArmorClass ArmorClass;

		public int HitPoints;

		public int TemporaryHitPoints;

		public UnitDescription.SavesData Saves;

		public List<UnitDescription.SpellResistanceData> SpellResistance;

		public UnitDescription.DamageReduction[] DamageReduction;

		public UnitDescription.ImmunitiesData Immunities;

		public UnitDescription.RegenerationData RegenerationData;

		public int FastHealing;

		public DefencePartData(UnitDescription description)
		{
			ArmorClass = description.AC;
			HitPoints = description.HP;
			TemporaryHitPoints = description.TemporaryHP;
			Saves = description.Saves;
			DamageReduction = description.DR;
			Immunities = description.Immunities;
			RegenerationData = description.Regeneration;
			FastHealing = description.FastHealing;
			if (description.SR == null)
			{
				return;
			}
			SpellResistance = new List<UnitDescription.SpellResistanceData>();
			UnitDescription.SpellResistanceData[] sR = description.SR;
			foreach (UnitDescription.SpellResistanceData sr in sR)
			{
				UnitDescription.SpellResistanceData spellResistanceData = SpellResistance.FirstOrDefault((UnitDescription.SpellResistanceData s) => s.OnlyAgainstSpellDescriptor == sr.OnlyAgainstSpellDescriptor);
				if (spellResistanceData == null)
				{
					SpellResistance.Add(sr);
				}
				else
				{
					spellResistanceData.Value = Mathf.Max(spellResistanceData.Value, sr.Value);
				}
			}
		}
	}

	public class AttackPartData
	{
		public Feet Reach;

		public UnitDescription.AttackData[] Attacks;

		public int BAB;

		public int CMB;

		public int CMD;

		public AttackPartData(UnitDescription description)
		{
			Reach = description.Reach;
			Attacks = description.Attacks;
			BAB = description.BAB;
			CMB = description.CMB;
			CMD = description.CMD;
		}
	}

	public class AbilitiesPartData
	{
		public BlueprintActivatableAbility[] ActivatableAbilities;

		public BlueprintAbility[] Abilities;

		public FeatureUIData[] Features;

		public BlueprintBuff[] Buffs;

		public IUIDataProvider[] Facts;

		public BlueprintAbility[] Spells;

		public AbilitiesPartData(UnitDescription description)
		{
			Abilities = description.Abilities.ToArray();
			ActivatableAbilities = description.ActivatableAbilities.ToArray();
			Features = description.UIFeatures;
			Buffs = description.Buffs.ToArray();
			IUIDataProvider[] facts = description.Facts.Where((BlueprintUnitFact f) => !string.IsNullOrEmpty(f.Name)).ToArray();
			Facts = facts;
			Spells = description.Spells;
		}
	}

	public class ActiveBuffsPartData
	{
		public List<Buff> ActiveBuffs = new List<Buff>();
	}

	[CanBeNull]
	public AxiomaticPartData AxiomaticPart;

	[CanBeNull]
	public BasePartData BasePart;

	[CanBeNull]
	public DefencePartData DefencePart;

	[CanBeNull]
	public AttackPartData OffencePart;

	[CanBeNull]
	public AbilitiesPartData AbilitiesPart;

	[CanBeNull]
	public ActiveBuffsPartData ActiveBuffsPart;

	public InspectUnitsManager.UnitInfo UnitInfo { get; private set; }

	public bool IsEmpty
	{
		get
		{
			if (BasePart == null && DefencePart == null && OffencePart == null)
			{
				return AbilitiesPart == null;
			}
			return false;
		}
	}

	public int DCBase => UnitInfo?.DC ?? 0;

	public int DCDefence
	{
		get
		{
			InspectUnitsManager.UnitInfo unitInfo = UnitInfo;
			if (unitInfo == null)
			{
				return 0;
			}
			return unitInfo.DC + 5;
		}
	}

	public int DCOffence
	{
		get
		{
			InspectUnitsManager.UnitInfo unitInfo = UnitInfo;
			if (unitInfo == null)
			{
				return 0;
			}
			return unitInfo.DC + 10;
		}
	}

	public int DCAbility
	{
		get
		{
			InspectUnitsManager.UnitInfo unitInfo = UnitInfo;
			if (unitInfo == null)
			{
				return 0;
			}
			return unitInfo.DC + 15;
		}
	}

	public UnitInspectInfoByPart(InspectUnitsManager.UnitInfo unitInfo, bool force = false)
	{
		UnitInfo = unitInfo;
		if (unitInfo != null)
		{
			UnitDescription description = UnitDescriptionHelper.GetDescription(unitInfo.Blueprint);
			AxiomaticPart = new AxiomaticPartData(description);
			if (unitInfo.IsUnlocked(UnitInfoPart.Base) || force)
			{
				BasePart = new BasePartData(description);
			}
			if (unitInfo.IsUnlocked(UnitInfoPart.Defence) || force)
			{
				DefencePart = new DefencePartData(description);
			}
			if (unitInfo.IsUnlocked(UnitInfoPart.Offence) || force)
			{
				OffencePart = new AttackPartData(description);
			}
			if (unitInfo.IsUnlocked(UnitInfoPart.Abilities) || force)
			{
				AbilitiesPart = new AbilitiesPartData(description);
			}
		}
	}
}
