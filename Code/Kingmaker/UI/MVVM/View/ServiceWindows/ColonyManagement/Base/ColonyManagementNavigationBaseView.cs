using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;

public class ColonyManagementNavigationBaseView<TColonyManagementNavigationElementView> : ViewBase<ColonyManagementNavigationVM> where TColonyManagementNavigationElementView : ColonyManagementNavigationElementBaseView
{
	[SerializeField]
	private List<TColonyManagementNavigationElementView> m_ColoniesTabs;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UpdateNavigationElementsCommand.Subscribe(UpdateColoniesTabs));
		UpdateColoniesTabs();
	}

	protected override void DestroyViewImplementation()
	{
		ClearColoniesTabs();
	}

	public void SetInteractable(bool interactable)
	{
		foreach (TColonyManagementNavigationElementView coloniesTab in m_ColoniesTabs)
		{
			coloniesTab.SetButtonInteractable(interactable);
		}
	}

	public void SelectNextColony()
	{
		base.ViewModel.SelectNextColony();
	}

	public void SelectPrevColony()
	{
		base.ViewModel.SelectPrevColony();
	}

	public int GetActiveColonyIndex()
	{
		return base.ViewModel.GetActiveColonyIndex();
	}

	private void UpdateColoniesTabs()
	{
		ClearColoniesTabs();
		for (int i = 0; i < base.ViewModel.NavigationElements.Count; i++)
		{
			if (i >= m_ColoniesTabs.Count)
			{
				PFLog.UI.Error("ColonyManagementNavigationPCView.UpdateColoniesTabs - NavigationElements count is more than slots count!");
				break;
			}
			TColonyManagementNavigationElementView val = m_ColoniesTabs[i];
			val.Bind(base.ViewModel.NavigationElements[i]);
			val.gameObject.SetActive(value: true);
		}
	}

	private void ClearColoniesTabs()
	{
		foreach (TColonyManagementNavigationElementView coloniesTab in m_ColoniesTabs)
		{
			coloniesTab.Unbind();
			coloniesTab.gameObject.SetActive(value: false);
		}
	}
}
