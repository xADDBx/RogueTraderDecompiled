using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public abstract class SettingsEntityVM : VirtualListElementVMBase, ILocalizationHandler, ISubscriber
{
	public readonly LocalizedString Title;

	public readonly string Description;

	public readonly bool IsConnector;

	public readonly bool IsSet;

	public readonly IUISettingsEntityBase UISettingsEntity;

	public readonly ReactiveCommand LanguageChanged = new ReactiveCommand();

	public SettingsEntityVM(IUISettingsEntityBase uiSettingsEntity)
	{
		UISettingsEntity = uiSettingsEntity;
		Title = uiSettingsEntity.Description;
		Description = ((!string.IsNullOrWhiteSpace(uiSettingsEntity.TooltipDescription)) ? ((string)uiSettingsEntity.TooltipDescription) : string.Empty);
		IsConnector = uiSettingsEntity.ShowVisualConnection && !uiSettingsEntity.IAmSetHandler;
		IsSet = uiSettingsEntity.ShowVisualConnection && uiSettingsEntity.IAmSetHandler;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleLanguageChanged()
	{
		LanguageChanged.Execute();
	}
}
