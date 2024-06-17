using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.UI.MVVM.View.Colonization;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;

public class ColonyManagementNavigationElementBaseView : ViewBase<ColonyManagementNavigationElementVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private ColonyEventNotificatorListPCView m_ColonyEventNotificatorListPCView;

	protected override void BindViewImplementation()
	{
		m_Label.text = base.ViewModel.Title;
		AddDisposable(m_MultiButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectPage();
		}));
		m_ColonyEventNotificatorListPCView.Bind(base.ViewModel.ColonyEventsVM);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetButtonInteractable(bool interactable)
	{
		m_MultiButton.Interactable = interactable;
	}
}
