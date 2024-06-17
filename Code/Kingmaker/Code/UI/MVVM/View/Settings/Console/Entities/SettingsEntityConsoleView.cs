using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public abstract class SettingsEntityConsoleView<TSettingsEntityVM> : VirtualListElementViewBase<TSettingsEntityVM>, IWidgetView where TSettingsEntityVM : SettingsEntityVM
{
	[SerializeField]
	private GameObject m_SetConnector;

	[SerializeField]
	private GameObject m_SetConnectorIAmSet;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		UpdateLocalization();
		if (m_SetConnector != null)
		{
			m_SetConnector.SetActive(base.ViewModel.IsConnector || base.ViewModel.IsSet);
		}
		if (m_SetConnectorIAmSet != null)
		{
			m_SetConnectorIAmSet.SetActive(base.ViewModel.IsSet);
		}
		AddDisposable(base.ViewModel.LanguageChanged.Subscribe(delegate
		{
			UpdateLocalization();
		}));
	}

	protected virtual void UpdateLocalization()
	{
		if (m_Title != null)
		{
			m_Title.text = base.ViewModel.Title;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TSettingsEntityVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TSettingsEntityVM;
	}
}
