using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.UniRxExtensions;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntityBoolVM : SettingsEntityWithValueVM
{
	private readonly UISettingsEntityBool m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<bool> TempValue;

	public SettingsEntityBoolVM(UISettingsEntityBool uiSettingsEntity, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		AddDisposable(TempValue = m_UISettingsEntity.Setting.ObserveTempValue());
	}

	public bool GetTempValue()
	{
		return m_UISettingsEntity.Setting.GetTempValue();
	}

	public void SetTempValue(bool value)
	{
		if (ModificationAllowed.Value)
		{
			m_UISettingsEntity.Setting.SetTempValue(value);
		}
	}

	public void ChangeValue()
	{
		SetTempValue(!GetTempValue());
	}
}
