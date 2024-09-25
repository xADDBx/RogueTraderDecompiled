using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingsEntityHeaderConsoleView : VirtualListElementViewBase<SettingsEntityHeaderVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Tittle;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	[SerializeField]
	private OwlcatMultiButton m_SelectableMultiButton;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void BindViewImplementation()
	{
		m_Tittle.text = base.ViewModel.Tittle;
		AddDisposable(base.ViewModel.LanguageChanged.Subscribe(delegate
		{
			m_Tittle.text = base.ViewModel.Tittle;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, base.ViewModel.Tittle);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
		m_SelectableMultiButton.SetFocus(value);
		m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return false;
	}
}
