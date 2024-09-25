using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.UniRxExtensions;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public abstract class SettingsEntityWithValueVM : SettingsEntityVM
{
	public readonly ReadOnlyReactiveProperty<bool> IsChanged;

	private readonly IUISettingsEntityWithValueBase m_UISettingsEntity;

	public readonly ReactiveProperty<bool> ModificationAllowed = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> ModificationAllowedReason = new ReactiveProperty<string>();

	public readonly bool IsOdd;

	public readonly bool HideMarkImage;

	public SettingsEntityWithValueVM(IUISettingsEntityWithValueBase uiSettingsEntity, bool hideMarkImage)
		: base(uiSettingsEntity)
	{
		m_UISettingsEntity = uiSettingsEntity;
		HideMarkImage = hideMarkImage;
		AddDisposable(IsChanged = m_UISettingsEntity.SettingsEntity.ObserveTempValueIsConfirmed().Not().ToReadOnlyReactiveProperty());
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			ModificationAllowed.Value = m_UISettingsEntity.ModificationAllowed;
		}));
		ModificationAllowedReason.Value = m_UISettingsEntity.ModificationAllowedReason;
		IsOdd = false;
	}

	public void ResetToDefault()
	{
		m_UISettingsEntity.SettingsEntity.ResetToDefault(confirmChanges: false);
	}
}
