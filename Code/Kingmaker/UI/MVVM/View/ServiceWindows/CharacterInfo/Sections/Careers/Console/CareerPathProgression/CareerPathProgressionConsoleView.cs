using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression;

public class CareerPathProgressionConsoleView : CareerPathProgressionCommonView, ICharInfoCanHookConfirm
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_ExpandHint;

	private readonly ReactiveProperty<bool> m_CanReset = new ReactiveProperty<bool>();

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsInScreenNavigation = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanShowInfo = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsOnCareerItem = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsOnRightPanel = new BoolReactiveProperty();

	private readonly CompositeDisposable m_DelayedInvokes = new CompositeDisposable();

	private readonly CompositeDisposable m_InfoDisposables = new CompositeDisposable();

	private bool m_InputAdded;

	private IDisposable m_DelayedFocusUpdate;

	private ConsoleHintDescription m_DeclineHint;

	private ConsoleHintDescription m_ConfirmHint;

	private ConsoleHintDescription m_Func01Hint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ScreenNavigation;

	private IConsoleNavigationOwner m_NavigationOwner;

	private Dictionary<ConsoleNavigationBehaviour, IConsoleEntity> m_PreviousEntities = new Dictionary<ConsoleNavigationBehaviour, IConsoleEntity>();

	private ConsoleNavigationBehaviour m_PreviousBehaviour;

	private bool m_HasTooltip;

	private bool m_ShowTooltip = true;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_ContentEntity;

	private CareerPathSelectionTabsConsoleView CareerPathSelectionTabsConsoleView => m_CareerPathSelectionPartCommonView as CareerPathSelectionTabsConsoleView;

	private ConsoleNavigationBehaviour LeftNavigation => m_CareerPathRoundProgressionCommonView.GetNavigationBehaviour();

	private GridConsoleNavigationBehaviour RightNavigation => CareerPathSelectionTabsConsoleView.GetNavigationBehaviour();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoSection.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		AddDisposable(m_IsShown.And(base.ViewModel.HasNewValidSelections).Subscribe(delegate(bool value)
		{
			m_CanReset.Value = value;
		}));
		AddDisposable(base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem value)
		{
			if (value != null && value.CanSelect())
			{
				FocusOnRightPanel();
			}
			else if (base.ViewModel.CanCommit.Value && base.ViewModel.AllVisited.Value && value == null)
			{
				LeftNavigation.FocusOnEntityManual(LeftNavigation.Entities.First());
			}
		}));
		AddDisposable(base.ViewModel.UnitProgressionVM.OnRepeatedCurrentRankEntryItem.Subscribe(delegate
		{
			SwitchDescriptionShowed(true);
			FocusOnRightPanel();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputAdded = false;
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_DelayedFocusUpdate?.Dispose();
		m_DelayedFocusUpdate = null;
		m_DelayedInvokes?.Clear();
		m_InfoDisposables?.Clear();
		m_PreviousEntities.Clear();
	}

	private void CreateNavigation(IConsoleNavigationOwner owner)
	{
		m_NavigationOwner = owner;
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner));
		AddDisposable(m_ScreenNavigation = new GridConsoleNavigationBehaviour());
		m_PreviousEntities.Add(LeftNavigation, null);
		m_PreviousEntities.Add(RightNavigation, null);
		m_ScreenNavigation.AddColumn<ConsoleNavigationBehaviour>(LeftNavigation);
		m_ScreenNavigation.AddColumn<GridConsoleNavigationBehaviour>(RightNavigation);
		m_ScreenNavigation.FocusOnEntityManual(LeftNavigation);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		m_IsInScreenNavigation.Value = true;
		AddDisposable(LeftNavigation.DeepestFocusAsObservable.Subscribe(OnFocusToLeftNavigation));
		AddDisposable(RightNavigation.DeepestFocusAsObservable.Subscribe(OnFocusToRightNavigation));
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner)
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation(owner);
		}
		return m_NavigationBehaviour;
	}

	private void FocusOnRightPanel()
	{
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_ScreenNavigation.SetCurrentEntity(RightNavigation);
			m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
			m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			RightNavigation.FocusOnCurrentEntity();
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}, 3));
	}

	private void FocusOnLeftPanel()
	{
		m_ScreenNavigation.SetCurrentEntity(LeftNavigation);
		m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		LeftNavigation.FocusOnCurrentEntity();
		m_NavigationBehaviour.UpdateDeepestFocusObserve();
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void ToggleInfoNavigation()
	{
		TooltipHelper.HideTooltip();
		m_NavigationBehaviour.Clear();
		if (m_IsInScreenNavigation.Value)
		{
			GridConsoleNavigationBehaviour navigationBehaviour = m_InfoSection.GetNavigationBehaviour();
			m_InfoDisposables.Add(navigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusToInfo));
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour);
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DeclineHint?.SetLabel(UIStrings.Instance.CommonTexts.Back);
			m_ConfirmHint?.SetLabel(UIStrings.Instance.CommonTexts.Information);
		}
		else
		{
			m_InfoDisposables.Clear();
			foreach (var (consoleNavigationBehaviour2, currentEntity) in m_PreviousEntities)
			{
				consoleNavigationBehaviour2.SetCurrentEntity(currentEntity);
			}
			IConsoleEntity entity = m_PreviousEntities[m_PreviousBehaviour];
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
			m_PreviousBehaviour.SetCurrentEntity(m_PreviousEntities[m_PreviousBehaviour]);
			m_ScreenNavigation.SetCurrentEntity(m_PreviousBehaviour);
			m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			m_PreviousBehaviour.FocusOnEntityManual(entity);
		}
		m_IsInScreenNavigation.Value = !m_IsInScreenNavigation.Value;
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void OnFocusToLeftNavigation(IConsoleEntity entity)
	{
		if (entity == null)
		{
			m_IsOnCareerItem.Value = false;
			return;
		}
		if (m_ScreenNavigation.CurrentEntity != LeftNavigation)
		{
			m_ScreenNavigation.SetCurrentEntity(LeftNavigation);
		}
		m_PreviousEntities[LeftNavigation] = entity;
		m_PreviousBehaviour = LeftNavigation;
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationOwner?.EntityFocused(entity);
		}, 1));
		m_IsOnCareerItem.Value = entity == LeftNavigation.Entities.First();
		string label = (m_IsOnCareerItem.Value ? UIStrings.Instance.CharacterSheet.BackToCareersList : UIStrings.Instance.CommonTexts.Back);
		m_DeclineHint?.SetLabel(label);
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour != null)
		{
			EnsureVisible(monoBehaviour.GetComponent<RectTransform>());
		}
		IRankEntrySelectItem value = base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value;
		if (value == null || !value.CanSelect())
		{
			SwitchDescriptionShowed(false);
		}
	}

	private void OnFocusToRightNavigation(IConsoleEntity entity)
	{
		m_IsOnRightPanel.Value = entity != null;
		if (entity == null)
		{
			if (!RightNavigation.IsFocused && m_IsInScreenNavigation.Value)
			{
				FocusOnLeftPanel();
			}
			return;
		}
		if (m_ScreenNavigation.CurrentEntity != RightNavigation)
		{
			m_ScreenNavigation.SetCurrentEntity(RightNavigation);
		}
		m_PreviousEntities[RightNavigation] = entity;
		m_PreviousBehaviour = RightNavigation;
		m_DeclineHint?.SetLabel(UIStrings.Instance.CommonTexts.Back);
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		if (entity is TMPLinkNavigationEntity)
		{
			m_CanConfirm.Value = false;
		}
		m_IsOnCareerItem.Value = false;
		m_DelayedInvokes.Add(DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationOwner?.EntityFocused(entity);
		}, 1));
	}

	private void OnFocusToInfo(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && !m_ScreenNavigation.IsFocused;
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (m_ShowTooltip && (bool)monoBehaviour && tooltipBaseTemplate != null)
		{
			monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, m_TooltipConfig);
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		if (m_InputAdded)
		{
			return;
		}
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnDeclinePressed();
		}, 9);
		AddDisposable(m_DeclineHint = hintsWidget.BindHint(inputBindStruct) as ConsoleHintDescription);
		AddDisposable(inputBindStruct);
		AddDisposable(inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, m_IsInScreenNavigation.And(m_CanConfirm).ToReactiveProperty()));
		AddDisposable(inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Select((IRankEntrySelectItem item) => item is RankEntryFeatureItemVM).ToReactiveProperty()));
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			ToggleInfoNavigation();
		}, 10, m_IsInScreenNavigation.CombineLatest(base.ViewModel.IsDescriptionShowed, (bool isScreen, bool showDesc) => isScreen && (showDesc || m_AlwaysShowInfo)).ToReactiveProperty());
		AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct3);
		AddDisposable(inputLayer.AddAxis(Scroll, 3, m_IsInScreenNavigation.And(base.ViewModel.IsDescriptionShowed).ToReactiveProperty()));
		CareerPathSelectionTabsConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			base.ViewModel.Commit();
		}, 8, base.ViewModel.CanCommit.CombineLatest(base.ViewModel.AllVisited, m_IsOnRightPanel, (bool canCommit, bool allVisited, bool onRightPanel) => canCommit && allVisited && !onRightPanel).And(m_IsOnCareerItem).And(m_IsInScreenNavigation)
			.ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed);
		AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CharGen.Complete));
		AddDisposable(inputBindStruct4);
		if ((bool)m_ExpandHint && m_CanExpand)
		{
			InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
			{
				base.ViewModel.SwitchDescriptionShowed(true);
			}, 8, m_IsInScreenNavigation.CombineLatest(m_IsOnRightPanel, base.ViewModel.IsDescriptionShowed, (bool isOnScreen, bool onRightPanel, bool descShowed) => isOnScreen && onRightPanel && !descShowed).ToReactiveProperty());
			AddDisposable(m_ExpandHint.Bind(inputBindStruct5));
			m_ExpandHint.SetLabel(UIStrings.Instance.CommonTexts.Expand);
			AddDisposable(inputBindStruct5);
		}
		if (m_NavigationBehaviour != null)
		{
			OnFocusToLeftNavigation(m_NavigationBehaviour.DeepestNestedFocus);
		}
		m_InputAdded = true;
	}

	private void OnDeclinePressed()
	{
		if (!m_IsInScreenNavigation.Value)
		{
			ToggleInfoNavigation();
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
			return;
		}
		if (m_ScreenNavigation.CurrentEntity == RightNavigation)
		{
			if (base.ViewModel.IsInLevelupProcess && base.ViewModel.UnitProgressionVM?.CurrentRankEntryItem.Value != null && base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value.CanSelect())
			{
				if (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null)
				{
					FocusOnLeftPanel();
				}
				else
				{
					base.ViewModel.SelectPreviousItem();
					if (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null)
					{
						FocusOnLeftPanel();
					}
				}
			}
			else
			{
				RightNavigation.UnFocusCurrentEntity();
				m_ScreenNavigation.FocusOnEntityManual(LeftNavigation);
				m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
				SwitchDescriptionShowed(false);
				EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
				{
					h.SetFocusOn(null);
				});
			}
		}
		else if (m_IsOnCareerItem.Value)
		{
			HandleReturn();
		}
		else
		{
			base.ViewModel.SetRankEntry(null);
			LeftNavigation.FocusOnEntityManual(LeftNavigation.Entities.First());
		}
		if (m_NavigationBehaviour != null)
		{
			OnFocusToLeftNavigation(m_NavigationBehaviour.DeepestNestedFocus);
		}
		EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
		{
			h.HandleFocus();
		});
	}

	private void OnConfirmClick()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (m_ScreenNavigation.IsFocused && base.ViewModel.IsInLevelupProcess && base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value == null)
			{
				FocusOnLeftPanel();
			}
		}, 1);
	}

	private void ToggleTooltip()
	{
		TooltipHelper.HideTooltip();
		m_ShowTooltip = !m_ShowTooltip;
		OnFocusToInfo(m_NavigationBehaviour.DeepestNestedFocus);
	}

	public IReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return m_CanConfirm.And(m_IsInScreenNavigation).And(m_IsOnCareerItem.And(base.ViewModel.CanCommit).And(base.ViewModel.AllVisited).Not()).ToReactiveProperty();
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_InfoSection.Scroll(value);
	}
}
