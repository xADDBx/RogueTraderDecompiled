using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic;

public class ProgressionData
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintProgression Blueprint;

	[NotNull]
	[JsonProperty]
	public readonly List<BlueprintArchetype> Archetypes = new List<BlueprintArchetype>();

	[NotNull]
	public LevelEntry[] LevelEntries { get; private set; }

	[JsonProperty]
	public int Level { get; set; }

	[JsonConstructor]
	public ProgressionData(JsonConstructorMark _)
	{
	}

	public ProgressionData([NotNull] BlueprintProgression blueprint)
	{
		Blueprint = blueprint;
		LevelEntries = CalculateLevelEntries();
	}

	public void PostLoad()
	{
		LevelEntries = CalculateLevelEntries();
	}

	public void AddArchetype([NotNull] BlueprintArchetype archetype)
	{
		if (!Archetypes.Contains(archetype))
		{
			Archetypes.Add(archetype);
			LevelEntries = CalculateLevelEntries();
		}
	}

	[NotNull]
	public LevelEntry GetLevelEntry(int level)
	{
		return LevelEntries.FirstOrDefault((LevelEntry le) => le.Level == level) ?? LevelEntry.Empty;
	}

	public bool CanAddArchetype([NotNull] BlueprintArchetype archetype)
	{
		if ((archetype.ReplaceSpellbook != null || archetype.RemoveSpellbook) && Archetypes.Any((BlueprintArchetype a) => a.ReplaceSpellbook != null || a.RemoveSpellbook))
		{
			return false;
		}
		LevelEntry[] removeFeatures = archetype.RemoveFeatures;
		foreach (LevelEntry levelEntry in removeFeatures)
		{
			LevelEntry levelEntry2 = GetLevelEntry(levelEntry.Level);
			foreach (BlueprintFeatureBase feature in levelEntry.Features)
			{
				int num = levelEntry.Features.Count((BlueprintFeatureBase f) => f == feature);
				int num2 = levelEntry2.Features.Count((BlueprintFeatureBase f) => f == feature);
				if (num > num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected virtual LevelEntry[] CalculateLevelEntries()
	{
		if (Archetypes.Count <= 0)
		{
			return Blueprint.LevelEntries;
		}
		List<LevelEntry> list = new List<LevelEntry>();
		for (int i = 1; i <= 20; i++)
		{
			List<BlueprintFeatureBase> list2 = new List<BlueprintFeatureBase>(Blueprint.GetLevelEntry(i).Features);
			foreach (BlueprintArchetype archetype in Archetypes)
			{
				foreach (BlueprintFeatureBase feature in archetype.GetRemoveEntry(i).Features)
				{
					list2.Remove(feature);
				}
				list2.AddRange(archetype.GetAddEntry(i).Features);
			}
			if (list2.Count > 0)
			{
				LevelEntry levelEntry = new LevelEntry
				{
					Level = i
				};
				levelEntry.SetFeatures(list2);
				list.Add(levelEntry);
			}
		}
		return list.ToArray();
	}
}
