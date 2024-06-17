using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;

public class FirstLaunchSafeZonePageVM : FirstLaunchSettingsPageVM
{
	public readonly SettingsEntitySliderVM Offset;

	public FirstLaunchSafeZonePageVM()
	{
		Offset = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIDisplaySettings.SafeZoneOffset);
	}
}
