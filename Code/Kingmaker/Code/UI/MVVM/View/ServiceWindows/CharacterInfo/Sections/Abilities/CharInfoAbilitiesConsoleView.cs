using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesConsoleView : CharInfoAbilitiesBaseView, ICharInfoComponentConsoleView, ICharInfoComponentView, ICharInfoIgnoreNavigationConsoleView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ActionBarNavigation;

	private GridConsoleNavigationBehaviour m_ScrollRectNavigation;

	private SimpleConsoleNavigationEntity m_ActiveConsoleEntity;

	private SimpleConsoleNavigationEntity m_PassiveConsoleEntity;

	private SurfaceActionBarPartAbilitiesConsoleView m_ActionBarConsoleView;

	private BoolReactiveProperty m_ActionBarActive = new BoolReactiveProperty();

	private SurfaceActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private InputLayer m_ChooseAbilityLayer;

	protected override void BindViewImplementation()
	{
		m_ActionBarConsoleView = m_ActionBarPartAbilitiesView as SurfaceActionBarPartAbilitiesConsoleView;
		CreateNavigation();
		base.BindViewImplementation();
		AddDisposable(m_ActionBarNavigation = m_ActionBarConsoleView.Or(null)?.NavigationBehaviour);
		AddDisposable(m_ActionBarNavigation?.DeepestFocusAsObservable.Subscribe(OnActionBarFocused));
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ActionBarNavigation);
		CreateActionBarManagement();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CurrentAbilitySlot = null;
		m_NavigationBehaviour.RemoveEntity(m_ActionBarNavigation);
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		Action<Ability> onAbilityClick = (ActiveAbilitiesSelected.Value ? new Action<Ability>(OnAbilityClick) : null);
		m_WidgetList.Entries.ForEach(delegate(IWidgetView e)
		{
			(e as CharInfoFeatureGroupConsoleView)?.SetupClickAction(onAbilityClick);
		});
		UpdateNavigation();
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_ScrollRectNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(m_ScrollRectNavigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		m_ActiveConsoleEntity = new SimpleConsoleNavigationEntity(m_ActiveAbilities, null, delegate
		{
			SetActiveAbilitiesState(state: true);
		});
		m_PassiveConsoleEntity = new SimpleConsoleNavigationEntity(m_PassiveAbilities, null, delegate
		{
			SetActiveAbilitiesState(state: false);
		});
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_ActiveConsoleEntity, m_PassiveConsoleEntity);
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ScrollRectNavigation);
		SimpleConsoleNavigationEntity entity = (ActiveAbilitiesSelected.Value ? m_ActiveConsoleEntity : m_PassiveConsoleEntity);
		m_NavigationBehaviour.FocusOnEntityManual(entity);
	}

	private void CreateActionBarManagement()
	{
		m_ChooseAbilityLayer = m_ScrollRectNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "ChooseAbility"
		});
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
			base.ViewModel.ChooseAbilityMode.Value = false;
		}, 9, base.ViewModel.ChooseAbilityMode), UIStrings.Instance.CommonTexts.Cancel));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
		}, 8, base.ViewModel.ChooseAbilityMode), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(base.ViewModel.ChooseAbilityMode.Subscribe(delegate(bool on)
		{
			if (on)
			{
				GamePad.Instance.PushLayer(m_ChooseAbilityLayer);
				m_ScrollRectNavigation.FocusOnFirstValidEntity();
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: true);
				});
			}
			else
			{
				GamePad.Instance.PopLayer(m_ChooseAbilityLayer);
				m_ScrollRectNavigation.UnFocusCurrentEntity();
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: false);
				});
			}
		}));
		AddDisposable(base.ViewModel.ActionBarPartAbilitiesVM.MoveAbilityMode.Subscribe(delegate(bool on)
		{
			if (!on && m_ScrollRectNavigation.Focus.Value != null)
			{
				m_ActionBarNavigation.UnFocusCurrentEntity();
			}
		}));
	}

	private void UpdateNavigation()
	{
		IConsoleEntity value = m_NavigationBehaviour.Focus.Value;
		m_ScrollRectNavigation.Clear();
		if (m_WidgetList.Entries == null)
		{
			return;
		}
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			m_ScrollRectNavigation.AddRow<GridConsoleNavigationBehaviour>((entry as CharInfoFeatureGroupConsoleView)?.GetNavigation());
		}
		m_NavigationBehaviour.FocusOnEntityManual(value);
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(ShowContextMenu, 10, m_ActionBarActive), UIStrings.Instance.ContextMenu.ContextMenu));
		m_ActionBarConsoleView.AddInputToPages(inputLayer, m_ActionBarActive);
	}

	public List<GridConsoleNavigationBehaviour> GetIgnoreNavigation()
	{
		return new List<GridConsoleNavigationBehaviour> { m_ActionBarNavigation };
	}

	private void OnAbilityClick(Ability ability)
	{
		if (base.ViewModel.ChooseAbilityMode.Value)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(ability, base.ViewModel.TargetSlotIndex);
			});
			base.ViewModel.ChooseAbilityMode.Value = false;
			return;
		}
		SurfaceActionBarSlotAbilityConsoleView targetSlot = m_ActionBarConsoleView.GetFirstEmptySlot();
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.MoveSlot(ability, targetSlot.Index);
		});
		m_ActionBarNavigation.FocusOnEntityManual(targetSlot);
		base.ViewModel.ActionBarPartAbilitiesVM.SetMoveAbilityMode(on: true);
	}

	private void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void OnActionBarFocused(IConsoleEntity focus)
	{
		m_ActionBarActive.Value = focus != null;
		m_CurrentAbilitySlot = focus as SurfaceActionBarSlotAbilityConsoleView;
	}

	private void ShowContextMenu(InputActionEventData data)
	{
		m_CurrentAbilitySlot.ShowContextMenu(m_CurrentAbilitySlot.ContextMenuEntities);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
