using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipSkillsConsoleView : ShipSkillsBaseView<ShipCareerPathSelectionTabsConsoleView>, IShipCustomizationPage
{
	private readonly ReactiveProperty<bool> m_CanReset = new ReactiveProperty<bool>();

	private readonly BoolReactiveProperty m_CanShowInfo = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanFuncAdditional = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsInScreenNavigation = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsOnCareerItem = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsOnRightPanel = new BoolReactiveProperty();

	private readonly CompositeDisposable m_InfoDisposables = new CompositeDisposable();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_ScreenNavigation;

	private ConsoleNavigationBehaviour m_LeftNavigation;

	private GridConsoleNavigationBehaviour m_RightNavigation;

	private bool m_InputAdded;

	private IConsoleHint m_ConfirmHint;

	private IConsoleHint m_DeclineHint;

	private bool m_HasTooltip;

	private bool m_ShowTooltip = true;

	private TooltipConfig m_TooltipConfig;

	private Dictionary<ConsoleNavigationBehaviour, IConsoleEntity> m_PreviousEntities = new Dictionary<ConsoleNavigationBehaviour, IConsoleEntity>();

	private ConsoleNavigationBehaviour m_PreviousNavigation;

	public ShipCareerPathSelectionTabsConsoleView CareerPathSelectionTabsConsoleView => m_ShipCareerPathSelectionTabsPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_ShipCareerPathSelectionTabsPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ShipCareerPathSelectionTabsPCView.Bind(base.ViewModel.ShipProgressionVM?.CareerPathVM);
		AddDisposable(base.ViewModel.ShipProgressionVM?.CareerPathVM.HasNewValidSelections.Subscribe(delegate(bool value)
		{
			m_CanReset.Value = value;
		}));
		AddDisposable(base.ViewModel.ShipProgressionVM?.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem value)
		{
			CareerPathVM careerPathVM = base.ViewModel.ShipProgressionVM?.CareerPathVM;
			if (value != null && value.CanSelect())
			{
				FocusOnRightPanel();
			}
			else if (careerPathVM != null && careerPathVM.CanCommit.Value && careerPathVM.AllVisited.Value && value == null)
			{
				FocusOnLeftPanel();
				m_LeftNavigation.FocusOnEntityManual(m_LeftNavigation.Entities.First());
			}
		}));
		AddDisposable(base.ViewModel.ShipProgressionVM?.OnRepeatedCurrentRankEntryItem.Subscribe(delegate
		{
			FocusOnRightPanel();
		}));
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoSection.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		AddDisposable(EventBus.Subscribe(this));
		DelayedInvoker.InvokeInFrames(delegate
		{
			FocusOnLeftPanel();
		}, 1);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TooltipHelper.HideTooltip();
		m_NavigationBehaviour.Dispose();
		m_NavigationBehaviour = null;
		m_InputAdded = false;
		m_InfoDisposables.Clear();
		m_PreviousEntities.Clear();
	}

	private void OnFocusLeftPanel(IConsoleEntity entity)
	{
		if (entity == null)
		{
			m_IsOnCareerItem.Value = false;
			return;
		}
		m_IsOnCareerItem.Value = entity == m_LeftNavigation.Entities.First();
		OnFocusEntity(entity);
		m_PreviousEntities[m_LeftNavigation] = entity;
		m_PreviousNavigation = m_LeftNavigation;
	}

	private void OnFocusRightPanel(IConsoleEntity entity)
	{
		m_IsOnRightPanel.Value = entity != null;
		if (entity != null)
		{
			OnFocusEntity(entity);
			m_PreviousEntities[m_RightNavigation] = entity;
			m_PreviousNavigation = m_RightNavigation;
		}
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		string confirmClickHint = entity.GetConfirmClickHint();
		m_ConfirmHint?.SetLabel((!string.IsNullOrEmpty(confirmClickHint)) ? confirmClickHint : ((string)UIStrings.Instance.CommonTexts.Select));
		string label = ((m_ScreenNavigation.CurrentEntity == m_LeftNavigation && m_IsOnCareerItem.Value) ? UIStrings.Instance.CommonTexts.CloseWindow : UIStrings.Instance.CommonTexts.Back);
		m_DeclineHint?.SetLabel(label);
		HandleTooltip(entity);
		m_CanConfirm.Value = m_IsInScreenNavigation.Value && ((entity as IConfirmClickHandler)?.CanConfirmClick() ?? false);
		m_CanFuncAdditional.Value = (entity as IFuncAdditionalClickHandler)?.CanFuncAdditionalClick() ?? false;
	}

	private void FocusOnRightPanel()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_LeftNavigation.UnFocusCurrentEntity();
			m_ScreenNavigation.SetCurrentEntity(m_RightNavigation);
			m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
			m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			m_RightNavigation.FocusOnCurrentEntity();
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}, 1);
	}

	private void FocusOnLeftPanel()
	{
		m_ScreenNavigation.SetCurrentEntity(m_LeftNavigation);
		m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
		m_LeftNavigation.FocusOnCurrentEntity();
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		m_NavigationBehaviour.UpdateDeepestFocusObserve();
	}

	private void ToggleInfoNavigation()
	{
		TooltipHelper.HideTooltip();
		m_NavigationBehaviour.Clear();
		if (m_IsInScreenNavigation.Value)
		{
			GridConsoleNavigationBehaviour navigationBehaviour = m_InfoSection.GetNavigationBehaviour();
			m_InfoDisposables.Add(navigationBehaviour.DeepestFocusAsObservable.Subscribe(HandleTooltip));
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour);
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			m_DeclineHint?.SetLabel(UIStrings.Instance.CommonTexts.Back);
			m_ConfirmHint?.SetLabel(UIStrings.Instance.CommonTexts.Information);
		}
		else
		{
			bool flag = m_PreviousNavigation == m_RightNavigation;
			m_InfoDisposables.Clear();
			foreach (var (consoleNavigationBehaviour2, currentEntity) in m_PreviousEntities)
			{
				consoleNavigationBehaviour2.SetCurrentEntity(currentEntity);
			}
			m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
			m_NavigationBehaviour.SetCurrentEntity(m_ScreenNavigation);
			if (flag)
			{
				m_ScreenNavigation.FocusOnEntityManual(m_RightNavigation);
				m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
			}
			else
			{
				m_PreviousNavigation.FocusOnEntityManual(m_PreviousEntities[m_PreviousNavigation]);
				m_ScreenNavigation.FocusOnEntityManual(m_PreviousNavigation);
				m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
				m_RightNavigation.UnFocusCurrentEntity();
			}
		}
		m_IsInScreenNavigation.Value = !m_IsInScreenNavigation.Value;
		m_NavigationBehaviour.UpdateDeepestFocusObserve();
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_ScreenNavigation = new GridConsoleNavigationBehaviour());
		m_LeftNavigation = m_CareerPathRoundProgression.GetNavigationBehaviour();
		m_RightNavigation = CareerPathSelectionTabsConsoleView.GetNavigationBehaviour();
		m_PreviousEntities.Add(m_LeftNavigation, null);
		m_PreviousEntities.Add(m_RightNavigation, null);
		m_ScreenNavigation.AddColumn<ConsoleNavigationBehaviour>(m_LeftNavigation);
		m_ScreenNavigation.AddColumn<GridConsoleNavigationBehaviour>(m_RightNavigation);
		m_ScreenNavigation.FocusOnEntityManual(m_LeftNavigation);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ScreenNavigation);
		m_NavigationBehaviour.FocusOnEntityManual(m_ScreenNavigation);
		AddDisposable(m_LeftNavigation.DeepestFocusAsObservable.Subscribe(OnFocusLeftPanel));
		AddDisposable(m_RightNavigation.DeepestFocusAsObservable.Subscribe(OnFocusRightPanel));
		UpdateCurrentFocusState();
		m_IsInScreenNavigation.Value = true;
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour;
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && !m_ScreenNavigation.IsFocused;
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (!m_IsInScreenNavigation.Value && m_ShowTooltip && (bool)monoBehaviour && tooltipBaseTemplate != null)
		{
			monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, m_TooltipConfig);
		}
		if (entity is IPrerequisiteLinkEntity prerequisiteLinkEntity)
		{
			m_CanShowInfo.Value = base.ViewModel.ShipProgressionVM.CurrentRankEntryItem.Value.ContainsFeature(prerequisiteLinkEntity.LinkId);
		}
		else
		{
			m_CanShowInfo.Value = false;
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		if (!m_InputAdded)
		{
			CareerPathVM careerPathVM = base.ViewModel.ShipProgressionVM?.CareerPathVM;
			IReadOnlyReactiveProperty<bool> property = careerPathVM?.CanCommit.CombineLatest(m_IsOnCareerItem, m_IsOnRightPanel, (bool canCommit, bool isOnCareer, bool onRightPanel) => canCommit && isOnCareer && base.ViewModel.ShipProgressionVM.CurrentRankEntryItem.Value == null && !onRightPanel).And(careerPathVM.AllVisited).And(m_IsInScreenNavigation)
				.ToReactiveProperty();
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				OnConfirmClick();
			}, 8, m_CanConfirm.And(m_IsInScreenNavigation).And(property.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
			AddDisposable(m_ConfirmHint = hintsWidget.BindHint(inputBindStruct));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				m_PreviousEntities.Remove(m_RightNavigation);
				(m_NavigationBehaviour.DeepestNestedFocus as IConfirmClickHandler)?.OnConfirmClick();
				AddDisposable(DelayedInvoker.InvokeInFrames(ToggleInfoNavigation, 2));
			}, 8, m_CanShowInfo);
			AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.Tooltips.ToCurrentPrerequisiteFeature));
			AddDisposable(inputBindStruct2);
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
			{
				OnDeclineClick();
			}, 9);
			AddDisposable(m_DeclineHint = hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.CloseWindow));
			AddDisposable(inputBindStruct3);
			InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
			{
				OnFuncAdditionalClick();
			}, 17, m_CanFuncAdditional, InputActionEventType.ButtonJustReleased);
			AddDisposable(inputBindStruct4);
			AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CharacterSheet.ToggleFavorites));
			InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
			{
				ToggleInfoNavigation();
			}, 10, m_IsInScreenNavigation);
			AddDisposable(hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.CommonTexts.Information));
			AddDisposable(inputBindStruct5);
			AddDisposable(inputLayer.AddAxis(Scroll, 3, m_IsInScreenNavigation));
			CareerPathSelectionTabsConsoleView.AddInput(ref inputLayer, ref hintsWidget);
			InputBindStruct inputBindStruct6 = inputLayer.AddButton(delegate
			{
				base.ViewModel.ShipProgressionVM.Commit();
			}, 8, property, InputActionEventType.ButtonJustLongPressed);
			AddDisposable(hintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.CharGen.Complete));
			AddDisposable(inputBindStruct6);
			if (m_NavigationBehaviour != null)
			{
				FocusOnLeftPanel();
			}
			m_InputAdded = true;
		}
	}

	private void OnConfirmClick()
	{
		if (m_NavigationBehaviour.DeepestNestedFocus is IConfirmClickHandler confirmClickHandler)
		{
			confirmClickHandler.OnConfirmClick();
			UpdateCurrentFocusState();
		}
	}

	public void UpdateCurrentFocusState()
	{
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFuncAdditionalClick()
	{
		if (m_NavigationBehaviour.DeepestNestedFocus is IFuncAdditionalClickHandler funcAdditionalClickHandler)
		{
			funcAdditionalClickHandler.OnFuncAdditionalClick();
		}
	}

	private void OnDeclineClick()
	{
		if (!m_IsInScreenNavigation.Value)
		{
			ToggleInfoNavigation();
		}
		else if (m_ScreenNavigation.CurrentEntity == m_RightNavigation)
		{
			if (base.ViewModel.ShipProgressionVM.CareerPathVM.IsInLevelupProcess && base.ViewModel.ShipProgressionVM?.CurrentRankEntryItem.Value != null && base.ViewModel.ShipProgressionVM.CurrentRankEntryItem.Value.CanSelect())
			{
				if (base.ViewModel.ShipProgressionVM?.CurrentRankEntryItem.Value == null)
				{
					FocusOnLeftPanel();
					return;
				}
				base.ViewModel.ShipProgressionVM.CareerPathVM.SelectPreviousItem();
				if (base.ViewModel.ShipProgressionVM.CurrentRankEntryItem.Value == null)
				{
					FocusOnLeftPanel();
				}
				UpdateCurrentFocusState();
			}
			else
			{
				m_RightNavigation.UnFocusCurrentEntity();
				FocusOnLeftPanel();
			}
		}
		else if (m_IsOnCareerItem.Value)
		{
			EventBus.RaiseEvent(delegate(IShipCustomizationForceUIHandler h)
			{
				h.HandleForceCloseAllComponentsMenu();
			});
		}
		else
		{
			m_LeftNavigation.FocusOnEntityManual(m_LeftNavigation.Entities.First());
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_InfoSection.Scroll(value);
	}

	public bool CanOverrideClose()
	{
		return false;
	}
}
