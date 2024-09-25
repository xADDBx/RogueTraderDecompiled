using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;

public interface IKeyBindingSetupDialogHandler : ISubscriber
{
	void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex);
}
