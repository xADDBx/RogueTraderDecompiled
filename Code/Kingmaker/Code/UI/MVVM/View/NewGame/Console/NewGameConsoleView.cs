using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGameConsoleView : NewGameBaseView
{
	[Header("Views")]
	[SerializeField]
	private NewGamePhaseStoryConsoleView m_NewGamePhaseStoryConsoleView;

	[SerializeField]
	private NewGamePhaseDifficultyConsoleView m_NewGamePhaseDifficultyConsoleView;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_SwitchOnOffDlcHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_InstallDlcHint;

	[SerializeField]
	private ConsoleHint m_DeleteDlcHint;

	[SerializeField]
	private ConsoleHint m_PlayPauseVideoHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private InputLayer m_GlossaryInputLayer;

	private GridConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private readonly BoolReactiveProperty m_GlossaryMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasGlossary = new BoolReactiveProperty();

	private TooltipConfig m_TooltipConfig;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_NewGamePhaseDifficultyConsoleView.Initialize();
		m_Selector.Initialize();
		m_NewGamePhaseStoryConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_NewGamePhaseStoryConsoleView.Bind(base.ViewModel.StoryVM);
		m_NewGamePhaseDifficultyConsoleView.Bind(base.ViewModel.DifficultyVM);
		m_TooltipConfig.IsGlossary = true;
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_GlossaryNavigationBehavior = new GridConsoleNavigationBehaviour());
		AddDisposable(m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusedChanged));
		CreateInput();
		UpdateNavigation();
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ChangeTab, delegate
		{
			UpdateNavigation();
		}));
		AddDisposable(m_NewGamePhaseDifficultyConsoleView.ReactiveTooltipTemplate.Subscribe(delegate
		{
			CalculateGlossary();
		}));
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		if (base.ViewModel.SelectedMenuEntity.Value.NewGamePhaseVM == base.ViewModel.StoryVM)
		{
			base.ViewModel.StoryVM.OnSelected();
			m_NavigationBehaviour.SetEntitiesVertical(m_NewGamePhaseStoryConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_NewGamePhaseStoryConsoleView.ScrollToTop();
			m_HasGlossary.Value = false;
		}
		else
		{
			VirtualListVertical virtualList = m_NewGamePhaseDifficultyConsoleView.VirtualList;
			m_NavigationBehaviour.SetEntitiesVertical<GridConsoleNavigationBehaviour>(virtualList.GetNavigationBehaviour());
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_NewGamePhaseDifficultyConsoleView.UpdateFirstFocus();
			}, 1);
			m_NewGamePhaseDifficultyConsoleView.StrollInfoViewToTop();
			virtualList.ScrollController.ForceScrollToTop();
		}
		m_InputLayer.Bind();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NewGame"
		});
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "NewGameSettingsGlossary"
		});
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_ConfirmHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonNext();
		}, 8, base.ViewModel.StoryVM.IsNextButtonAvailable)));
		m_ConfirmHint.SetLabel(UIStrings.Instance.CharGen.Next);
		AddDisposable(m_DeclineHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonBack();
		}, 9)));
		m_DeclineHint.SetLabel(UIStrings.Instance.CharGen.Back);
		AddDisposable(m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonBack();
		}, 14)));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonNext();
		}, 15, base.ViewModel.StoryVM.IsNextButtonAvailable)));
		AddDisposable(m_CommonHintsWidget.BindHint(inputLayer.AddButton(ShowGlossary, 11, m_HasGlossary, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.OpenGlossary));
		AddDisposable(m_GlossaryInputLayer.AddAxis(ScrollDescription, 3, repeat: true));
		AddDisposable(m_CommonHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, m_GlossaryMode), UIStrings.Instance.Dialog.CloseGlossary));
		AddDisposable(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 11, m_GlossaryMode, InputActionEventType.ButtonJustReleased));
		m_NewGamePhaseStoryConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_SwitchOnOffDlcHint, m_PurchaseHint, m_InstallDlcHint, m_DeleteDlcHint, m_PlayPauseVideoHint);
		m_NewGamePhaseDifficultyConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget);
	}

	private void ShowGlossary(InputActionEventData data)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		(m_NavigationBehaviour.CurrentEntity as ExpandableElement)?.SetCustomLayer("On");
		m_GlossaryMode.Value = true;
		AddDisposable(GamePad.Instance.PushLayer(m_GlossaryInputLayer));
		CalculateGlossary();
		m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseGlossary()
	{
		TooltipHelper.HideTooltip();
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_GlossaryInputLayer);
		m_GlossaryMode.Value = false;
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void CalculateGlossary()
	{
		if (m_GlossaryNavigationBehavior != null)
		{
			m_GlossaryNavigationBehavior.Clear();
			List<IConsoleEntity> entities = m_NewGamePhaseDifficultyConsoleView.InfoView.GetNavigationBehaviour().Entities.Where((IConsoleEntity e) => e is FloatConsoleNavigationBehaviour).ToList();
			m_GlossaryNavigationBehavior.AddColumn(entities);
			m_HasGlossary.Value = m_GlossaryNavigationBehavior != null && m_GlossaryNavigationBehavior.Entities.Any();
			if (m_GlossaryMode.Value)
			{
				TooltipHelper.HideTooltip();
			}
		}
	}

	private void ShowTooltip()
	{
		IConsoleEntity value = m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Value;
		MonoBehaviour component = (value as MonoBehaviour) ?? (value as IMonoBehaviour)?.MonoBehaviour;
		TooltipBaseTemplate template = (value as IHasTooltipTemplate)?.TooltipTemplate();
		component.ShowConsoleTooltip(template, m_GlossaryNavigationBehavior, m_TooltipConfig);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (base.ViewModel.SelectedMenuEntity.Value.NewGamePhaseVM == base.ViewModel.StoryVM)
		{
			m_NewGamePhaseStoryConsoleView.Scroll(obj, value);
		}
		else
		{
			ScrollDescription(obj, value);
		}
	}

	private void ScrollDescription(InputActionEventData obj, float value)
	{
		m_NewGamePhaseDifficultyConsoleView.Scroll(obj, value);
	}

	public void OnGlossaryFocusedChanged(IConsoleEntity entity)
	{
		MonoBehaviour monoBehaviour = ((!(entity is TMPLinkNavigationEntity tMPLinkNavigationEntity)) ? null : tMPLinkNavigationEntity.MonoBehaviour);
		MonoBehaviour monoBehaviour2 = monoBehaviour;
		if (monoBehaviour2 != null)
		{
			m_NewGamePhaseDifficultyConsoleView.InfoView.ScrollRectExtended.EnsureVisibleVertical(monoBehaviour2.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
		ShowTooltip();
	}
}
