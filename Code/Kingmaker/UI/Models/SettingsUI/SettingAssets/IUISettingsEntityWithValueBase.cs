using Kingmaker.Settings.Interfaces;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public interface IUISettingsEntityWithValueBase : IUISettingsEntityBase
{
	ISettingsEntity SettingsEntity { get; }

	bool ModificationAllowed => true;

	string ModificationAllowedReason { get; set; }
}
