using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;

public class FirstLaunchAccessiabilityPageVM : FirstLaunchSettingsPageVM, ISettingsDescriptionUIHandler, ISubscriber
{
	public readonly SettingsEntitySliderVM FontSize;

	public readonly SettingsEntitySliderVM Protanopia;

	public readonly SettingsEntitySliderVM Deuteranopia;

	public readonly SettingsEntitySliderVM Tritanopia;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public FirstLaunchAccessiabilityPageVM()
	{
		FontSize = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.FontSize, SettingsEntitySliderVM.EntitySliderType.FontSizeIndex);
		Protanopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Protanopia);
		Deuteranopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Deuteranopia);
		Tritanopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Tritanopia);
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
		AddDisposable(EventBus.Subscribe(this));
		HandleShowSettingsDescription(FontSize.UISettingsEntity);
	}

	protected override void DisposeImplementation()
	{
		HandleHideSettingsDescription();
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
		SetupTooltipTemplate(null);
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}
}
