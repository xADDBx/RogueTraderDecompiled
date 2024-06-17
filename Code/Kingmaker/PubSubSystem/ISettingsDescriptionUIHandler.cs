using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.PubSubSystem;

public interface ISettingsDescriptionUIHandler : ISubscriber
{
	void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null);

	void HandleHideSettingsDescription();
}
