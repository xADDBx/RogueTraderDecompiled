using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

public interface IFeatureSelection
{
	[ItemNotNull]
	IEnumerable<IFeatureSelectionItem> GetItems(BaseUnitEntity beforeLevelUpUnit, BaseUnitEntity previewUnit, BlueprintCharacterClass @class);

	bool CanSelect([NotNull] LevelUpState state, [NotNull] FeatureSelectionState selectionState, [CanBeNull] IFeatureSelectionItem item);

	FeatureGroup GetGroup();

	bool IsIgnorePrerequisites();

	bool IsObligatory();

	bool IsSelectionProhibited(BaseUnitEntity unit);
}
