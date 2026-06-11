using Kingmaker.UI.MVVM.View.Exploration.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationPCView : ExplorationBaseView
{
	[Header("Components")]
	[SerializeField]
	protected ExplorationResourceListPCView m_ExplorationResourceListPCView;

	[SerializeField]
	private ExplorationColonyStatsWrapperPCView m_ExplorationColonyStatsWrapperPCView;

	[SerializeField]
	private ExplorationColonyTraitsWrapperPCView m_ExplorationColonyTraitsWrapperPCView;

	[SerializeField]
	private ExplorationColonyEventsWrapperPCView m_ExplorationColonyEventsWrapperPCView;

	[SerializeField]
	private ExplorationPointOfInterestListPCView m_ExplorationPointOfInterestListView;

	[SerializeField]
	private ExplorationColonyRewardsWrapperPCView m_ExplorationColonyRewardsWrapperPCView;

	[SerializeField]
	private GameObject[] m_ObjectsToHideForDialog;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private ExplorationScanButtonWrapperPCView m_ExplorationScanButtonWrapperPCView;

	[Header("Projects")]
	[SerializeField]
	private ExplorationColonyProjectsWrapperPCView m_ExplorationColonyProjectsWrapperPCView;

	[SerializeField]
	private ExplorationColonyProjectsButtonWrapperPCView m_ExplorationColonyProjectsButtonWrapperPCView;

	[SerializeField]
	private ExplorationColonyProjectsBuiltListWrapperPCView m_ExplorationColonyProjectsBuiltListWrapperPCView;

	[Header("Bark Part")]
	[SerializeField]
	private ExplorationSpaceBarksHolderPCView m_ExplorationSpaceBarksHolderPCView;

	[Header("Resources")]
	[SerializeField]
	private ExplorationSpaceResourcesWrapperPCView m_ExplorationSpaceResourcesWrapperPCView;

	protected override void InitializeImpl()
	{
		m_ExplorationScanButtonWrapperPCView.Initialize();
		m_ExplorationColonyTraitsWrapperPCView.Initialize();
		m_ExplorationColonyStatsWrapperPCView.Initialize();
		m_ExplorationColonyEventsWrapperPCView.Initialize();
		m_ExplorationColonyRewardsWrapperPCView.Initialize();
		m_ExplorationColonyProjectsWrapperPCView.Initialize();
		m_ExplorationColonyProjectsButtonWrapperPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ExplorationResourceListPCView.Bind(base.ViewModel.ExplorationResourceListVM);
		m_ExplorationSpaceResourcesWrapperPCView.Bind(base.ViewModel.ExplorationSpaceResourcesWrapperVM);
		m_ExplorationScanButtonWrapperPCView.Bind(base.ViewModel.ExplorationScanButtonWrapperVM);
		m_ExplorationColonyStatsWrapperPCView.Bind(base.ViewModel.ExplorationColonyStatsWrapperVM);
		m_ExplorationColonyTraitsWrapperPCView.Bind(base.ViewModel.ExplorationColonyTraitsWrapperVM);
		m_ExplorationColonyEventsWrapperPCView.Bind(base.ViewModel.ExplorationColonyEventsWrapperVM);
		m_ExplorationColonyRewardsWrapperPCView.Bind(base.ViewModel.ExplorationColonyRewardsWrapperVM);
		m_ExplorationColonyProjectsWrapperPCView.Bind(base.ViewModel.ExplorationColonyProjectsWrapperVM);
		m_ExplorationColonyProjectsButtonWrapperPCView.Bind(base.ViewModel.ExplorationColonyProjectsButtonWrapperVM);
		m_ExplorationColonyProjectsBuiltListWrapperPCView.Bind(base.ViewModel.ExplorationColonyProjectsBuiltListWrapperVM);
		m_ExplorationPointOfInterestListView.Bind(base.ViewModel.ExplorationPointOfInterestListVM);
		m_ExplorationSpaceBarksHolderPCView.Bind(base.ViewModel.ExplorationSpaceBarksHolderVM);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.OnCloseClickDelegate));
		AddDisposable(m_CloseButton.OnConfirmClickAsObservable().Subscribe(base.OnCloseClickDelegate));
	}

	protected override void BuildNavigationImpl()
	{
	}

	protected override void ScanPlanetImpl()
	{
		SetButtonsInteractable(value: false);
	}

	protected override void ScanAnimationImpl()
	{
		SetButtonsInteractable(value: true);
	}

	protected override void ClearScanProgressImpl()
	{
		SetButtonsInteractable(value: true);
	}

	protected override void HandleScanStarSystemObjectImpl()
	{
		SetButtonsInteractable(value: true);
	}

	private void SetButtonsInteractable(bool value)
	{
		m_CloseButton.Interactable = value;
		m_ExplorationScanButtonWrapperPCView.SetButtonInteractable(value);
	}

	protected override void SetLockUIForDialogImpl(bool value)
	{
		GameObject[] objectsToHideForDialog = m_ObjectsToHideForDialog;
		for (int i = 0; i < objectsToHideForDialog.Length; i++)
		{
			objectsToHideForDialog[i].SetActive(!value);
		}
	}
}
