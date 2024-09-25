using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;

public class AddedOnLevelUpFeatures
{
	private readonly Dictionary<FeatureGroup, List<AddFeaturesToLevelUp>[]> m_Features;

	private readonly CareerPathVM m_CareerPathVM;

	private int EntriesTotalCount => m_CareerPathVM.RankEntriesScan.Count;

	public AddedOnLevelUpFeatures(CareerPathVM careerPathVM)
	{
		m_CareerPathVM = careerPathVM;
		m_Features = new Dictionary<FeatureGroup, List<AddFeaturesToLevelUp>[]>();
	}

	public void RefreshSelectedFeatureAtRank(RankEntrySelectionFeatureVM selectedFeature, int entryId)
	{
		if (selectedFeature == null)
		{
			return;
		}
		foreach (var (_, array2) in m_Features)
		{
			if (array2 != null && entryId < array2.Length)
			{
				array2[entryId].Clear();
			}
		}
		List<AddFeaturesToLevelUp> list = new List<AddFeaturesToLevelUp>();
		try
		{
			ExtractFeaturesToAdd(selectedFeature.Feature, list);
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"Error at extracting features,\n{arg}");
			list = selectedFeature.Feature.ComponentsArray.OfType<AddFeaturesToLevelUp>().ToList();
		}
		foreach (AddFeaturesToLevelUp item in list)
		{
			FeatureGroup group = item.Group;
			if (!m_Features.ContainsKey(group))
			{
				m_Features.Add(group, new List<AddFeaturesToLevelUp>[EntriesTotalCount]);
				for (int i = 0; i < EntriesTotalCount; i++)
				{
					m_Features[group][i] = new List<AddFeaturesToLevelUp>();
				}
			}
			m_Features[group][entryId].Add(item);
		}
	}

	private void ExtractFeaturesToAdd(BlueprintFeature feature, List<AddFeaturesToLevelUp> extractedFeatures)
	{
		BlueprintComponent[] componentsArray = feature.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent is AddFeaturesToLevelUp item)
			{
				extractedFeatures.Add(item);
			}
			else
			{
				if (!(blueprintComponent is AddFacts { Facts: var facts }))
				{
					continue;
				}
				foreach (BlueprintUnitFact item2 in facts)
				{
					if (item2 is BlueprintFeature feature2)
					{
						ExtractFeaturesToAdd(feature2, extractedFeatures);
					}
				}
			}
		}
	}

	public void ExcludeUnavailableFeatures(FeatureGroup group, int entryId, List<FeatureSelectionItem> items)
	{
		if (!m_Features.ContainsKey(group))
		{
			return;
		}
		List<AddFeaturesToLevelUp> list = new List<AddFeaturesToLevelUp>();
		for (int j = entryId; j < EntriesTotalCount; j++)
		{
			list.AddRange(m_Features[group][j]);
		}
		foreach (AddFeaturesToLevelUp featuresToLevelUp in list)
		{
			foreach (BlueprintFeature feature in featuresToLevelUp.Features)
			{
				items.RemoveAll((FeatureSelectionItem i) => i.SourceBlueprint == featuresToLevelUp.OwnerBlueprint && i.Feature == feature);
			}
		}
	}

	public bool NeedUpdateFor(FeatureGroup featureGroup)
	{
		return m_Features.ContainsKey(featureGroup);
	}
}
