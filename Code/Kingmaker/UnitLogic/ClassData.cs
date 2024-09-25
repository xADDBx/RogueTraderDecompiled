using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic;

public class ClassData : IComparable<ClassData>
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintCharacterClass CharacterClass;

	[NotNull]
	[JsonProperty]
	public readonly List<BlueprintArchetype> Archetypes = new List<BlueprintArchetype>();

	[JsonProperty]
	public int Level { get; set; }

	[CanBeNull]
	[JsonProperty]
	public BlueprintSpellbook Spellbook { get; set; }

	[JsonProperty]
	public bool PriorityEquipment { get; set; }

	public StatType[] RecommendedAttributes
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.OverrideAttributeRecommendations)
				{
					return archetype.RecommendedAttributes;
				}
			}
			return CharacterClass.RecommendedAttributes;
		}
	}

	public StatType[] NotRecommendedAttributes
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.OverrideAttributeRecommendations)
				{
					return archetype.NotRecommendedAttributes;
				}
			}
			return CharacterClass.NotRecommendedAttributes;
		}
	}

	[NotNull]
	public BlueprintStatProgression BaseAttackBonus
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.BaseAttackBonus != null)
				{
					return archetype.BaseAttackBonus;
				}
			}
			return CharacterClass.BaseAttackBonus;
		}
	}

	[NotNull]
	public BlueprintStatProgression FortitudeSave
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.FortitudeSave != null)
				{
					return archetype.FortitudeSave;
				}
			}
			return CharacterClass.FortitudeSave;
		}
	}

	[NotNull]
	public BlueprintStatProgression ReflexSave
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.ReflexSave != null)
				{
					return archetype.ReflexSave;
				}
			}
			return CharacterClass.ReflexSave;
		}
	}

	[NotNull]
	public BlueprintStatProgression WillSave
	{
		get
		{
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				if (archetype.WillSave != null)
				{
					return archetype.WillSave;
				}
			}
			return CharacterClass.WillSave;
		}
	}

	[JsonConstructor]
	public ClassData(JsonConstructorMark _)
	{
	}

	public ClassData([NotNull] BlueprintCharacterClass characterClass)
	{
		CharacterClass = characterClass;
		Spellbook = characterClass.Spellbook;
	}

	public int CalcSkillPoints()
	{
		return CharacterClass.SkillPoints + Archetypes.Sum((BlueprintArchetype a) => a.AddSkillPoints);
	}

	public void AddArchetype([NotNull] BlueprintArchetype archetype)
	{
		if (!Archetypes.Contains(archetype))
		{
			Archetypes.Add(archetype);
			if (archetype.RemoveSpellbook)
			{
				Spellbook = null;
			}
			else if ((bool)archetype.ReplaceSpellbook)
			{
				Spellbook = archetype.ReplaceSpellbook;
			}
		}
	}

	public int CompareTo(ClassData other)
	{
		if (this == other)
		{
			return 0;
		}
		return other?.Level.CompareTo(Level) ?? 1;
	}

	public void PostLoad()
	{
	}
}
