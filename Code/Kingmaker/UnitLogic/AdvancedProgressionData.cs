using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic;

public class AdvancedProgressionData : ProgressionData
{
	public AdvancedProgressionData(JsonConstructorMark _)
		: base(_)
	{
	}

	public AdvancedProgressionData([NotNull] BlueprintProgression blueprint)
		: base(blueprint)
	{
	}

	protected override LevelEntry[] CalculateLevelEntries()
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
