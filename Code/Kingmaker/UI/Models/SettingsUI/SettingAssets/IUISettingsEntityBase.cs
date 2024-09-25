using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Localization;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public interface IUISettingsEntityBase
{
	LocalizedString Description { get; }

	LocalizedString TooltipDescription { get; }

	bool ShowVisualConnection { get; }

	bool IAmSetHandler { get; }

	bool HasCoopTooltipDescription { get; }

	bool NoDefaultReset { get; }

	List<BlueprintEncyclopediaPageReference> EncyclopediaDescription { get; }
}
