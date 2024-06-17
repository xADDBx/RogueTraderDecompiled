using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntityDisplayImagesVM : SettingsEntityVM
{
	private UISettingsEntityDisplayImages m_DisplayImagesEntity;

	public SettingsEntityDisplayImagesVM(UISettingsEntityDisplayImages uiSettingsEntity)
		: base(uiSettingsEntity)
	{
		m_DisplayImagesEntity = uiSettingsEntity;
	}
}
