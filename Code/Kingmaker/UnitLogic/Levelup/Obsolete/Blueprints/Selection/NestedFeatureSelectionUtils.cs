using JetBrains.Annotations;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

public static class NestedFeatureSelectionUtils
{
	public static bool AllNestedFeaturesUnavailable([NotNull] LevelUpState state, [NotNull] FeatureSelectionState selectionState, BlueprintFeature feature)
	{
		if (feature is IFeatureSelection selection && !selection.CanSelectAny(state, selectionState))
		{
			return true;
		}
		return false;
	}
}
