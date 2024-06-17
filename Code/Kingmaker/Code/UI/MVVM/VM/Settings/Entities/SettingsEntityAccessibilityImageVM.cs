using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntityAccessibilityImageVM : SettingsEntityVM
{
	private UISettingsEntityAccessiabilityImage m_AccessibilityImageEntity;

	public SettingsEntityAccessibilityImageVM(UISettingsEntityAccessiabilityImage uiSettingsEntity)
		: base(uiSettingsEntity)
	{
		m_AccessibilityImageEntity = uiSettingsEntity;
	}
}
