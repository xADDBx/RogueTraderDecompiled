using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("bec71e89a676a99458c9e2d0804f2a0c")]
public class BlueprintProgression : BlueprintFeature
{
	[Serializable]
	public class ClassWithLevel
	{
		[SerializeField]
		[ValidateNotNull]
		public BlueprintCharacterClassReference m_Class;

		public int AdditionalLevel;

		public BlueprintCharacterClass Class => m_Class.Get();
	}

	[Serializable]
	public class ArchetypeWithLevel
	{
		[SerializeField]
		public BlueprintArchetypeReference m_Archetype;

		public int AdditionalLevel;

		public BlueprintArchetype Archetype => m_Archetype;
	}

	[NotNull]
	[SerializeField]
	private ClassWithLevel[] m_Classes = new ClassWithLevel[0];

	[NotNull]
	[SerializeField]
	private ArchetypeWithLevel[] m_Archetypes = new ArchetypeWithLevel[0];

	public bool ForAllOtherClasses;

	[NotNull]
	[SerializeField]
	[HideIf("ForAllOtherClasses")]
	private ClassWithLevel[] m_AlternateProgressionClasses = new ClassWithLevel[0];

	public AlternateProgressionType AlternateProgressionType;

	[NotNull]
	public LevelEntry[] LevelEntries = new LevelEntry[0];

	[Tooltip("Icons will be connected with line inside one group")]
	[NotNull]
	public UIGroup[] UIGroups = new UIGroup[0];

	[Tooltip("Icon will be shown in first column of class progression")]
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("UIDeterminatorsGroup")]
	private BlueprintFeatureBaseReference[] m_UIDeterminatorsGroup = new BlueprintFeatureBaseReference[0];

	[SerializeField]
	[FormerlySerializedAs("ExclusiveProgression")]
	private BlueprintCharacterClassReference m_ExclusiveProgression;

	public bool GiveFeaturesForPreviousLevels;

	public IEnumerable<BlueprintCharacterClass> Classes => m_Classes.Select((ClassWithLevel i) => i.Class);

	public ReferenceArrayProxy<BlueprintFeatureBase> UIDeterminatorsGroup
	{
		get
		{
			BlueprintReference<BlueprintFeatureBase>[] uIDeterminatorsGroup = m_UIDeterminatorsGroup;
			return uIDeterminatorsGroup;
		}
	}

	public BlueprintCharacterClass ExclusiveProgression => m_ExclusiveProgression?.Get();

	public BlueprintCharacterClass FirstClass => m_Classes.FirstItem()?.Class;

	[CanBeNull]
	public BlueprintCharacterClass MythicSource
	{
		get
		{
			if (!IsMythic)
			{
				return null;
			}
			return FirstClass;
		}
	}

	public bool IsMythic => false;

	public int MaxLevel => LevelEntries.MaxBy((LevelEntry i) => i.Level)?.Level ?? 0;

	[NotNull]
	public LevelEntry GetLevelEntry(int level)
	{
		LevelEntry levelEntry = new LevelEntry
		{
			Level = level
		};
		LevelEntry[] levelEntries = LevelEntries;
		foreach (LevelEntry levelEntry2 in levelEntries)
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

	public int CalcLevel([NotNull] BaseUnitEntity unit)
	{
		if (m_Classes.Empty() && m_Archetypes.Empty())
		{
			return unit.Progression.CharacterLevel;
		}
		int num = 0;
		ClassWithLevel[] classes = m_Classes;
		foreach (ClassWithLevel classWithLevel in classes)
		{
			if (!classWithLevel.Class.Archetypes.Any((BlueprintArchetype a) => m_Archetypes.Contains((ArchetypeWithLevel i) => i.Archetype == a)))
			{
				num += Math.Max(0, unit.Progression.GetClassLevel(classWithLevel.Class) + classWithLevel.AdditionalLevel);
			}
		}
		ArchetypeWithLevel[] archetypes = m_Archetypes;
		foreach (ArchetypeWithLevel archetypeWithLevel in archetypes)
		{
			foreach (ClassData @class in unit.Progression.Classes)
			{
				if (@class.Archetypes.HasItem(archetypeWithLevel.Archetype))
				{
					num += Math.Max(0, @class.Level + archetypeWithLevel.AdditionalLevel);
				}
			}
		}
		int num2 = 0;
		if (ForAllOtherClasses)
		{
			foreach (ClassData c in unit.Progression.Classes)
			{
				if (!m_Classes.HasItem((ClassWithLevel i) => i.Class == c.CharacterClass) || !c.Archetypes.HasItem((BlueprintArchetype a) => m_Archetypes.HasItem((ArchetypeWithLevel i) => i.Archetype == a)))
				{
					num2 += c.Level;
				}
			}
		}
		else
		{
			classes = m_AlternateProgressionClasses;
			foreach (ClassWithLevel classWithLevel2 in classes)
			{
				if (!classWithLevel2.Class.Archetypes.Any((BlueprintArchetype a) => m_Archetypes.Contains((ArchetypeWithLevel i) => i.Archetype == a)))
				{
					num2 += Math.Max(0, unit.Progression.GetClassLevel(classWithLevel2.Class) + classWithLevel2.AdditionalLevel);
				}
			}
		}
		if (AlternateProgressionType == AlternateProgressionType.Div2)
		{
			num += num2 / 2;
		}
		return num;
	}
}
