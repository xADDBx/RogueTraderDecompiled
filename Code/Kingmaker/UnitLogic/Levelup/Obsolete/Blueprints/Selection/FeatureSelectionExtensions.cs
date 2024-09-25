using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

public static class FeatureSelectionExtensions
{
	public static bool CanSelectAny(this IFeatureSelection selection, [NotNull] LevelUpState state, [NotNull] FeatureSelectionState selectionState)
	{
		return selection.GetItems(state).Any((IFeatureSelectionItem item) => selection.CanSelect(state, selectionState, item));
	}

	[ItemNotNull]
	public static IEnumerable<IFeatureSelectionItem> GetItems(this IFeatureSelection selection, [NotNull] LevelUpState state)
	{
		return selection.GetItems(state.TargetUnit, state.PreviewUnit, state.SelectedClass);
	}
}
