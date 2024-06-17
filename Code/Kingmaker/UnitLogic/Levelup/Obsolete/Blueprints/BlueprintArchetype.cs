using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("f04f17ea4eae4fb89afa5b4021444740")]
public class BlueprintArchetype : BlueprintScriptableObject, IUIDataProvider
{
	public const int MinDifficulty = 1;

	public const int MaxDifficulty = 5;

	[ValidateNotEmpty]
	public LocalizedString LocalizedName;

	[ValidateNotEmpty]
	public LocalizedString LocalizedDescription;

	[ValidateNotEmpty]
	public LocalizedString LocalizedDescriptionShort;

	[SerializeField]
	private Sprite m_Icon;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("ReplaceSpellbook")]
	private BlueprintSpellbookReference m_ReplaceSpellbook;

	public bool RemoveSpellbook;

	public bool BuildChanging;

	[NotNull]
	public LevelEntry[] AddFeatures = new LevelEntry[0];

	[NotNull]
	public LevelEntry[] RemoveFeatures = new LevelEntry[0];

	public bool ReplaceStartingEquipment;

	[ShowIf("ReplaceStartingEquipment")]
	public int StartingGold;

	[NotNull]
	[ShowIf("ReplaceStartingEquipment")]
	[SerializeField]
	[FormerlySerializedAs("StartingItems")]
	private BlueprintItemReference[] m_StartingItems = new BlueprintItemReference[0];

	public bool ReplaceClassSkills;

	[NotNull]
	[ShowIf("ReplaceClassSkills")]
	public StatType[] ClassSkills = new StatType[0];

	public bool ChangeCasterType;

	[ShowIf("ChangeCasterType")]
	[Tooltip("Used to determine whether spell-like abilities granted by this class are considered divine or arcane (default). Also for prerequisites.")]
	public bool IsDivineCaster;

	[ShowIf("ChangeCasterType")]
	[Tooltip("Used for prerequisites.")]
	public bool IsArcaneCaster;

	public int AddSkillPoints;

	public bool OverrideAttributeRecommendations;

	[ShowIf("OverrideAttributeRecommendations")]
	public StatType[] RecommendedAttributes = new StatType[0];

	[ShowIf("OverrideAttributeRecommendations")]
	public StatType[] NotRecommendedAttributes = new StatType[0];

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintFeatureReference[] m_SignatureAbilities = new BlueprintFeatureReference[0];

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("BaseAttackBonus")]
	private BlueprintStatProgressionReference m_BaseAttackBonus;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("FortitudeSave")]
	private BlueprintStatProgressionReference m_FortitudeSave;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("ReflexSave")]
	private BlueprintStatProgressionReference m_ReflexSave;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("WillSave")]
	private BlueprintStatProgressionReference m_WillSave;

	private BlueprintCharacterClass m_ParentClass;

	[SerializeField]
	[Range(1f, 5f)]
	private int m_Difficulty = 1;

	public string Name => LocalizedName;

	public string Description => LocalizedDescription;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => name;

	public BlueprintSpellbook ReplaceSpellbook => m_ReplaceSpellbook?.Get();

	public ReferenceArrayProxy<BlueprintItem> StartingItems
	{
		get
		{
			BlueprintReference<BlueprintItem>[] startingItems = m_StartingItems;
			return startingItems;
		}
	}

	public BlueprintStatProgression BaseAttackBonus => m_BaseAttackBonus?.Get();

	public BlueprintStatProgression FortitudeSave => m_FortitudeSave?.Get();

	public BlueprintStatProgression ReflexSave => m_ReflexSave?.Get();

	public BlueprintStatProgression WillSave => m_WillSave?.Get();

	public int MinFeatureLevel
	{
		get
		{
			if (RemoveSpellbook)
			{
				return 1;
			}
			if (ReplaceSpellbook != null)
			{
				return 1;
			}
			return (from le in AddFeatures.Concat(RemoveFeatures)
				select le.Level).Min();
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

	[NotNull]
	public LevelEntry GetAddEntry(int level)
	{
		LevelEntry levelEntry = new LevelEntry
		{
			Level = level
		};
		LevelEntry[] addFeatures = AddFeatures;
		foreach (LevelEntry levelEntry2 in addFeatures)
		{
			if (levelEntry2.Level != level)
			{
				continue;
			}
			foreach (BlueprintFeatureBase feature in levelEntry2.Features)
			{
				levelEntry.Features.Add(feature);
			}
		}
		return levelEntry;
	}

	[NotNull]
	public LevelEntry GetRemoveEntry(int level)
	{
		LevelEntry levelEntry = new LevelEntry
		{
			Level = level
		};
		LevelEntry[] removeFeatures = RemoveFeatures;
		foreach (LevelEntry levelEntry2 in removeFeatures)
		{
			if (levelEntry2.Level != level)
			{
				continue;
			}
			foreach (BlueprintFeatureBase feature in levelEntry2.Features)
			{
				levelEntry.Features.Add(feature);
			}
		}
		return levelEntry;
	}

	public BlueprintCharacterClass GetParentClass()
	{
		m_ParentClass = (m_ParentClass ? m_ParentClass : BlueprintRoot.Instance.Progression.CharacterClasses.SingleOrDefault((BlueprintCharacterClass c) => c.Archetypes.Contains(this)));
		return m_ParentClass;
	}

	public virtual bool MeetsPrerequisites([NotNull] BaseUnitEntity unit, [NotNull] LevelUpState state)
	{
		if (IgnorePrerequisites.Ignore)
		{
			return true;
		}
		bool? flag = null;
		bool? flag2 = null;
		for (int i = 0; i < base.ComponentsArray.Length; i++)
		{
			Prerequisite_Obsolete prerequisite_Obsolete = base.ComponentsArray[i] as Prerequisite_Obsolete;
			if ((bool)prerequisite_Obsolete)
			{
				bool flag3 = prerequisite_Obsolete.Check(null, unit, state);
				if (prerequisite_Obsolete.Group == Prerequisite_Obsolete.GroupType.All)
				{
					flag = ((!flag.HasValue) ? flag3 : (flag.Value && flag3));
				}
				else
				{
					flag2 = ((!flag2.HasValue) ? flag3 : (flag2.Value || flag3));
				}
			}
		}
		if (flag ?? true)
		{
			return flag2 ?? true;
		}
		return false;
	}
}
