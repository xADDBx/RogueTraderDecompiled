using System;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Menu;

public class SettingsMenuEntityVM : SelectionGroupEntityVM, ILocalizationHandler, ISubscriber
{
	public readonly StringReactiveProperty Title = new StringReactiveProperty();

	private Action<UISettingsManager.SettingsScreen> m_ConfirmAction;

	private readonly LocalizedString m_Title;

	public UISettingsManager.SettingsScreen SettingsScreenType { get; }

	public SettingsMenuEntityVM(LocalizedString title, UISettingsManager.SettingsScreen screenType, Action<UISettingsManager.SettingsScreen> confirmAction)
		: base(allowSwitchOff: false)
	{
		m_Title = title;
		SettingsScreenType = screenType;
		m_ConfirmAction = confirmAction;
		UpdateTitle();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void Confirm()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_ConfirmAction?.Invoke(SettingsScreenType);
	}

	protected override void DisposeImplementation()
	{
		m_ConfirmAction = null;
	}

	public void UpdateTitle()
	{
		Title.Value = m_Title.Text;
	}

	public void HandleLanguageChanged()
	{
		UpdateTitle();
	}
}
