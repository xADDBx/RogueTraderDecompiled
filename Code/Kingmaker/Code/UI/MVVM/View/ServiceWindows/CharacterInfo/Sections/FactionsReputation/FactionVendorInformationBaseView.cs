using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class FactionVendorInformationBaseView : ViewBase<FactionVendorInformationVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_VendorLocation;

	[SerializeField]
	protected TextMeshProUGUI m_VendorName;

	[SerializeField]
	protected OwlcatButton m_MainButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_VendorLocation.text = base.ViewModel.Location;
		m_VendorName.text = base.ViewModel.Name;
		if (base.ViewModel.Vendor != null && (bool)m_MainButton)
		{
			m_MainButton.gameObject.SetActive(value: true);
			AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				StartTrade();
			}));
		}
		else
		{
			m_MainButton.gameObject.SetActive(value: false);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((FactionVendorInformationVM)vm);
	}

	public void StartTrade()
	{
		base.ViewModel.StartTrade();
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is FactionVendorInformationVM;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_MainButton.IsValid();
	}
}
