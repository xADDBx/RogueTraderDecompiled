using System.Collections;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class InitiativeTrackerVerticalConsoleView : InitiativeTrackerVerticalView, IShowInspectChanged, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	protected bool m_ChangeHintsWidgetPosition;

	[SerializeField]
	[ConditionalShow("m_ChangeHintsWidgetPosition")]
	private Vector3 m_PlayerTurnHintsWidgetPosition;

	[SerializeField]
	[ConditionalShow("m_ChangeHintsWidgetPosition")]
	private Vector3 m_NotPlayerTurnHintsWidgetPosition;

	[SerializeField]
	private ConsoleHint m_ActivateHint;

	[SerializeField]
	private ConsoleHint m_DeactivateHint;

	[SerializeField]
	private RectTransform m_TooltipUpperRightPosition;

	[Header("AnimationSettings")]
	[SerializeField]
	private float m_FocusUnitDelay = 0.1f;

	[SerializeField]
	private RectTransform m_TrackerContainer;

	[SerializeField]
	private Vector2 m_TrackerContainerDeltaOnSelect;

	[SerializeField]
	private float m_ContainerAnimationTime = 0.1f;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_ConsoleNavigation;

	private GridConsoleNavigationBehaviour m_VerticalListNavigation;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	private readonly ReactiveProperty<SurfaceCombatUnitOrderView> m_SelectedUnit = new ReactiveProperty<SurfaceCombatUnitOrderView>();

	private readonly BoolReactiveProperty m_HasSelectedUnit = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasSquad = new BoolReactiveProperty();

	private IShowInspectChanged m_ShowInspectChangedImplementation;

	private Coroutine m_FocusUnitCoroutine;

	private WaitForSeconds m_CachedDelay;

	private Vector2 m_InitialPosition;

	private Tweener m_ActiveStateTweener;

	private SurfaceCombatUnitOrderVerticalConsoleView CurrentUnit => m_CurrentUnit as SurfaceCombatUnitOrderVerticalConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CachedDelay = new WaitForSeconds(m_FocusUnitDelay);
		m_InitialPosition = m_TrackerContainer.anchoredPosition;
	}

	protected override void DestroyViewImplementation()
	{
		if (m_FocusUnitCoroutine != null)
		{
			StopCoroutine(m_FocusUnitCoroutine);
			m_FocusUnitCoroutine = null;
		}
		Deactivate();
		base.DestroyViewImplementation();
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(m_ActivateHint.Bind(inputLayer.AddButton(delegate
		{
			Activate();
		}, 18, InputActionEventType.ButtonJustReleased)));
	}

	protected override void PrepareInitiativeTracker()
	{
		base.PrepareInitiativeTracker();
		AddDisposable(m_SelectedUnit.Subscribe(delegate(SurfaceCombatUnitOrderView value)
		{
			m_HasSelectedUnit.Value = value != null;
		}));
		AddDisposable(m_SelectedUnit.Subscribe(delegate(SurfaceCombatUnitOrderView value)
		{
			m_HasSquad.Value = (object)value != null && value.IsBinded && value.HasSquad;
		}));
		AddDisposable(m_ConsoleNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(m_VerticalListNavigation = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_ConsoleNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "InitiativeTracker",
			CursorEnabled = false
		});
		AddDisposable(m_DeactivateHint.Bind(m_InputLayer.AddButton(delegate
		{
			Deactivate();
		}, 18, base.ViewModel.ConsoleActive, InputActionEventType.ButtonJustReleased)));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Deactivate();
		}, 9, base.ViewModel.ConsoleActive), UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowInspect();
		}, 10, base.ViewModel.ConsoleActive.CombineLatest(m_HasSelectedUnit, (bool x, bool y) => x && y).ToReactiveProperty()), UIStrings.Instance.MainMenu.Inspect, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ToggleSquad();
		}, 11, base.ViewModel.ConsoleActive.CombineLatest(m_HasSquad, (bool x, bool y) => x && y).ToReactiveProperty()), UIStrings.Instance.Inspect.ToggleSquad, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleNavigation.DeepestFocusAsObservable.Subscribe(OnFocusedChanged));
	}

	public void Activate()
	{
		TooltipHelper.HideTooltip();
		DOTween.Kill(m_TrackerContainer);
		m_TrackerContainer.DOAnchorPos(m_InitialPosition + m_TrackerContainerDeltaOnSelect, m_ContainerAnimationTime);
		m_Disposable.Clear();
		m_ConsoleNavigation.Clear();
		m_VerticalListNavigation.Clear();
		base.ViewModel.ConsoleActive.Value = true;
		VirtualList.FillNavigationBehaviour(m_VerticalListNavigation);
		m_VerticalListNavigation.FocusOnLastValidEntity();
		m_VerticalListNavigation.UnFocusCurrentEntity();
		m_ConsoleNavigation.AddEntityVertical(m_VerticalListNavigation);
		m_ConsoleNavigation.AddEntityVertical(CurrentUnit);
		m_ConsoleNavigation.FocusOnLastValidEntity();
		m_Disposable.Add(GamePad.Instance.PushLayer(m_InputLayer));
	}

	public void Deactivate()
	{
		TooltipHelper.HideTooltip();
		DOTween.Kill(m_TrackerContainer);
		m_TrackerContainer.DOAnchorPos(m_InitialPosition, m_ContainerAnimationTime);
		base.ViewModel.ConsoleActive.Value = false;
		m_ConsoleNavigation.UnFocusCurrentEntity();
		VirtualList.FillNavigationBehaviour(null);
		m_CurrentUnit.Or(null)?.OnPointerClick();
		m_Disposable?.Clear();
	}

	private void OnFocusedChanged(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		if (entity is SurfaceCombatUnitOrderVerticalConsoleView surfaceCombatUnitOrderVerticalConsoleView)
		{
			m_SelectedUnit.Value = surfaceCombatUnitOrderVerticalConsoleView;
			VirtualList.EntitySelected(surfaceCombatUnitOrderVerticalConsoleView);
			if (m_FocusUnitCoroutine != null)
			{
				StopCoroutine(m_FocusUnitCoroutine);
			}
			m_FocusUnitCoroutine = StartCoroutine(FocusSelectedCo());
		}
	}

	private void ShowInspect()
	{
		if (m_SelectedUnit.Value is SurfaceCombatUnitOrderVerticalConsoleView surfaceCombatUnitOrderVerticalConsoleView)
		{
			surfaceCombatUnitOrderVerticalConsoleView.ShowInspect();
		}
	}

	private void ToggleSquad()
	{
		if (m_SelectedUnit.Value is SurfaceCombatUnitOrderVerticalConsoleView surfaceCombatUnitOrderVerticalConsoleView)
		{
			surfaceCombatUnitOrderVerticalConsoleView.ToggleSquad();
		}
	}

	private void ShowInspectTooltip()
	{
		if (m_SelectedUnit.Value is SurfaceCombatUnitOrderVerticalConsoleView surfaceCombatUnitOrderVerticalConsoleView)
		{
			surfaceCombatUnitOrderVerticalConsoleView.ShowTooltip(m_TooltipUpperRightPosition);
		}
	}

	public void HandleShowInspect(bool state)
	{
		if (state)
		{
			return;
		}
		IConsoleEntity deepestNestedFocus = m_ConsoleNavigation.DeepestNestedFocus;
		SurfaceCombatUnitOrderVerticalConsoleView view = deepestNestedFocus as SurfaceCombatUnitOrderVerticalConsoleView;
		if ((object)view != null)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				view.ShowTooltip(m_TooltipUpperRightPosition);
			}, 1);
		}
	}

	private IEnumerator FocusSelectedCo()
	{
		yield return m_CachedDelay;
		m_SelectedUnit.Value.OnPointerClick();
		yield return null;
		yield return null;
		yield return null;
		ShowInspectTooltip();
		m_FocusUnitCoroutine = null;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (m_ChangeHintsWidgetPosition && isTurnBased)
		{
			bool isPlayerTurn = Game.Instance.TurnController.IsPlayerTurn;
			m_ConsoleHintsWidget.gameObject.transform.localPosition = (isPlayerTurn ? m_PlayerTurnHintsWidgetPosition : m_NotPlayerTurnHintsWidgetPosition);
		}
	}
}
