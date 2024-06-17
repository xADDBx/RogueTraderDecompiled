using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NavigatorResource;
using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow;
using Kingmaker.Code.UI.MVVM.View.SectorMap.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Kingmaker.Visual.SectorMap;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class SectorMapPCView : SectorMapBaseView
{
	[Header("BottomHud")]
	[SerializeField]
	private SectorMapBottomHudPCView m_SectorMapBottomHudPCView;

	[Header("InformationWindow")]
	[SerializeField]
	private SpaceSystemInformationWindowPCView m_SpaceSystemInformationWindowPCView;

	[Header("AllSystemsWindow")]
	[SerializeField]
	private OwlcatMultiButton m_ShowHideAllSystemsInformationWindowButton;

	[SerializeField]
	private AllSystemsInformationWindowPCView m_AllSystemsInformationWindowPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SectorMapView.Instance.NeedShowSectorMapMouseLines = true;
		m_SectorMapBottomHudPCView.Bind(base.ViewModel.SectorMapBottomHudVM);
		m_SpaceSystemInformationWindowPCView.Bind(base.ViewModel.SpaceSystemInformationWindowVM);
		m_AllSystemsInformationWindowPCView.Bind(base.ViewModel.AllSystemsInformationWindowVM);
		UISounds.Instance.SetClickAndHoverSound(m_ShowHideAllSystemsInformationWindowButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(m_ShowHideAllSystemsInformationWindowButton.gameObject.SetActive));
		AddDisposable(m_ShowHideAllSystemsInformationWindowButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowHideAllSystemsInformation(!base.ViewModel.AllSystemInformationIsShowed.Value);
		}));
		AddDisposable(m_ShowHideAllSystemsInformationWindowButton.SetHint(UIStrings.Instance.GlobalMap.AllSystems));
		AddDisposable(base.ViewModel.AllSystemInformationIsShowed.CombineLatest(base.ViewModel.SystemInformationIsShowed, (bool allSystemsInfoShowed, bool systemsInfoShowed) => new { allSystemsInfoShowed, systemsInfoShowed }).Subscribe(value =>
		{
			m_ShowHideAllSystemsInformationWindowButton.SetActiveLayer((value.allSystemsInfoShowed || value.systemsInfoShowed) ? "Active" : "NotActive");
		}));
		AddDisposable(base.ViewModel.IsScanning.CombineLatest(base.ViewModel.IsTraveling, base.ViewModel.IsDialogActive, (bool isScanning, bool isTraveling, bool isDialogActive) => isScanning || isTraveling || isDialogActive).Subscribe(delegate(bool isLocked)
		{
			m_ShowHideAllSystemsInformationWindowButton.Interactable = !isLocked;
		}));
	}
}
