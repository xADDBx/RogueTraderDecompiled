using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Ship;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Ship;

public class CharGenShipPhaseDetailedConsoleView : CharGenShipPhaseDetailedView
{
	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	[SerializeField]
	private CharGenChangeNameMessageBoxConsoleView m_MessageBoxConsoleView;

	private GridConsoleNavigationBehaviour m_Navigation;

	private GridConsoleNavigationBehaviour m_MenuNavigation;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSwitchNavigation = new BoolReactiveProperty();

	private bool m_HasTooltip;

	public override void Initialize()
	{
		base.Initialize();
		m_MessageBoxConsoleView.Initialize();
		m_SecondaryInfoViewContainer.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		AddDisposable(base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxConsoleView.Bind));
		AddDisposable(m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		AddDisposable(m_MenuNavigation = new GridConsoleNavigationBehaviour());
		m_MenuNavigation.SetEntitiesVertical(m_CharGenShipPhaseSelectorView.GetNavigationEntities());
		AddDisposable(m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation));
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
		AddDisposable(m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, m_CanConfirm);
		AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Select));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_CanSwitchNavigation);
		AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Back));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			OnFunc02Click();
		}, 11, base.ViewModel.IsCompletedAndAvailable);
		AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CharGen.EditName));
		AddDisposable(inputBindStruct4);
		AddInputToPaperHints(ref inputLayer);
	}

	private void AddInputToPaperHints(ref InputLayer inputLayer)
	{
		if (PaperHints != null)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoPrevPage();
				RefreshMenuFocus();
			}, 12, base.ViewModel.CurrentPageIsFirst.CombineLatest(m_ActivePhaseNavigation, (bool isFirst, ActivePhaseNavigation navigation) => !isFirst && navigation == ActivePhaseNavigation.Menu).ToReactiveProperty());
			AddDisposable(PaperHints.PageUpHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoNextPage();
				RefreshMenuFocus();
			}, 13, base.ViewModel.CurrentPageIsLast.CombineLatest(m_ActivePhaseNavigation, (bool isLast, ActivePhaseNavigation navigation) => !isLast && navigation == ActivePhaseNavigation.Menu).ToReactiveProperty());
			AddDisposable(PaperHints.PageDownHint.Bind(inputBindStruct2));
			AddDisposable(inputBindStruct2);
		}
	}

	private void RefreshMenuFocus()
	{
		m_MenuNavigation.FocusOnEntityManual(m_CharGenShipPhaseSelectorView.GetSelectedEntity());
	}

	private void SetMenuNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_MenuNavigation);
		m_MenuNavigation.FocusOnEntityManual(m_CharGenShipPhaseSelectorView.GetSelectedEntity());
		m_Navigation.FocusOnEntityManual(m_MenuNavigation);
	}

	private void SetContentNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		if (activeNavigation == ActivePhaseNavigation.Menu)
		{
			SetMenuNavigation();
		}
		else
		{
			SetContentNavigation();
		}
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.Content;
		m_CanSwitchNavigation.Value = activeNavigation == ActivePhaseNavigation.Menu;
		CanGoNextOnConfirm.Value = activeNavigation == ActivePhaseNavigation.Menu;
	}

	private void SwitchNavigation()
	{
		m_ActivePhaseNavigation.Value = ((m_ActivePhaseNavigation.Value != ActivePhaseNavigation.Content) ? ActivePhaseNavigation.Content : ActivePhaseNavigation.Menu);
	}

	private void Scroll(InputActionEventData data, float y)
	{
		InfoSectionView infoSectionView = ((m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu) ? m_InfoView : m_SecondaryInfoView);
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !infoSectionView.ScrollbarOnTop) && (!(y < 0f) || !infoSectionView.ScrollbarOnBottom))
		{
			infoSectionView.Scroll(y);
		}
	}

	private void OnConfirmClick()
	{
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
		if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content && m_HasTooltip)
		{
			TooltipHelper.ShowInfo(base.ViewModel.SecondaryInfoVM.CurrentTooltip);
		}
	}

	private void OnDeclineClick()
	{
		SwitchNavigation();
	}

	protected virtual void OnFocusChanged(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_CanConfirm.Value = (m_HasTooltip = tooltipBaseTemplate != null);
		if (tooltipBaseTemplate == null)
		{
			base.ViewModel.SecondaryInfoVM.SetTemplate(null);
		}
		else
		{
			base.ViewModel.SecondaryInfoVM.SetTemplate(tooltipBaseTemplate);
		}
	}

	private void OnFunc02Click()
	{
		base.ViewModel.ShowChangeNameMessageBox();
	}

	public override bool PressConfirmOnPhase()
	{
		return true;
	}
}
