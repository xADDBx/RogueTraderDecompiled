using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public abstract class CalculatedPrerequisite
{
	public bool Value { get; }

	public bool Not { get; }

	public string Description => GetDescriptionInternal();

	protected CalculatedPrerequisite(bool value, bool not = false)
	{
		Value = value;
		Not = not;
	}

	protected CalculatedPrerequisite(bool value, [CanBeNull] Prerequisite source)
		: this(value, source?.Not ?? false)
	{
	}

	protected abstract string GetDescriptionInternal();

	public static CalculatedPrerequisite Calculate([NotNull] BlueprintCareerPath careerPath, [NotNull] BaseUnitEntity unit)
	{
		if (unit.Progression.GetRank(careerPath) >= careerPath.Ranks)
		{
			return new CalculatedPrerequisiteMaxRankNotReached(value: false, careerPath.Ranks);
		}
		return CalculateInternal(careerPath.Prerequisites, unit);
	}

	public static CalculatedPrerequisite Calculate([NotNull] PrerequisitesList prerequisitesList, [NotNull] BaseUnitEntity unit)
	{
		return CalculateInternal(prerequisitesList, unit);
	}

	[CanBeNull]
	public static CalculatedPrerequisite Calculate(SelectionStateFeature selection, FeatureSelectionItem selectionItem, [NotNull] BaseUnitEntity unit)
	{
		PrerequisitesList prerequisites = selectionItem.Feature.Prerequisites;
		if ((prerequisites == null || !prerequisites.Any) && selectionItem.MaxRank < 1)
		{
			return null;
		}
		bool flag = selectionItem.MeetRankPrerequisites(unit);
		if (selection != null && selection.Manager.AutoCommit)
		{
			if ((prerequisites == null || !prerequisites.Any) && flag)
			{
				return null;
			}
			if (!(prerequisites.Meet(unit) && flag))
			{
				return CalculatedPrerequisiteSimple.False;
			}
			return CalculatedPrerequisiteSimple.True;
		}
		if (flag)
		{
			return CalculateInternal(prerequisites, unit);
		}
		return CalculateMaxRankPrerequisite(selectionItem.Feature, selectionItem.MaxRank, unit);
	}

	private static CalculatedPrerequisite CalculateMaxRankPrerequisite(BlueprintFeature feature, int maxRank, [NotNull] BaseUnitEntity unit)
	{
		maxRank = Math.Min(maxRank, feature.Ranks);
		bool flag = unit.Progression.GetRank(feature) < maxRank;
		CalculatedPrerequisiteMaxRankNotReached calculatedPrerequisiteMaxRankNotReached = new CalculatedPrerequisiteMaxRankNotReached(flag, maxRank);
		PrerequisitesList prerequisites = feature.Prerequisites;
		if (prerequisites.Empty)
		{
			return calculatedPrerequisiteMaxRankNotReached;
		}
		bool flag2 = prerequisites.Meet(unit);
		return new CalculatedPrerequisiteComposite(flag && flag2, FeaturePrerequisiteComposition.And, not: false, new CalculatedPrerequisite[2]
		{
			calculatedPrerequisiteMaxRankNotReached,
			CalculateInternal(prerequisites, unit)
		});
	}

	[CanBeNull]
	private static CalculatedPrerequisite CalculateInternal([NotNull] PrerequisitesList prerequisitesList, [NotNull] BaseUnitEntity unit, bool not = false)
	{
		List<CalculatedPrerequisite> list = new List<CalculatedPrerequisite>();
		Prerequisite[] list2 = prerequisitesList.List;
		foreach (Prerequisite prerequisite in list2)
		{
			bool value = prerequisite.Meet(unit);
			CalculatedPrerequisite calculatedPrerequisite;
			if (!(prerequisite is PrerequisiteComposite prerequisiteComposite))
			{
				if (!(prerequisite is PrerequisiteFact source))
				{
					if (!(prerequisite is PrerequisiteStat source2))
					{
						if (!(prerequisite is PrerequisiteLevel source3))
						{
							throw new ArgumentOutOfRangeException("prerequisitesList");
						}
						calculatedPrerequisite = new CalculatedPrerequisiteLevel(value, source3);
					}
					else
					{
						calculatedPrerequisite = new CalculatedPrerequisiteStat(value, source2);
					}
				}
				else
				{
					calculatedPrerequisite = new CalculatedPrerequisiteFact(value, source);
				}
			}
			else
			{
				calculatedPrerequisite = CalculateInternal(prerequisiteComposite.Prerequisites, unit, prerequisiteComposite.Not);
			}
			CalculatedPrerequisite item = calculatedPrerequisite;
			list.Add(item);
		}
		if (list.Empty())
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		return new CalculatedPrerequisiteComposite(prerequisitesList.Meet(unit), prerequisitesList.Composition, not, list);
	}

	public static List<BlueprintFeature> CalculateRelyingFeatures(BlueprintCareerPath career, BlueprintFeature featureToAnalyze)
	{
		List<BlueprintFeature> list = new List<BlueprintFeature>();
		BlueprintComponentsEnumerator<AddFeaturesToLevelUp> components = career.GetComponents<AddFeaturesToLevelUp>();
		if (components.Empty())
		{
			return list;
		}
		foreach (AddFeaturesToLevelUp item in components)
		{
			foreach (BlueprintFeature feature in item.Features)
			{
				if ((feature.Prerequisites?.List?.Any((Prerequisite p) => p.IsRelyingOnFeature(featureToAnalyze))).GetValueOrDefault())
				{
					list.AddUnique(feature);
				}
			}
		}
		return list;
	}
}
