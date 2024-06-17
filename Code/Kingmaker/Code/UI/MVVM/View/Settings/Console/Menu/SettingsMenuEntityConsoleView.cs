using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;

public class SettingsMenuEntityConsoleView : ViewBase<SettingsMenuEntityVM>, IConsoleNavigationEntity, IConsoleEntity, IWidgetView
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	public UISettingsManager.SettingsScreen SettingsScreenType => base.ViewModel.SettingsScreenType;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string t)
		{
			m_Title.text = t;
		}));
		AddDisposable(base.ViewModel.IsSelected.Subscribe(SetFocus));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return true;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((SettingsMenuEntityVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SettingsMenuEntityVM;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.Confirm();
	}
}
