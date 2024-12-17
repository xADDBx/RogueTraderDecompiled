using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;
using Kingmaker.UI.Models.SettingsUI;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;

public class FirstLaunchLanguagePageVM : FirstLaunchSettingsPageVM
{
	public readonly FirstLaunchEntityLanguageVM Languages;

	public FirstLaunchLanguagePageVM()
	{
		Languages = new FirstLaunchEntityLanguageVM(UISettingsRoot.Instance.UIGameMainSettings.LocalizationSetting, forceSetValue: true);
	}

	protected override void DisposeImplementation()
	{
		Languages?.Dispose();
	}
}
