using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class ZoneExitPCView : ZoneExitBaseView
{
	[Header("Objects")]
	[SerializeField]
	private GameObject m_ExitToWarpObject;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_ExitToShipButton;

	[SerializeField]
	private OwlcatButton m_ExitToWarpButton;

	[SerializeField]
	private OwlcatButton m_ShipCustomizationButton;

	[Header("Button Effects")]
	[SerializeField]
	private OwlcatMultiButton m_ExitToWarpBackgroundEffects;

	[SerializeField]
	private RectTransform m_HoverCircle;

	[SerializeField]
	private Image m_HoverLights;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_ExitToShipButton, UISounds.ButtonSoundsEnum.ExitToWarpSound);
		AddDisposable(m_ExitToShipButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ExitToShip();
		}));
		AddDisposable(m_ExitToShipButton.SetHint(UIStrings.Instance.SpaceCombatTexts.BackToShipBridge));
		UISounds.Instance.SetClickAndHoverSound(m_ShipCustomizationButton, UISounds.ButtonSoundsEnum.ExitToWarpSound);
		AddDisposable(m_ShipCustomizationButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenShipCustomization();
		}));
		AddDisposable(m_ShipCustomizationButton.SetHint(UIStrings.Instance.MainMenu.ShipCustomization));
		AddDisposable(base.ViewModel.HasAccessStarshipInventory.Subscribe(delegate(bool value)
		{
			m_ShipCustomizationButton.SetInteractable(value && base.ViewModel.IsExitAvailable.Value);
		}));
		UISounds.Instance.SetClickAndHoverSound(m_ExitToWarpButton, UISounds.ButtonSoundsEnum.ExitToWarpSound);
		AddDisposable(m_ExitToWarpButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			UISounds.Instance.Sounds.SpaceExploration.ExitToKoronusMap.Play();
			base.ViewModel.ExitToWarp();
		}));
		AddDisposable(m_ExitToWarpButton.SetHint(UIStrings.Instance.SpaceCombatTexts.KoronusExpanse));
		AddDisposable(base.ViewModel.IsWarpJumpAvailable.Subscribe(m_ExitToWarpObject.SetActive));
		AddDisposable(base.ViewModel.IsExitAvailable.Subscribe(delegate(bool value)
		{
			m_ExitToShipButton.SetInteractable(value);
			m_ShipCustomizationButton.SetInteractable(value && base.ViewModel.HasAccessStarshipInventory.Value);
			m_ExitToWarpButton.SetInteractable(value);
		}));
		m_ExitToWarpBackgroundEffects.SetActiveLayer(base.ViewModel.PushedWarpJumpBefore ? "Default" : "ActiveExit");
		if (!base.ViewModel.PushedWarpJumpBefore)
		{
			AddDisposable(m_ExitToWarpButton.OnHoverAsObservable().Subscribe(NeedScanHoverEffect));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void NeedScanHoverEffect(bool state)
	{
		if (!base.ViewModel.PushedWarpJumpBefore)
		{
			float num = (state ? 1f : 0.7f);
			m_HoverCircle.DOScale(new Vector2(num, num), 0.3f);
			m_HoverLights.DOFade(state ? 1f : 0f, 0.3f);
		}
	}
}
