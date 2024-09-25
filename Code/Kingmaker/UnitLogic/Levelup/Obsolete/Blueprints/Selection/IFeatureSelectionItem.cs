using JetBrains.Annotations;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

public interface IFeatureSelectionItem : IUIDataProvider
{
	[NotNull]
	BlueprintFeature Feature { get; }

	FeatureParam Param { get; }
}
