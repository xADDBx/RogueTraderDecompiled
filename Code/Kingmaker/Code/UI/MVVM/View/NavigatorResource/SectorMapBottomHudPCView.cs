using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NavigatorResource.Base;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NavigatorResource;

public class SectorMapBottomHudPCView : SectorMapBottomHudBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_CenterOnShipButton;

	[SerializeField]
	private OwlcatButton m_ExitToShipButton;

	[SerializeField]
	private OwlcatButton m_ShipCustomizationButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CenterOnShipButton.SetHint(CenterOnShipHintText));
		AddDisposable(m_CenterOnShipButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetCameraOnVoidShip();
		}));
		UISounds.Instance.SetClickAndHoverSound(m_ExitToShipButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_ExitToShipButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ExitToShip();
		}));
		AddDisposable(m_ExitToShipButton.SetHint(UIStrings.Instance.SpaceCombatTexts.BackToShipBridge));
		UISounds.Instance.SetClickAndHoverSound(m_ShipCustomizationButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_ShipCustomizationButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenShipCustomization();
		}));
		AddDisposable(m_ShipCustomizationButton.SetHint(UIStrings.Instance.MainMenu.ShipCustomization));
		AddDisposable(base.ViewModel.HasAccessStarshipInventory.Subscribe(delegate(bool value)
		{
			m_ShipCustomizationButton.SetInteractable(value && base.ViewModel.IsExitAvailable.Value);
		}));
		AddDisposable(base.ViewModel.IsExitAvailable.Subscribe(delegate(bool value)
		{
			m_ExitToShipButton.SetInteractable(value);
			m_ShipCustomizationButton.SetInteractable(value && base.ViewModel.HasAccessStarshipInventory.Value);
		}));
		UISounds.Instance.SetHoverSound(m_ScanButton, UISounds.ButtonSoundsEnum.NoSound);
		AddDisposable(m_ScanButton.OnHoverAsObservable().Subscribe(base.NeedScanHoverEffect));
		AddDisposable(m_ScanButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleScanSystem();
		}));
		AddDisposable(base.ViewModel.IsScanning.CombineLatest(base.ViewModel.IsTraveling, base.ViewModel.IsDialogActive, (bool isScanning, bool isTraveling, bool isDialogActive) => isScanning || isTraveling || isDialogActive).Subscribe(delegate(bool isLocked)
		{
			m_ScanButton.Interactable = !isLocked;
		}));
	}

	protected override void OnUpdateHandler()
	{
		if (!(m_CenterOnShipButton == null))
		{
			Game instance = Game.Instance;
			if (instance != null && (instance.SectorMapController?.CurrentStarSystem?.Position).HasValue)
			{
				m_CenterOnShipButton.Interactable = !IsPointOnScreen(Game.Instance.SectorMapController.CurrentStarSystem.Position);
			}
		}
	}

	protected override void NeedScanEffect(bool state)
	{
		m_ScanButton.SetInteractable(state);
		base.NeedScanEffect(state);
	}

	private void HandleScanSystem()
	{
		if (base.ViewModel.IsScanAvailable.Value && !base.ViewModel.IsTraveling.Value && !base.ViewModel.IsDialogActive.Value && !base.ViewModel.IsScanning.Value)
		{
			m_ScanButton.SetInteractable(state: false);
			base.ViewModel.ScanSystem();
			SectorMapOvertipsVM.Instance.ClosePopups();
			UISounds.Instance.Sounds.SpaceExploration.KoronusRouteButtonUnHover.Play();
		}
	}
}
