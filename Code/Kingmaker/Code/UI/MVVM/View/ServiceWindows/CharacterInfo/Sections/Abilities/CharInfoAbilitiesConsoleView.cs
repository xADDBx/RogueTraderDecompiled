using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesConsoleView : CharInfoAbilitiesBaseView, ICharInfoComponentConsoleView, ICharInfoComponentView, ICharInfoIgnoreNavigationConsoleView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ActionBarNavigation;

	private GridConsoleNavigationBehaviour m_ScrollRectNavigation;

	private Action<IConsoleEntity> m_RefreshParentFocus;

	private readonly List<CharInfoFeatureGroupConsoleView> m_FeatureGroups = new List<CharInfoFeatureGroupConsoleView>();

	private SurfaceActionBarPartAbilitiesConsoleView m_ActionBarConsoleView;

	private readonly BoolReactiveProperty m_ActionBarActive = new BoolReactiveProperty();

	private SurfaceActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_ChangeTabHint;

	[SerializeField]
	private ConsoleHint m_ChangeTabHintAdditional;

	private InputLayer m_ChooseAbilityLayer;

	private CharInfoFeatureConsoleView m_MoveModeAbility;

	protected override void BindViewImplementation()
	{
		m_ActionBarConsoleView = m_ActionBarPartAbilitiesView as SurfaceActionBarPartAbilitiesConsoleView;
		CreateNavigation();
		CurrentSelectedTab.Value = CurrentInfoAbilitiesTab.ActiveAbilities;
		base.BindViewImplementation();
		AddDisposable(m_ActionBarNavigation = m_ActionBarConsoleView.Or(null)?.NavigationBehaviour);
		AddDisposable(m_ActionBarNavigation?.DeepestFocusAsObservable.Subscribe(OnActionBarFocused));
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ActionBarNavigation);
		RefreshFocus();
		CreateActionBarManagement();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CurrentAbilitySlot = null;
		m_MoveModeAbility = null;
		m_NavigationBehaviour.RemoveEntity(m_ActionBarNavigation);
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		Action<CharInfoFeatureConsoleView> onAbilityClick = ((CurrentSelectedTab.Value == CurrentInfoAbilitiesTab.ActiveAbilities) ? new Action<CharInfoFeatureConsoleView>(OnAbilityClick) : null);
		if (m_WidgetList.Entries != null)
		{
			m_WidgetList.Entries.ForEach(delegate(IWidgetView e)
			{
				(e as CharInfoFeatureGroupConsoleView)?.SetupChooseModeActions(onAbilityClick, OnAbilityFocus);
			});
			UpdateNavigation();
			m_ScrollRect.ScrollToTop();
		}
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_ScrollRectNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(m_ScrollRectNavigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_ScrollRectNavigation);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateActionBarManagement()
	{
		m_ChooseAbilityLayer = m_ScrollRectNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "ChooseAbility"
		});
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = UniRxExtensionMethods.Or(base.ViewModel.ChooseAbilityMode, base.ViewModel.ActionBarPartAbilitiesVM.MoveAbilityMode).ToReactiveProperty();
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
			base.ViewModel.ChooseAbilityMode.Value = false;
		}, 9, readOnlyReactiveProperty), UIStrings.Instance.CommonTexts.Cancel));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseAbilityLayer.AddButton(delegate
		{
		}, 8, base.ViewModel.ChooseAbilityMode), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(base.ViewModel.ChooseAbilityMode.Subscribe(delegate(bool on)
		{
			if (on)
			{
				AddDisposable(GamePad.Instance.PushLayer(m_ChooseAbilityLayer));
				m_NavigationBehaviour.UnFocusCurrentEntity();
				m_ScrollRectNavigation.FocusOnFirstValidEntity();
				DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
				{
					OnAbilityFocus(m_ScrollRectNavigation.DeepestNestedFocus as CharInfoFeatureConsoleView);
				});
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: true);
				});
			}
			else
			{
				OnAbilityFocus(null);
				GamePad.Instance.PopLayer(m_ChooseAbilityLayer);
				m_ScrollRectNavigation.UnFocusCurrentEntity();
				m_NavigationBehaviour.FocusOnCurrentEntity();
				EventBus.RaiseEvent(delegate(ICharInfoAbilitiesChooseModeHandler h)
				{
					h.HandleChooseMode(active: false);
				});
				EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
				{
					h.HandleFocus();
				});
			}
		}));
		AddDisposable(base.ViewModel.ActionBarPartAbilitiesVM.MoveAbilityMode.Subscribe(delegate(bool on)
		{
			if (!on)
			{
				m_ActionBarNavigation.UnFocusCurrentEntity();
				m_NavigationBehaviour.FocusOnCurrentEntity();
				EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
				{
					h.HandleFocus();
				});
				if ((bool)m_MoveModeAbility)
				{
					m_MoveModeAbility.SetMoveState(state: false);
					m_MoveModeAbility = null;
				}
			}
		}));
	}

	private void UpdateNavigation()
	{
		m_ScrollRectNavigation.Clear();
		if (m_WidgetList.Entries == null)
		{
			return;
		}
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			if (entry is CharInfoFeatureGroupConsoleView charInfoFeatureGroupConsoleView)
			{
				m_FeatureGroups.Add(charInfoFeatureGroupConsoleView);
				m_ScrollRectNavigation.AddRow<GridConsoleNavigationBehaviour>(charInfoFeatureGroupConsoleView.GetNavigation());
			}
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		InputBindStruct inputBindStruct = inputLayer.AddButton(ShowContextMenu, 10, m_ActionBarActive.And(base.ViewModel.IsNotControllableCharacter?.Not()).ToReactiveProperty());
		AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.ContextMenu.ContextMenu));
		AddDisposable(inputBindStruct);
		m_ActionBarConsoleView.AddInputToPages(inputLayer, m_ActionBarActive);
		if (!base.ViewModel.Unit.Value.IsPet)
		{
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				ToggleAbilitiesTab();
			}, 18);
			AddDisposable(m_ChangeTabHint.Bind(inputBindStruct2));
			AddDisposable(m_ChangeTabHintAdditional.Bind(inputBindStruct2));
			AddDisposable(inputBindStruct2);
		}
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_ChangeTabHint.gameObject.SetActive(!u.IsPet);
		}));
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_ChangeTabHintAdditional.gameObject.SetActive(!u.IsPet);
		}));
		if (!base.ViewModel.Unit.Value.IsStarship() && !base.ViewModel.Unit.Value.IsPlayerShip())
		{
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
			{
				ToggleGroupingMode();
			}, 17);
			AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CharGen.OrderByType));
			AddDisposable(inputBindStruct3);
		}
	}

	public List<GridConsoleNavigationBehaviour> GetIgnoreNavigation()
	{
		return new List<GridConsoleNavigationBehaviour> { m_ActionBarNavigation };
	}

	private void OnAbilityClick(CharInfoFeatureConsoleView featureView)
	{
		Ability ability = (featureView.GetViewModel() as CharInfoFeatureVM)?.Ability;
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
		m_NavigationBehaviour.UnFocusCurrentEntity();
		EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
		{
			h.MoveSlot(ability, targetSlot.Index);
		});
		m_ActionBarNavigation.FocusOnEntityManual(targetSlot);
		base.ViewModel.ActionBarPartAbilitiesVM.SetMoveAbilityMode(on: true);
		m_MoveModeAbility = featureView;
		featureView.SetMoveState(state: true);
	}

	private void OnAbilityFocus(CharInfoFeatureConsoleView featureView)
	{
		ActionBarSlotVM actionBarSlotVM = base.ViewModel.ActionBarPartAbilitiesVM.Slots.ElementAtOrDefault(base.ViewModel.TargetSlotIndex);
		Ability ability = ((!(featureView != null)) ? null : (featureView.GetViewModel() as CharInfoFeatureVM)?.Ability);
		if (ability != null || actionBarSlotVM == null || actionBarSlotVM.IsEmpty.Value)
		{
			actionBarSlotVM?.OverrideIcon(ability?.Icon);
		}
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

	private void ToggleAbilitiesTab()
	{
		int activeAbilitiesState = (int)(CurrentSelectedTab.Value + 1) % 3;
		SetActiveAbilitiesState((CurrentInfoAbilitiesTab)activeAbilitiesState);
		RefreshFocus();
	}

	private void ToggleGroupingMode()
	{
		FeatureGroupingMode groupingMode = ((base.ViewModel.GroupingMode.Value == FeatureGroupingMode.ByType) ? FeatureGroupingMode.BySource : FeatureGroupingMode.ByType);
		base.ViewModel.SetGroupingMode(groupingMode);
		RefreshView();
	}

	private void RefreshFocus()
	{
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			foreach (CharInfoFeatureGroupConsoleView featureGroup in m_FeatureGroups)
			{
				CharInfoFeatureGroupVM.FeatureGroupType groupType = featureGroup.GroupType;
				if (groupType != 0 && (groupType != CharInfoFeatureGroupVM.FeatureGroupType.Abilities || ActiveAbilitiesSelected.Value))
				{
					IConsoleEntity firstFeature = featureGroup.GetFirstFeature();
					if (firstFeature != null)
					{
						m_NavigationBehaviour.FocusOnEntityManual(firstFeature);
						break;
					}
				}
			}
		});
		m_ScrollRect.ScrollToTop();
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
