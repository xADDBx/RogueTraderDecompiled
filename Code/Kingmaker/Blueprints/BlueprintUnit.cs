using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Code.GameCore.Blueprints;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using JetBrains.Annotations;
using Kingmaker.AI.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("fa4fa7e4548127a47a2846c91b051065")]
public class BlueprintUnit : BlueprintUnitFact, IBlueprintCreateMechanicEntity<BaseUnitEntity>, IBlueprintUnitExportCharacter
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintUnit>
	{
	}

	[Serializable]
	public class ArmyStat
	{
		[HideInInspector]
		public bool NotModified = true;

		[HideInInspector]
		public int Modifier;

		[HideInInspector]
		public bool isProfesional;
	}

	[Serializable]
	public class UnitSkills
	{
		public int Athletics;

		public int Awareness;

		public int Carouse;

		public int Persuasion;

		public int Demolition;

		public int Coercion;

		public int Medicae;

		public int LoreXenos;

		public int LoreWarp;

		public int LoreImperium;

		public int TechUse;

		public int Commerce;

		public int Logic;
	}

	[Serializable]
	public class UnitBody : IUnitBodyExtension
	{
		public bool DisableHands;

		[SerializeField]
		[HideIf("DisableHands")]
		private BlueprintItemWeaponReference m_EmptyHandWeapon;

		[SerializeField]
		[HideIf("DisableHands")]
		public UnitItemEquipmentHandSettings ItemEquipmentHandSettings = new UnitItemEquipmentHandSettings();

		[SerializeField]
		private BlueprintItemWeaponReference[] m_AdditionalLimbs = new BlueprintItemWeaponReference[0];

		[SerializeField]
		private BlueprintItemWeaponReference[] m_AdditionalSecondaryLimbs = new BlueprintItemWeaponReference[0];

		[SerializeField]
		private BlueprintItemArmorReference m_Armor;

		[SerializeField]
		private BlueprintItemEquipmentShirtReference m_Shirt;

		[SerializeField]
		private BlueprintItemEquipmentBeltReference m_Belt;

		[SerializeField]
		private BlueprintItemEquipmentHeadReference m_Head;

		[SerializeField]
		private BlueprintItemEquipmentGlassesReference m_Glasses;

		[SerializeField]
		private BlueprintItemEquipmentFeetReference m_Feet;

		[SerializeField]
		private BlueprintItemEquipmentGlovesReference m_Gloves;

		[SerializeField]
		private BlueprintItemEquipmentNeckReference m_Neck;

		[SerializeField]
		private BlueprintItemEquipmentRingReference m_Ring1;

		[SerializeField]
		private BlueprintItemEquipmentRingReference m_Ring2;

		[SerializeField]
		private BlueprintItemEquipmentWristReference m_Wrist;

		[SerializeField]
		private BlueprintItemEquipmentShouldersReference m_Shoulders;

		[SerializeField]
		private BlueprintItemEquipmentUsableReference[] m_QuickSlots = new BlueprintItemEquipmentUsableReference[4];

		[SerializeField]
		private BlueprintItemMechadendrite.BlueprintItemMechadendriteReference[] m_Mechadendrites = Array.Empty<BlueprintItemMechadendrite.BlueprintItemMechadendriteReference>();

		[JsonProperty]
		public UnitItemEquipmentHandSettings OverridenUnitItemEquipmentHandSettings { get; set; }

		public BlueprintItemWeapon EmptyHandWeapon
		{
			get
			{
				return m_EmptyHandWeapon?.Get();
			}
			set
			{
				m_EmptyHandWeapon = value.ToReference<BlueprintItemWeaponReference>();
			}
		}

		public ReferenceArrayProxy<BlueprintItemWeapon> AdditionalLimbs
		{
			get
			{
				BlueprintReference<BlueprintItemWeapon>[] additionalLimbs = m_AdditionalLimbs;
				return additionalLimbs;
			}
		}

		public ReferenceArrayProxy<BlueprintItemWeapon> AdditionalSecondaryLimbs
		{
			get
			{
				BlueprintReference<BlueprintItemWeapon>[] additionalSecondaryLimbs = m_AdditionalSecondaryLimbs;
				return additionalSecondaryLimbs;
			}
		}

		public BlueprintItemArmor Armor => m_Armor?.Get();

		public BlueprintItemEquipmentShirt Shirt => m_Shirt?.Get();

		public BlueprintItemEquipmentBelt Belt => m_Belt?.Get();

		public BlueprintItemEquipmentHead Head => m_Head?.Get();

		public BlueprintItemEquipmentGlasses Glasses => m_Glasses?.Get();

		public BlueprintItemEquipmentFeet Feet => m_Feet?.Get();

		public BlueprintItemEquipmentGloves Gloves => m_Gloves?.Get();

		public BlueprintItemEquipmentNeck Neck => m_Neck?.Get();

		public BlueprintItemEquipmentRing Ring1 => m_Ring1?.Get();

		public BlueprintItemEquipmentRing Ring2 => m_Ring2?.Get();

		public BlueprintItemEquipmentWrist Wrist => m_Wrist?.Get();

		public BlueprintItemEquipmentShoulders Shoulders => m_Shoulders?.Get();

		public ReferenceArrayProxy<BlueprintItemEquipmentUsable> QuickSlots
		{
			get
			{
				BlueprintReference<BlueprintItemEquipmentUsable>[] quickSlots = m_QuickSlots;
				return quickSlots;
			}
		}

		public ReferenceArrayProxy<BlueprintItemMechadendrite> Mechadendrites
		{
			get
			{
				BlueprintReference<BlueprintItemMechadendrite>[] mechadendrites = m_Mechadendrites;
				return mechadendrites;
			}
		}

		[CanBeNull]
		public BlueprintItemEquipmentHand GetHandEquipment(int i, bool main, UnitItemEquipmentHandSettings settings)
		{
			return i switch
			{
				0 => main ? settings.PrimaryHand : settings.SecondaryHand, 
				1 => main ? settings.PrimaryHandAlternative1 : settings.SecondaryHandAlternative1, 
				_ => null, 
			};
		}

		void IUnitBodyExtension.SetBody(PartUnitBody body, BlueprintUnit blueprintUnit)
		{
		}
	}

	private interface IUnitBodyExtension
	{
		void SetBody(PartUnitBody body, BlueprintUnit blueprintUnit);
	}

	[SerializeField]
	private BlueprintUnitTypeReference m_Type;

	[SerializeField]
	private BlueprintArmyDescriptionReference m_Army;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.UnitName)]
	public new SharedStringAsset LocalizedName;

	public Gender Gender;

	public Size Size = Size.Medium;

	public bool IsLeftHanded;

	public Color Color = new Color(0.15f, 0.15f, 0.15f, 1f);

	[SerializeField]
	private BlueprintRaceReference m_Race;

	public Alignment Alignment = Alignment.TrueNeutral;

	[SerializeField]
	private BlueprintPortraitReference m_Portrait;

	[ValidateNotNull]
	public UnitViewLink Prefab;

	[SerializeField]
	[CanBeNull]
	private UnitCustomizationPresetReference m_CustomizationPreset;

	[SerializeField]
	private BlueprintUnitVisualSettings.Reference m_VisualSettings;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintFactionReference m_Faction;

	public FactionOverrides FactionOverrides;

	[SerializeField]
	private BlueprintItemReference[] m_StartingInventory;

	[Header("Brain settings")]
	[SerializeField]
	private BlueprintBrainBaseReference m_Brain;

	public BlueprintBrainBaseReference[] AlternativeBrains = new BlueprintBrainBaseReference[0];

	[Tooltip("If true, unit will use scatter shots more carefully")]
	public bool IsCarefulShooter;

	[Tooltip("If true, unit won't return to target position on combat leave")]
	public bool IsStayOnSameSpotAfterCombat;

	[Header("Body")]
	public UnitBody Body = new UnitBody();

	[Header("Stats")]
	[HideIf("IsNewStat")]
	public int OldWarhammerBallisticSkill;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerBallisticSkillSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerWeaponSkill;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerWeaponSkillSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerStrength;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerStrengthSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerToughness;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerToughnessSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerAgility;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerAgilitySetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerIntelligence;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerIntelligenceSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerWillpower;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerWillpowerSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerPerception;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerPerceptionSetting = new ArmyStat();

	[HideIf("IsNewStat")]
	public int OldWarhammerFellowship;

	[HideIf("IsOldStat")]
	public ArmyStat WarhammerFellowshipSetting = new ArmyStat();

	public float WarhammerMovementApPerCell = 1f;

	public float WarhammerMovementApPerCellThreateningArea = 3f;

	public int WarhammerInitialAPBlue = 3;

	public int WarhammerInitialAPYellow = 3;

	public Feet Speed = 30.Feet();

	public UnitSkills Skills;

	public int MaxHP = 6;

	[Header("Templates")]
	[SerializeField]
	private BlueprintUnitTemplateReference[] m_AdditionalTemplates;

	[Header("Facts")]
	[SerializeField]
	private BlueprintUnitFactReference[] m_AddFacts;

	[Tooltip("Trap actors, mapobject cast targets and other units that are not actually subject ot game mechanics. Cheaters can use any ability, are never ingame but do show FX")]
	public bool IsCheater;

	public bool IsFake;

	public bool VisualsDone;

	public UnitDifficultyType DifficultyType;

	public UnitSubtype Subtype = UnitSubtype.Default;

	private AddTags m_CachedTags;

	private int? m_DefaultLevel;

	[CanBeNull]
	public BlueprintArmyDescription Army
	{
		get
		{
			return m_Army;
		}
		set
		{
			m_Army = value.ToReference<BlueprintArmyDescriptionReference>();
		}
	}

	public BlueprintUnitType Type => m_Type?.Get();

	public string CharacterName
	{
		get
		{
			if ((bool)LocalizedName)
			{
				return LocalizedName.String;
			}
			return "-unit name not set-";
		}
	}

	public BlueprintRace Race => m_Race?.Get();

	private bool IsNewStat => Army != null;

	private bool IsOldStat => !IsNewStat;

	[NotNull]
	public BlueprintPortrait PortraitSafe
	{
		get
		{
			if (m_Portrait.IsEmpty())
			{
				if (Gender != 0)
				{
					return BlueprintRoot.Instance.UIConfig.Portraits.FemalePlaceholderPortrait;
				}
				return BlueprintRoot.Instance.UIConfig.Portraits.MalePlaceholderPortrait;
			}
			return m_Portrait.Get();
		}
	}

	public UnitCustomizationPreset CustomizationPreset
	{
		get
		{
			return m_CustomizationPreset?.Get();
		}
		set
		{
			m_CustomizationPreset = value.ToReference<UnitCustomizationPresetReference>();
		}
	}

	public UnitVisualSettings VisualSettings => m_VisualSettings?.Get()?.Settings ?? UnitVisualSettings.Empty;

	public BlueprintFaction Faction => m_Faction?.Get();

	public ReferenceArrayProxy<BlueprintItem> StartingInventory
	{
		get
		{
			BlueprintReference<BlueprintItem>[] startingInventory = m_StartingInventory;
			return startingInventory;
		}
	}

	public BlueprintBrainBase DefaultBrain => m_Brain?.Get();

	public int WarhammerBallisticSkill => GetAttributeValue(StatType.WarhammerBallisticSkill);

	public int WarhammerWeaponSkill => GetAttributeValue(StatType.WarhammerWeaponSkill);

	public int WarhammerStrength => GetAttributeValue(StatType.WarhammerStrength);

	public int WarhammerToughness => GetAttributeValue(StatType.WarhammerToughness);

	public int WarhammerAgility => GetAttributeValue(StatType.WarhammerAgility);

	public int WarhammerIntelligence => GetAttributeValue(StatType.WarhammerIntelligence);

	public int WarhammerWillpower => GetAttributeValue(StatType.WarhammerWillpower);

	public int WarhammerPerception => GetAttributeValue(StatType.WarhammerPerception);

	public int WarhammerFellowship => GetAttributeValue(StatType.WarhammerFellowship);

	public ReferenceArrayProxy<BlueprintUnitTemplate> AdditionalTemplates
	{
		get
		{
			BlueprintReference<BlueprintUnitTemplate>[] additionalTemplates = m_AdditionalTemplates;
			return additionalTemplates;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> AddFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] addFacts = m_AddFacts;
			return addFacts;
		}
	}

	public int CR
	{
		get
		{
			Experience component = this.GetComponent<Experience>();
			if (component == null || (component.CR == 1 && Math.Abs(component.Modifier - 0.5f) < 0.001f))
			{
				return 0;
			}
			return component.CR;
		}
	}

	[CanBeNull]
	public AddTags Tags => BlueprintComponentExtendAsObject.Or(m_CachedTags, null) ?? BlueprintComponentExtendAsObject.Or(m_CachedTags = this.GetComponent<AddTags>(), null);

	public IEnumerable<BlueprintFaction> AttackFactions
	{
		get
		{
			HashSet<BlueprintFaction> hashSet = new HashSet<BlueprintFaction>(Faction.AttackFactions);
			hashSet.UnionWith(FactionOverrides.AttackFactionsToAdd);
			hashSet.ExceptWith(FactionOverrides.AttackFactionsToRemove);
			return hashSet;
		}
	}

	public bool IsCompanion => Faction == BlueprintRoot.Instance.PlayerFaction;

	protected override bool ShowDisplayName => false;

	protected override bool ShowDescription => false;

	protected override Type GetFactType()
	{
		return typeof(EntityFact);
	}

	public bool CheckEqualsWithPrototype(BlueprintUnit other)
	{
		if (this != other)
		{
			if (base.PrototypeLink is BlueprintUnit blueprintUnit)
			{
				return blueprintUnit.CheckEqualsWithPrototype(other);
			}
			return false;
		}
		return true;
	}

	public void PreloadResources()
	{
		VisualSettings.ArmorFx.Preload();
		VisualSettings.BloodPuddleFx.Preload();
		VisualSettings.DismemberFx.Preload();
		VisualSettings.RipLimbsApartFx.Preload();
		BlueprintRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry b) => b.Type == VisualSettings.SurfaceType)?.PreloadResources();
	}

	public BaseUnitEntity CreateEntity(string uniqueId = null, bool isInGame = true)
	{
		if (uniqueId == null)
		{
			uniqueId = Uuid.Instance.CreateString();
		}
		return Entity.Initialize((this is BlueprintStarship blueprint) ? ((BaseUnitEntity)new StarshipEntity(uniqueId, isInGame, blueprint)) : ((BaseUnitEntity)new UnitEntity(uniqueId, isInGame, this)));
	}

	public int GetAttributeValue(StatType statType, bool onlyBase = true)
	{
		if (Army == null)
		{
			return statType switch
			{
				StatType.WarhammerBallisticSkill => OldWarhammerBallisticSkill, 
				StatType.WarhammerWeaponSkill => OldWarhammerWeaponSkill, 
				StatType.WarhammerStrength => OldWarhammerStrength, 
				StatType.WarhammerToughness => OldWarhammerToughness, 
				StatType.WarhammerAgility => OldWarhammerAgility, 
				StatType.WarhammerIntelligence => OldWarhammerIntelligence, 
				StatType.WarhammerWillpower => OldWarhammerWillpower, 
				StatType.WarhammerPerception => OldWarhammerPerception, 
				StatType.WarhammerFellowship => OldWarhammerFellowship, 
				_ => throw new InvalidEnumArgumentException("statType", (int)statType, typeof(StatType)), 
			};
		}
		ArmyStat attributeSettings = GetAttributeSettings(statType);
		BlueprintArmyDescription.ArmyStat attributeSettings2 = Army.GetAttributeSettings(statType);
		int num = DifficultyType - Army.DifficultyType;
		int num2 = ((!attributeSettings.NotModified) ? attributeSettings.Modifier : 0);
		int num3 = ((attributeSettings.NotModified ? attributeSettings2.isProfessional : attributeSettings.isProfesional) ? (num * 2) : num);
		int result = attributeSettings2.Value + num3 * 5 + num2;
		if (onlyBase)
		{
			return result;
		}
		return GetDifficultyBaseStat(statType, DifficultyType, ContextData<BlueprintUnitCheckerInEditorContextData>.Current?.AreaCR ?? Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0);
	}

	public int GetDifficultyBaseStat(StatType statType, UnitDifficultyType difficultyType, int challengeRating = -1)
	{
		if (Army == null)
		{
			return 0;
		}
		if (statType == StatType.WarhammerToughness)
		{
			float num = ((difficultyType > UnitDifficultyType.Common) ? 0.9f : 0.6f);
			int num2 = 5 * (int)((27f + num * (float)challengeRating + ((challengeRating >= 15) ? 4.6f : 0f) + (float)((challengeRating >= 34) ? 8 : 0)) / 5f);
			int num3 = ((challengeRating >= 25) ? 10 : 5);
			return num2 + (int)difficultyType * num3;
		}
		ArmyStat attributeSettings = GetAttributeSettings(statType);
		BlueprintArmyDescription.ArmyStat attributeSettings2 = Army.GetAttributeSettings(statType);
		bool isProfessional = (attributeSettings.NotModified ? attributeSettings2.isProfessional : attributeSettings.isProfesional);
		BlueprintItemWeapon primaryHand = Body.ItemEquipmentHandSettings.PrimaryHand as BlueprintItemWeapon;
		BlueprintItemWeapon secondaryHand = Body.ItemEquipmentHandSettings.SecondaryHand as BlueprintItemWeapon;
		BlueprintItemWeapon primaryHandAlternative = Body.ItemEquipmentHandSettings.PrimaryHandAlternative1 as BlueprintItemWeapon;
		BlueprintItemWeapon secondaryHandAlternative = Body.ItemEquipmentHandSettings.SecondaryHandAlternative1 as BlueprintItemWeapon;
		bool isBoss = difficultyType >= UnitDifficultyType.MiniBoss;
		MobTypeForStatCalculations? newType = base.ComponentsArray.OfType<ReplaceMobTypeForStatCalculations>().FirstOrDefault()?.NewType;
		bool isMelee = MobStatHelper.IsMobTypeMelee(primaryHand, secondaryHand, primaryHandAlternative, secondaryHandAlternative, newType);
		bool isRanged = MobStatHelper.IsMobTypeRanged(isMelee, newType);
		bool isPrimary = MobStatHelper.IsStatTypePrimary(statType, isMelee, isProfessional, attributeSettings, isRanged, isBoss, newType);
		bool isSecondary = MobStatHelper.IsStatTypeSecondary(statType, isMelee, attributeSettings, isProfessional, isRanged, isBoss);
		int cr = ((challengeRating >= 0) ? challengeRating : CR);
		return GetDifficultyBaseStatInternal(difficultyType, isPrimary, isSecondary, cr);
	}

	public int GetDefaultLevel()
	{
		if (m_DefaultLevel.HasValue)
		{
			return m_DefaultLevel.Value;
		}
		m_DefaultLevel = 0;
		BlueprintUnitFactReference[] addFacts = m_AddFacts;
		for (int i = 0; i < addFacts.Length; i++)
		{
			foreach (ApplyCareerPath component in addFacts[i].Get().GetComponents<ApplyCareerPath>())
			{
				if (component.CareerPath is BlueprintCareerPath)
				{
					m_DefaultLevel += component.Ranks;
				}
			}
		}
		m_DefaultLevel = Mathf.Max(1, m_DefaultLevel.Value);
		return m_DefaultLevel.Value;
	}

	private static int GetDifficultyBaseStatInternal(UnitDifficultyType difficultyType, bool isPrimary, bool isSecondary, int cr)
	{
		switch (difficultyType)
		{
		case UnitDifficultyType.Swarm:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 20;
				}
				return 20 + (int)((double)cr * 0.5 / 5.0) * 5;
			}
			return 25 + (int)((double)cr * 0.65 / 5.0) * 5;
		case UnitDifficultyType.Common:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 20 + (int)((double)cr * 0.2 / 5.0) * 5;
				}
				return 20 + (int)((double)cr * 0.6 / 5.0) * 5;
			}
			return 30 + (int)((double)cr * 0.75 / 5.0) * 5;
		case UnitDifficultyType.Hard:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 20 + (int)((double)cr * 0.3 / 5.0) * 5;
				}
				return 25 + (int)((double)cr * 0.7 / 5.0) * 5;
			}
			return 30 + (int)((double)cr * 0.85 / 5.0) * 5;
		case UnitDifficultyType.Elite:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 20 + (int)((double)cr * 0.4 / 5.0) * 5;
				}
				return 30 + (int)((double)cr * 0.8 / 5.0) * 5;
			}
			return 35 + (int)((double)cr * 1.05 / 5.0) * 5;
		case UnitDifficultyType.MiniBoss:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 25 + (int)((double)cr * 0.5 / 5.0) * 5;
				}
				return 30 + (int)((double)cr * 0.9 / 5.0) * 5;
			}
			return 40 + (int)((double)cr * 1.15 / 5.0) * 5;
		case UnitDifficultyType.Boss:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 30 + (int)((double)cr * 0.65 / 5.0) * 5;
				}
				return 35 + cr / 5 * 5;
			}
			return 45 + (int)((double)cr * 1.3 / 5.0) * 5;
		case UnitDifficultyType.ChapterBoss:
			if (!isPrimary)
			{
				if (!isSecondary)
				{
					return 35 + (int)((double)cr * 0.9 / 5.0) * 5;
				}
				return 40 + (int)((double)cr * 1.1 / 5.0) * 5;
			}
			return 50 + (int)((double)cr * 1.45 / 5.0) * 5;
		default:
			return 30;
		}
	}

	public int GetDifficultyDamageBonus(UnitDifficultyType difficultyType, int challengeRating = -1)
	{
		int num = ((challengeRating >= 0) ? challengeRating : CR);
		return difficultyType switch
		{
			UnitDifficultyType.Swarm => (int)((double)num * 0.2), 
			UnitDifficultyType.Common => (int)((double)num * 0.2), 
			UnitDifficultyType.Hard => (int)((double)num * 0.4), 
			UnitDifficultyType.Elite => (int)((double)num * 0.4), 
			UnitDifficultyType.MiniBoss => 4 + (int)((double)num * 0.5), 
			UnitDifficultyType.Boss => 4 + (int)((double)num * 0.5), 
			UnitDifficultyType.ChapterBoss => 4 + (int)((double)num * 0.5), 
			_ => 0, 
		};
	}

	public int GetDifficultyPenetrationBonus(UnitDifficultyType difficultyType, int challengeRating = -1)
	{
		int num = ((challengeRating >= 0) ? challengeRating : CR);
		return difficultyType switch
		{
			UnitDifficultyType.Swarm => 0, 
			UnitDifficultyType.Common => 0, 
			UnitDifficultyType.Hard => (int)((double)num * 0.4 / 10.0) * 10, 
			UnitDifficultyType.Elite => (int)((double)num * 0.4 / 10.0) * 10, 
			UnitDifficultyType.MiniBoss => 10 + (int)((double)num * 0.8 / 10.0) * 10, 
			UnitDifficultyType.Boss => 10 + (int)((double)num * 0.8 / 10.0) * 10, 
			UnitDifficultyType.ChapterBoss => 10 + (int)((double)num * 0.8 / 10.0) * 10, 
			_ => 0, 
		};
	}

	private ArmyStat GetAttributeSettings(StatType statType)
	{
		return statType switch
		{
			StatType.WarhammerBallisticSkill => WarhammerBallisticSkillSetting, 
			StatType.WarhammerWeaponSkill => WarhammerWeaponSkillSetting, 
			StatType.WarhammerStrength => WarhammerStrengthSetting, 
			StatType.WarhammerToughness => WarhammerToughnessSetting, 
			StatType.WarhammerAgility => WarhammerAgilitySetting, 
			StatType.WarhammerIntelligence => WarhammerIntelligenceSetting, 
			StatType.WarhammerWillpower => WarhammerWillpowerSetting, 
			StatType.WarhammerPerception => WarhammerPerceptionSetting, 
			StatType.WarhammerFellowship => WarhammerFellowshipSetting, 
			_ => throw new InvalidEnumArgumentException("statType", (int)statType, typeof(StatType)), 
		};
	}

	public void TrySetupOverridenUnitBodyHandsSettings()
	{
		OverrideUnitBodyWithRandomHandsSettings component = this.GetComponent<OverrideUnitBodyWithRandomHandsSettings>();
		if (component == null)
		{
			return;
		}
		float num = PFStatefulRandom.Blueprints.Range(0f, component.TotalWeightPercent);
		int num2 = 0;
		UnitItemEquipmentHandSettings unitItemEquipmentHandSettings = null;
		UnitItemEquipmentHandSettingsWithWeights[] array = component.SettingsWithWeights.EmptyIfNull();
		foreach (UnitItemEquipmentHandSettingsWithWeights unitItemEquipmentHandSettingsWithWeights in array)
		{
			num2 += unitItemEquipmentHandSettingsWithWeights.Weight;
			if (!((float)num2 <= num))
			{
				unitItemEquipmentHandSettings = unitItemEquipmentHandSettingsWithWeights.UnitHandsSettings;
				break;
			}
		}
		if (unitItemEquipmentHandSettings != null)
		{
			Body.OverridenUnitItemEquipmentHandSettings = unitItemEquipmentHandSettings;
		}
	}

	void IBlueprintUnitExportCharacter.SyncFacts(BlueprintUnitFact[] facts)
	{
	}

	void IBlueprintUnitExportCharacter.SyncBody(PartUnitBody body)
	{
		((IUnitBodyExtension)Body)?.SetBody(body, this);
	}
}
