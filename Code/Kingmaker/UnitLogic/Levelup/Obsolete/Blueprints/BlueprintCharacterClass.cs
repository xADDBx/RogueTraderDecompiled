using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Obsolete]
[TypeId("5f2bdd25f161a0d4c97bee89cf923d77")]
public class BlueprintCharacterClass : BlueprintScriptableObject, IUIDataProvider
{
	public const int MinDifficulty = 1;

	public const int MaxDifficulty = 5;

	[ValidateNotEmpty]
	public LocalizedString LocalizedName;

	[ValidateNotEmpty]
	public LocalizedString LocalizedDescription;

	[ValidateNotEmpty]
	public LocalizedString LocalizedDescriptionShort;

	public Sprite m_Icon;

	public int SkillPoints;

	[SerializeField]
	private DiceType HitDie = DiceType.D8;

	public bool HideIfRestricted;

	public bool PrestigeClass;

	public bool IsMythic;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("BaseAttackBonus")]
	private BlueprintStatProgressionReference m_BaseAttackBonus;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("FortitudeSave")]
	private BlueprintStatProgressionReference m_FortitudeSave;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("ReflexSave")]
	private BlueprintStatProgressionReference m_ReflexSave;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("WillSave")]
	private BlueprintStatProgressionReference m_WillSave;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Progression")]
	private BlueprintProgressionReference m_Progression;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("Spellbook")]
	private BlueprintSpellbookReference m_Spellbook;

	[NotNull]
	public StatType[] ClassSkills = new StatType[0];

	[Tooltip("Used to determine whether spell-like abilities granted by this class are considered divine or arcane (default). Also for prerequisites.")]
	public bool IsDivineCaster;

	[Tooltip("Used for prerequisites.")]
	public bool IsArcaneCaster;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Archetypes")]
	private BlueprintArchetypeReference[] m_Archetypes = new BlueprintArchetypeReference[0];

	public int StartingGold;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("StartingItems")]
	private BlueprintItemReference[] m_StartingItems = new BlueprintItemReference[0];

	public int PrimaryColor;

	public int SecondaryColor;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("EquipmentEntities")]
	private KingmakerEquipmentEntityReference[] m_EquipmentEntities = new KingmakerEquipmentEntityReference[0];

	[NotNull]
	public EquipmentEntityLink[] MaleEquipmentEntities = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] FemaleEquipmentEntities = new EquipmentEntityLink[0];

	[SerializeField]
	[Range(1f, 5f)]
	private int m_Difficulty = 1;

	public StatType[] RecommendedAttributes = new StatType[0];

	public StatType[] NotRecommendedAttributes = new StatType[0];

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintFeatureReference[] m_SignatureAbilities = new BlueprintFeatureReference[0];

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("DefaultBuild")]
	private BlueprintUnitFactReference m_DefaultBuild;

	public string Name => LocalizedName;

	public string Description => LocalizedDescription;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => name;

	public bool HasDefaultProgression => DefaultBuild != null;

	public BlueprintStatProgression BaseAttackBonus => m_BaseAttackBonus?.Get();

	public BlueprintStatProgression FortitudeSave => m_FortitudeSave?.Get();

	public BlueprintStatProgression ReflexSave => m_ReflexSave?.Get();

	public BlueprintStatProgression WillSave => m_WillSave?.Get();

	public BlueprintProgression Progression => m_Progression?.Get();

	public BlueprintSpellbook Spellbook => m_Spellbook?.Get();

	public ReferenceArrayProxy<BlueprintArchetype> Archetypes
	{
		get
		{
			BlueprintReference<BlueprintArchetype>[] archetypes = m_Archetypes;
			return archetypes;
		}
	}

	public ReferenceArrayProxy<BlueprintItem> StartingItems
	{
		get
		{
			BlueprintReference<BlueprintItem>[] startingItems = m_StartingItems;
			return startingItems;
		}
	}

	public ReferenceArrayProxy<KingmakerEquipmentEntity> EquipmentEntities
	{
		get
		{
			BlueprintReference<KingmakerEquipmentEntity>[] equipmentEntities = m_EquipmentEntities;
			return equipmentEntities;
		}
	}

	public int Difficulty
	{
		get
		{
			if (m_Difficulty >= 1)
			{
				if (m_Difficulty <= 5)
				{
					return m_Difficulty;
				}
				return 5;
			}
			return 1;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> SignatureAbilities
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] signatureAbilities = m_SignatureAbilities;
			return signatureAbilities;
		}
	}

	[CanBeNull]
	public BlueprintUnitFact DefaultBuild => m_DefaultBuild?.Get();

	[CanBeNull]
	public BuildBalanceRadarChart DefaultBuildBalanceRadarChart => m_DefaultBuild?.Get()?.GetComponent<BuildBalanceRadarChart>();

	public DiceType GetHitDie(BaseUnitEntity unit)
	{
		return HitDie;
	}

	public bool MeetsPrerequisites([NotNull] BaseUnitEntity unit, [NotNull] LevelUpState state)
	{
		if (IgnorePrerequisites.Ignore)
		{
			return true;
		}
		ClassData classData = unit.Progression.GetClassData(this);
		if (classData != null && classData.Level >= classData.CharacterClass.Progression.MaxLevel)
		{
			return false;
		}
		bool? flag = null;
		bool? flag2 = null;
		for (int i = 0; i < base.ComponentsArray.Length; i++)
		{
			Prerequisite_Obsolete prerequisite_Obsolete = base.ComponentsArray[i] as Prerequisite_Obsolete;
			if (!prerequisite_Obsolete)
			{
				continue;
			}
			bool flag3 = prerequisite_Obsolete.Check(null, unit, state);
			switch (prerequisite_Obsolete.Group)
			{
			case Prerequisite_Obsolete.GroupType.All:
				flag = ((!flag.HasValue) ? flag3 : (flag.Value && flag3));
				break;
			case Prerequisite_Obsolete.GroupType.Any:
				flag2 = ((!flag2.HasValue) ? flag3 : (flag2.Value || flag3));
				break;
			case Prerequisite_Obsolete.GroupType.ForcedTrue:
				if (flag3)
				{
					return true;
				}
				break;
			}
		}
		if (flag ?? true)
		{
			return flag2 ?? true;
		}
		return false;
	}

	public void RestrictPrerequisites([NotNull] BaseUnitEntity unit, [NotNull] LevelUpState state)
	{
		if (IgnorePrerequisites.Ignore)
		{
			return;
		}
		for (int i = 0; i < base.ComponentsArray.Length; i++)
		{
			Prerequisite_Obsolete prerequisite_Obsolete = base.ComponentsArray[i] as Prerequisite_Obsolete;
			if ((bool)prerequisite_Obsolete)
			{
				prerequisite_Obsolete.Restrict(null, unit, state);
			}
		}
	}

	public bool IsFavorite([NotNull] BaseUnitEntity unit)
	{
		foreach (Feature feature in unit.Progression.Features)
		{
			using EntityFact.ComponentEnumerator<RecommendedClass> componentEnumerator = feature.SelectComponents<RecommendedClass>().GetEnumerator();
			if (componentEnumerator.MoveNext())
			{
				return componentEnumerator.Current.FavoriteClass == this;
			}
		}
		return false;
	}

	public bool HasEquipmentEntities()
	{
		if (MaleEquipmentEntities.Length == 0)
		{
			return FemaleEquipmentEntities.Length != 0;
		}
		return true;
	}

	public List<EquipmentEntity> LoadClothes(Gender gender, BlueprintRace race)
	{
		return LoadClothes(gender, race.RaceId);
	}

	public void PreloadClothes(Gender gender, BlueprintRace race)
	{
		foreach (EquipmentEntityLink clothesLink in GetClothesLinks(gender, race.RaceId))
		{
			clothesLink.Preload();
		}
	}

	private List<EquipmentEntity> LoadClothes(Gender gender, Race race)
	{
		return (from l in GetClothesLinks(gender, race)
			select l.Load()).ToList();
	}

	public List<EquipmentEntityLink> GetClothesLinks(Gender gender, Race race)
	{
		List<EquipmentEntityLink> list = new List<EquipmentEntityLink>();
		int num = 0;
		foreach (KingmakerEquipmentEntity equipmentEntity in EquipmentEntities)
		{
			if (equipmentEntity != null)
			{
				EquipmentEntityLink[] links = equipmentEntity.GetLinks(gender, race);
				if (links != null)
				{
					list.AddRange(links);
				}
				else
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		if (num != 0)
		{
			PFLog.Default.Error(this, "Класс " + Name + "(" + AssetGuid + ") имеет пустые ссылки в EquipmentEntities");
		}
		EquipmentEntityLink[] collection = ((gender == Gender.Male) ? MaleEquipmentEntities : FemaleEquipmentEntities);
		list.AddRange(collection);
		return list;
	}
}
