using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.PagesMenu;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public class CharacterInfoConsoleView : CharacterInfoPCView, IUpdateFocusHandler, ISubscriber, ICullFocusHandler
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private RectTransform m_TooltipPlaceForLeftPanel;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_NavigationPanelLeft;

	private GridConsoleNavigationBehaviour m_NavigationPanelRight;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_LeftPanelSelected = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_RightPanelSelected = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	private readonly Dictionary<CharInfoComponentType, ICharInfoComponentConsoleView> m_ComponentConsoleViews = new Dictionary<CharInfoComponentType, ICharInfoComponentConsoleView>();

	private List<GridConsoleNavigationBehaviour> m_IgnoreRightPanels = new List<GridConsoleNavigationBehaviour>();

	private IDisposable m_CanHookDeclineSubscription;

	private readonly List<CharInfoComponentType> m_LeftPanelViews = new List<CharInfoComponentType>
	{
		CharInfoComponentType.LevelClassScores,
		CharInfoComponentType.SkillsAndWeapons
	};

	private IConsoleHint m_ConfirmHint;

	private TooltipConfig m_RightPanelConfig;

	private TooltipConfig m_LeftPanelConfig;

	private IConsoleEntity m_CulledFocus;

	private ConsoleNavigationBehaviour m_PrevNavigation;

	public override void Initialize()
	{
		base.Initialize();
		foreach (var (key, charInfoComponentView2) in ComponentViews)
		{
			if (charInfoComponentView2 is ICharInfoComponentConsoleView value)
			{
				m_ComponentConsoleViews.Add(key, value);
			}
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RightPanelConfig = new TooltipConfig
		{
			TooltipPlace = m_TooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_LeftPanelConfig = new TooltipConfig
		{
			TooltipPlace = m_TooltipPlaceForLeftPanel,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.5f)
			}
		};
		CreateNavigation();
		AddDisposable(base.ViewModel.PagesSelectionGroupRadioVM.SelectedEntity.Subscribe(delegate
		{
			RefreshInput();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.BiographyUpdated, delegate
		{
			RefreshInput();
		}));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.SetFocusChangeAction(OnFocusEntity);
		AddDisposable(EventBus.Subscribe(this));
		TooltipHelper.HideTooltip();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CanHookDeclineSubscription?.Dispose();
		m_CanHookDeclineSubscription = null;
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationPanelLeft = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationPanelRight = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharScreenView"
		});
		AddDisposable(m_NavigationPanelLeft.Focus.Subscribe(delegate
		{
			ToggleLeftPanelFocus();
		}));
		AddDisposable(m_NavigationPanelRight.DeepestFocusAsObservable.Subscribe(ToggleRightCanvasFocus));
		AddDisposable(m_NavigationBehaviour.OnClickAsObservable().Subscribe(OnClick));
		CreateInput();
	}

	private void CreateInput()
	{
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9, m_CanDecline), UIStrings.Instance.CommonTexts.CloseWindow));
		(m_CharInfoPagesMenu as CharInfoPagesMenuConsoleView)?.AddHints(m_InputLayer, m_RightPanelSelected);
		if (m_NameAndPortraitView is CharInfoNameAndPortraitConsoleView charInfoNameAndPortraitConsoleView)
		{
			charInfoNameAndPortraitConsoleView.AddInput(m_InputLayer, m_ConsoleHintsWidget, m_LeftPanelSelected);
		}
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(inputBindStruct);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Information));
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanConfirm);
		m_ConfirmHint = m_ConsoleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Select);
		AddDisposable(inputBindStruct2);
		AddDisposable(m_ConfirmHint);
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.AddInput(m_InputLayer, m_ConsoleHintsWidget);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		AddDisposable(m_CanvasSortingComponent.PushView());
	}

	private void RefreshInput()
	{
		m_NavigationPanelLeft.Clear();
		m_NavigationBehaviour.Clear();
		m_NavigationPanelRight.Clear();
		m_IgnoreRightPanels.Clear();
		m_CanHookDeclineSubscription?.Dispose();
		m_CanHookDeclineSubscription = null;
		foreach (var (item, charInfoComponentConsoleView2) in m_ComponentConsoleViews)
		{
			if (!charInfoComponentConsoleView2.IsBinded)
			{
				continue;
			}
			GridConsoleNavigationBehaviour navigationBehaviour = (m_LeftPanelViews.Contains(item) ? m_NavigationPanelLeft : m_NavigationPanelRight);
			charInfoComponentConsoleView2.AddInput(ref m_InputLayer, ref navigationBehaviour, m_ConsoleHintsWidget);
			if (charInfoComponentConsoleView2 is ICharInfoIgnoreNavigationConsoleView charInfoIgnoreNavigationConsoleView)
			{
				m_IgnoreRightPanels.AddRange(charInfoIgnoreNavigationConsoleView.GetIgnoreNavigation());
			}
			if (charInfoComponentConsoleView2 is ICharInfoCanHookDecline charInfoCanHookDecline)
			{
				m_CanHookDeclineSubscription = charInfoCanHookDecline.GetCanHookDeclineProperty().Subscribe(delegate(bool value)
				{
					m_CanDecline.Value = !value;
				});
			}
			else
			{
				m_CanDecline.Value = true;
			}
		}
		m_NavigationPanelLeft.FocusOnEntityManual(m_NavigationPanelLeft.Entities.LastOrDefault());
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelLeft);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelRight);
		m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight);
	}

	protected override void OnProgressionWindowStateChange(UnitProgressionWindowState state)
	{
		m_ShowTooltip.Value = m_ShowTooltip.Value && state != UnitProgressionWindowState.CareerPathProgression;
	}

	private void ToggleTooltip()
	{
		TooltipHelper.HideTooltip();
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		if (m_ProgressionView.IsBinded && m_ProgressionView.CurrentState == UnitProgressionWindowState.CareerPathProgression && m_ProgressionView is ICharInfoCanHookConfirm charInfoCanHookConfirm)
		{
			m_CanConfirm.Value = charInfoCanHookConfirm.GetCanHookConfirmProperty().Value;
		}
		string text = entity?.GetConfirmClickHint();
		m_ConfirmHint.SetLabel((!string.IsNullOrEmpty(text)) ? text : ((string)UIStrings.Instance.CommonTexts.Select));
		TooltipConfig config = (m_RightPanelSelected.Value ? m_RightPanelConfig : m_LeftPanelConfig);
		if (!(entity is IHasTooltipTemplate hasTooltipTemplate))
		{
			m_HasTooltip.Value = false;
			return;
		}
		TooltipBaseTemplate tooltipBaseTemplate = hasTooltipTemplate.TooltipTemplate();
		if (tooltipBaseTemplate is TooltipTemplateGlossary { GlossaryEntry: not null })
		{
			config.IsGlossary = true;
		}
		m_HasTooltip.Value = tooltipBaseTemplate != null && (m_LeftPanelSelected.Value || !m_ProgressionView.IsBinded || m_ProgressionView.CurrentState == UnitProgressionWindowState.CareerPathList);
		if (m_ShowTooltip.Value)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowConsoleTooltip(tooltipBaseTemplate, m_NavigationBehaviour, config);
		}
	}

	public void OnClick()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}, 3);
	}

	private void ToggleLeftPanelFocus()
	{
		m_LeftPanelSelected.Value = m_NavigationPanelLeft.IsFocused;
	}

	private void ToggleRightCanvasFocus(IConsoleEntity entity)
	{
		if (entity == null && !base.ViewModel.PageCanHaveNoEntities)
		{
			m_RightPanelSelected.Value = false;
			return;
		}
		m_RightPanelSelected.Value = !m_IgnoreRightPanels.SelectMany((GridConsoleNavigationBehaviour nb) => nb.NestedFocuses).Contains(entity);
	}

	private void Close()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
		TooltipHelper.HideTooltip();
	}

	public void HandleFocus()
	{
		OnClick();
	}

	public void HandleRemoveFocus()
	{
		m_PrevNavigation = (m_LeftPanelSelected.Value ? m_NavigationPanelLeft : m_NavigationPanelRight);
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_PrevNavigation.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.FocusOnEntityManual(m_PrevNavigation);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}
}
