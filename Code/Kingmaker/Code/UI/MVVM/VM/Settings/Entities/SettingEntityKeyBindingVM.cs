using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.UniRxExtensions;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingEntityKeyBindingVM : SettingsEntityWithValueVM
{
	private readonly UISettingsEntityKeyBinding m_UISettingsEntityKeybind;

	public readonly ReadOnlyReactiveProperty<KeyBindingData> TempBindingValue1;

	public readonly ReadOnlyReactiveProperty<KeyBindingData> TempBindingValue2;

	public SettingEntityKeyBindingVM(UISettingsEntityKeyBinding uiSettingsEntity, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntityKeybind = uiSettingsEntity;
		SettingsEntityKeyBindingPair settingKeyBindingPair = uiSettingsEntity.SettingKeyBindingPair;
		AddDisposable(TempBindingValue1 = (from pair in settingKeyBindingPair.ObserveTempValue()
			select pair.Binding1).ToReadOnlyReactiveProperty(settingKeyBindingPair.GetTempValue().Binding1));
		AddDisposable(TempBindingValue2 = (from pair in settingKeyBindingPair.ObserveTempValue()
			select pair.Binding2).ToReadOnlyReactiveProperty(settingKeyBindingPair.GetTempValue().Binding2));
	}

	public void OpenBindingDialogVM(int bindingIndex)
	{
		EventBus.RaiseEvent(delegate(IKeyBindingSetupDialogHandler h)
		{
			h.OpenKeyBindingSetupDialog(m_UISettingsEntityKeybind, bindingIndex);
		});
	}
}
