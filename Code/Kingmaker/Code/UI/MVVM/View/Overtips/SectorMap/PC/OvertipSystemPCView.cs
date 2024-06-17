using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup;
using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.PC;

public class OvertipSystemPCView : OvertipSystemView
{
	[Header("Buttons")]
	[SerializeField]
	protected OwlcatButton m_InfoButton;

	[SerializeField]
	protected OwlcatButton m_TravelButton;

	[SerializeField]
	private OwlcatButton m_VisitButton;

	[SerializeField]
	private TextMeshProUGUI m_VisitButtonLabel;

	[SerializeField]
	private OwlcatButton m_CoopPingForAnotherPlayersButton;

	[FormerlySerializedAs("m_SpaceSystemPopupPCView")]
	[Header("SpaceSystemPopup")]
	[SerializeField]
	private SpaceSystemNavigationButtonsPCView m_SpaceSystemNavigationButtonsPCView;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private List<Vector2> m_PriorityPivots;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VisitButtonLabel.text = UIStrings.Instance.ColonizationTexts.ColonyManagementVisitColonyButton;
		AddDisposable(base.ViewModel.IsVisitAvailable.AsObservable().Subscribe(delegate(bool state)
		{
			m_InfoButton.gameObject.SetActive(state);
			m_VisitButton.transform.parent.gameObject.SetActive(state);
		}));
		AddDisposable(base.ViewModel.IsTravelAvailable.AsObservable().Subscribe(m_TravelButton.gameObject.SetActive));
		AddDisposable(m_InfoButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			if (SpaceSystemInformationWindowVM.Instance.ShowSystemWindow.Value)
			{
				base.ViewModel.ChangeInformationInWindow();
			}
			else
			{
				HandleVisitDialogButtonClick();
			}
		}));
		AddDisposable(m_TravelButton.OnLeftClick.AsObservable().Subscribe(delegate
		{
			if (!base.ViewModel.CheckPingCoop())
			{
				base.ViewModel.ChangeInformationInWindow();
				if (base.ViewModel.SpaceSystemNavigationButtonsVM.Value == null)
				{
					base.ViewModel.ShowSpaceSystemPopup();
					ShowHideCircleBackground(state: true);
				}
				else
				{
					ClosePopup(withCircle: false);
				}
			}
		}));
		AddDisposable(m_TravelButton.OnLeftDoubleClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.TravelToSystemImmediately();
		}));
		AddDisposable(m_TravelButton.OnHoverAsObservable().Subscribe(base.ShowHideCircleBackground));
		AddDisposable(m_VisitButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.VisitSystem();
		}));
		AddDisposable(m_InfoButton.SetTooltip(new TooltipTemplateGlobalMapSystem(base.ViewModel.SectorMapObject.View), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace, 0, 0, 0, m_PriorityPivots)));
		AddDisposable(m_TravelButton.SetTooltip(new TooltipTemplateGlobalMapSystem(base.ViewModel.SectorMapObject.View), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace, 0, 0, 0, m_PriorityPivots)));
		AddDisposable(base.ViewModel.SpaceSystemNavigationButtonsVM.Subscribe(m_SpaceSystemNavigationButtonsPCView.Bind));
		AddDisposable(base.ViewModel.IsNotMainCharacter.AsObservable().Subscribe(m_CoopPingForAnotherPlayersButton.gameObject.SetActive));
		AddDisposable(m_CoopPingForAnotherPlayersButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.CheckPingCoop();
		}));
	}

	protected override void LockButtons(bool isLocked)
	{
		if (isLocked)
		{
			base.ViewModel.StopTrade();
		}
		m_InfoButton.Interactable = !isLocked;
		m_TravelButton.Interactable = !isLocked;
		m_VisitButton.Interactable = !isLocked;
	}
}
