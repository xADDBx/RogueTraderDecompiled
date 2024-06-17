using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

[TypeId("54df1ae30054499682533392d8ac2fd7")]
public class BlueprintWHFeatureSelection : BlueprintFeature, IFeatureSelection
{
	public FeatureGroup Group;

	public int MaxRank;

	public IEnumerable<IFeatureSelectionItem> GetItems(BaseUnitEntity beforeLevelUpUnit, BaseUnitEntity previewUnit, BlueprintCharacterClass @class)
	{
		IEnumerable<AddFeaturesToLevelUp> first = (from i in @class?.GetComponents<AddFeaturesToLevelUp>()
			where i.Group == Group
			select i) ?? Array.Empty<AddFeaturesToLevelUp>();
		IEnumerable<AddFeaturesToLevelUp> components = previewUnit.Facts.GetComponents((AddFeaturesToLevelUp i) => i.Group == Group);
		return first.Concat(components).SelectMany((AddFeaturesToLevelUp i) => i.Features);
	}

	public bool CanSelect(LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
	{
		if (item == null)
		{
			return false;
		}
		Feature feature = state.PreviewUnit.Progression.Features.Get(item.Feature);
		if (feature == null)
		{
			return true;
		}
		if (feature.Rank >= item.Feature.Ranks)
		{
			return false;
		}
		if (MaxRank != 0)
		{
			return feature.Rank < MaxRank;
		}
		return true;
	}

	public FeatureGroup GetGroup()
	{
		return Group;
	}

	public bool IsIgnorePrerequisites()
	{
		return false;
	}

	public bool IsObligatory()
	{
		return false;
	}

	public bool IsSelectionProhibited(BaseUnitEntity unit)
	{
		return false;
	}
}
