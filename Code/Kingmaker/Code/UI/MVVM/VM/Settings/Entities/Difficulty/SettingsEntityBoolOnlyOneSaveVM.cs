using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.UniRxExtensions;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;

public class SettingsEntityBoolOnlyOneSaveVM : SettingsEntityWithValueVM
{
	private readonly UISettingsEntityBoolOnlyOneSave m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<bool> TempValue;

	public readonly bool IsMainMenu;

	public SettingsEntityBoolOnlyOneSaveVM(UISettingsEntityBoolOnlyOneSave uiSettingsEntity, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		AddDisposable(TempValue = m_UISettingsEntity.Setting.ObserveTempValue());
		IsMainMenu = RootUIContext.Instance.IsMainMenu;
	}

	private bool GetTempValue()
	{
		return m_UISettingsEntity.Setting.GetTempValue();
	}

	private void SetTempValue(bool value)
	{
		if (ModificationAllowed.Value)
		{
			m_UISettingsEntity.Setting.SetTempValue(value);
		}
	}

	public void ChangeValue()
	{
		bool currentValue = GetTempValue();
		string text = ((!IsMainMenu) ? (currentValue ? ((string)UIStrings.Instance.SettingsUI.AreYouSureSwitchOffGrimDarkness) : string.Empty) : ((!currentValue) ? ((string)UIStrings.Instance.SettingsUI.AreYouSureSwitchOnGrimDarkness) : string.Empty));
		if (!string.IsNullOrWhiteSpace(text))
		{
			UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					SetTempValue(!currentValue);
				}
			});
		}
		else
		{
			SetTempValue(!currentValue);
		}
	}
}
