using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Common.InputField;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console;

public class SettingsConsoleView : ViewBase<SettingsVM>, IInitializable
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderConsoleView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolConsoleView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownConsoleView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyConsoleView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntitySliderGammaCorrectionConsoleView m_SettingsEntitySliderGammaCorrectionViewPrefab;

		[SerializeField]
		private SettingsEntityStatisticsOptOutConsoleView m_SettingsEntityStatisticsOptOutViewPrefab;

		[SerializeField]
		private SettingEntityKeyBindingConsoleView m_SettingEntityKeyBindingViewPrefab;

		[SerializeField]
		private SettingsEntityDisplayImagesConsoleView m_SettingEntityDisplayImagesViewPrefab;

		[SerializeField]
		private SettingsEntityAccessibilityImageConsoleView m_SettingEntityAccessibilityImageViewPrefab;

		[SerializeField]
		private SettingsEntitySliderFontSizeConsoleView m_SettingEntityFontSizeViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderGammaCorrectionViewPrefab, 1), new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(m_SettingsEntityStatisticsOptOutViewPrefab), new VirtualListElementTemplate<SettingsEntityDisplayImagesVM>(m_SettingEntityDisplayImagesViewPrefab), new VirtualListElementTemplate<SettingsEntityAccessibilityImageVM>(m_SettingEntityAccessibilityImageViewPrefab), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingEntityFontSizeViewPrefab, 2));
		}
	}

	[SerializeField]
	private SettingsViews m_SettingsViews;

	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private RectTransform m_BigGreenScreenView;

	[SerializeField]
	private RectTransform m_LittleGreenScreenView;

	[SerializeField]
	private RectTransform m_PaperGroup;

	[SerializeField]
	private RectTransform m_ScrollBarObject;

	[SerializeField]
	private SettingsMenuSelectorConsoleView m_MenuSelector;

	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	[Header("Input")]
	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Controls")]
	[SerializeField]
	private GameObject m_ControlsConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_PSConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_DualSenseConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_XBoxConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SwitchConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SteamConsoleGroup;

	[SerializeField]
	private SettingsControlConsole m_SteamDeckConsoleGroup;

	[Header("SafeZone")]
	[SerializeField]
	private RectTransform m_SafeZoneFrame;

	private bool m_IsShowed;

	private InputBindStruct m_ResetToDefaultStruct;

	private InputBindStruct m_ConfirmStruct;

	private InputBindStruct m_DeclineStruct;

	private readonly BoolReactiveProperty m_IsVisibleConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsVisibleResetToDefault = new BoolReactiveProperty();

	private GridConsoleNavigationBehaviour m_NavigationBehavior;

	private InputLayer m_GlossaryInputLayer;

	private GridConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private readonly BoolReactiveProperty m_GlossaryMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasGlossary = new BoolReactiveProperty();

	private TooltipConfig m_TooltipConfig;

	private IConsoleHint m_ResetToDefaultHint;

	private IConsoleHint m_ConfirmHint;

	private IConsoleHint m_DeclineHint;

	private InputLayer m_SettingsInputLayer;

	public static readonly string SettingsInputLayerName = "SettingsConsoleViewInput";

	public static readonly string GlossarySettingsInputLayerName = "SettingsGlossary";

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_MenuSelector.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		m_SelectorView.Bind(base.ViewModel.Selector);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		m_TooltipConfig.IsGlossary = true;
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.SettingEntities));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnSwitchSettings, delegate
		{
			OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.Value);
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnApplyWindowClose, delegate
		{
			DelayedWindowClose();
		}));
		AddDisposable(base.ViewModel.IsConsoleControls.Subscribe(delegate(bool value)
		{
			m_ScrollBarObject.gameObject.SetActive(!value);
		}));
		AddDisposable(m_GlossaryNavigationBehavior = new GridConsoleNavigationBehaviour());
		AddDisposable(m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusedChanged));
		m_NavigationBehavior = m_VirtualList.GetNavigationBehaviour();
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer(m_NavigationBehavior)));
		GamePad.Instance.BaseLayer?.Unbind();
		AddDisposable(ObservableExtensions.Subscribe(m_VirtualList.AttachedFirstValidView, delegate
		{
			FocusOnValidEntity(m_NavigationBehavior);
		}));
		AddDisposable(base.ViewModel.ReactiveTooltipTemplate.Subscribe(delegate
		{
			CalculateGlossary();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}));
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged += OnSafeZoneChanged;
		OnSafeZoneChanged(SettingsRoot.Display.SafeZoneOffset.GetValue());
		OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.Value);
		BindHints();
		Show();
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	private void FocusOnValidEntity(GridConsoleNavigationBehaviour navigationBehavior)
	{
		foreach (IConsoleEntity entity in navigationBehavior.Entities)
		{
			if (entity.IsValid())
			{
				navigationBehavior.FocusOnEntityManual(entity);
				return;
			}
		}
		navigationBehavior.FocusOnFirstValidEntity();
	}

	public void OnSelectedMenuEntity(SettingsMenuEntityVM entity)
	{
		m_InfoView.gameObject.SetActive(!base.ViewModel.IsConsoleControls.Value);
		m_PaperGroup.gameObject.SetActive(!base.ViewModel.IsConsoleControls.Value);
		m_LittleGreenScreenView.gameObject.SetActive(!base.ViewModel.IsConsoleControls.Value);
		m_VirtualList.gameObject.SetActive(!base.ViewModel.IsConsoleControls.Value);
		m_BigGreenScreenView.gameObject.SetActive(base.ViewModel.IsConsoleControls.Value);
		m_ControlsConsoleGroup.SetActive(base.ViewModel.IsConsoleControls.Value);
		m_IsVisibleResetToDefault.Value = !base.ViewModel.IsConsoleControls.Value && base.ViewModel.IsDefaultButtonInteractable.Value;
		m_SafeZoneFrame.gameObject.SetActive(entity.SettingsScreenType == UISettingsManager.SettingsScreen.Display);
		int index = base.ViewModel.MenuEntitiesList.IndexOf(base.ViewModel.MenuEntitiesList.FirstOrDefault((SettingsMenuEntityVM e) => e == base.ViewModel.SelectedMenuEntity.Value));
		m_SelectorView.ChangeTab(index);
		if (base.ViewModel.IsConsoleControls.Value)
		{
			SetupConsoleControls();
		}
	}

	private void OnApplyWindowClose()
	{
		SettingsVM viewModel = base.ViewModel;
		if (viewModel != null && viewModel.IsConsoleControls.Value)
		{
			SetupConsoleControls();
		}
	}

	private void DelayedWindowClose()
	{
		DelayedInvoker.InvokeInFrames(OnApplyWindowClose, 1);
	}

	private void SetupConsoleControls()
	{
		m_DualSenseConsoleGroup.gameObject.SetActive(value: false);
		m_PSConsoleGroup.gameObject.SetActive(value: false);
		m_XBoxConsoleGroup.gameObject.SetActive(value: false);
		m_SwitchConsoleGroup.gameObject.SetActive(value: false);
		m_SteamConsoleGroup.gameObject.SetActive(value: false);
		m_SteamDeckConsoleGroup.gameObject.SetActive(value: false);
		SettingsControlConsole settingsControlConsole = null;
		switch (GamePad.Instance.Type)
		{
		case ConsoleType.PS4:
			settingsControlConsole = m_PSConsoleGroup;
			m_PSConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.PS5:
			settingsControlConsole = m_DualSenseConsoleGroup;
			m_DualSenseConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.XBox:
			settingsControlConsole = m_XBoxConsoleGroup;
			m_XBoxConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.Switch:
			settingsControlConsole = m_SwitchConsoleGroup;
			m_SwitchConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.SteamController:
			settingsControlConsole = m_SteamConsoleGroup;
			m_SteamConsoleGroup.gameObject.SetActive(value: true);
			break;
		case ConsoleType.SteamDeck:
			settingsControlConsole = m_SteamDeckConsoleGroup;
			m_SteamDeckConsoleGroup.gameObject.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ConsoleType.Common:
			break;
		}
		if (!(settingsControlConsole == null))
		{
			InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
			AddDisposable(settingsControlConsole.LeftStickButtonHint.BindCustomAction(18, currentInputLayer));
			settingsControlConsole.LeftStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftStickButtonHint);
			settingsControlConsole.LeftStickButtonHint.SetActive(state: true);
			AddDisposable(settingsControlConsole.DPadRightHint.BindCustomAction(5, currentInputLayer));
			settingsControlConsole.DPadRightHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadRightHint);
			AddDisposable(settingsControlConsole.DPadDownHint.BindCustomAction(7, currentInputLayer));
			settingsControlConsole.DPadDownHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadDownHint);
			AddDisposable(settingsControlConsole.DPadLeftHint.BindCustomAction(4, currentInputLayer));
			settingsControlConsole.DPadLeftHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadLeftHint);
			AddDisposable(settingsControlConsole.DPadUpHint.BindCustomAction(6, currentInputLayer));
			settingsControlConsole.DPadUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDPadUpHint);
			AddDisposable(settingsControlConsole.LeftUpHint.BindCustomAction(14, currentInputLayer));
			settingsControlConsole.LeftUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftUpHint);
			AddDisposable(settingsControlConsole.LeftBottomHint.BindCustomAction(12, currentInputLayer));
			settingsControlConsole.LeftBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlLeftBottomHint);
			AddDisposable(settingsControlConsole.FuncAdditionalHint.BindCustomAction(17, currentInputLayer));
			settingsControlConsole.FuncAdditionalHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFuncAdditionalHint);
			AddDisposable(settingsControlConsole.RightBottomHint.BindCustomAction(13, currentInputLayer));
			settingsControlConsole.RightBottomHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightBottomHint);
			AddDisposable(settingsControlConsole.RightUpHint.BindCustomAction(15, currentInputLayer));
			settingsControlConsole.RightUpHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightUpHint);
			AddDisposable(settingsControlConsole.OptionsHint.BindCustomAction(16, currentInputLayer));
			settingsControlConsole.OptionsHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlOptionsHint);
			AddDisposable(settingsControlConsole.Func02Hint.BindCustomAction(11, currentInputLayer));
			settingsControlConsole.Func02Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc02Hint);
			AddDisposable(settingsControlConsole.DeclineHint.BindCustomAction(9, currentInputLayer));
			settingsControlConsole.DeclineHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlDeclineHint);
			AddDisposable(settingsControlConsole.ConfirmHint.BindCustomAction(8, currentInputLayer));
			settingsControlConsole.ConfirmHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlConfirmHint);
			AddDisposable(settingsControlConsole.Func01Hint.BindCustomAction(10, currentInputLayer));
			settingsControlConsole.Func01Hint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlFunc01Hint);
			AddDisposable(settingsControlConsole.RightStickButtonHint.BindCustomAction(19, currentInputLayer));
			settingsControlConsole.RightStickButtonHint.SetLabel(UIStrings.Instance.SettingsUI.ConsoleControlRightStickButtonHint);
			bool isActive = PhotonManager.Lobby.IsActive;
			settingsControlConsole.ConsoleCoopPingHint.gameObject.SetActive(isActive);
			if (isActive)
			{
				settingsControlConsole.ConsoleCoopPingHint.text = UIStrings.Instance.SettingsUI.ConsoleControlPingCoopHint;
			}
		}
	}

	private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
	{
		m_SettingsInputLayer = navigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = SettingsInputLayerName
		});
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = GlossarySettingsInputLayerName
		});
		AddDisposable(m_DeclineStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9));
		AddDisposable(m_ConfirmStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.OpenApplySettingsDialog();
		}, 8, m_IsVisibleConfirm));
		AddDisposable(m_ResetToDefaultStruct = m_SettingsInputLayer.AddButton(delegate
		{
			base.ViewModel.OpenDefaultSettingsDialog();
		}, 11, m_IsVisibleResetToDefault, InputActionEventType.ButtonJustLongPressed));
		AddDisposable(m_SettingsInputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_PrevHint.Bind(m_SettingsInputLayer.AddButton(delegate
		{
			m_MenuSelector.OnPrev();
		}, 14)));
		AddDisposable(m_NextHint.Bind(m_SettingsInputLayer.AddButton(delegate
		{
			m_MenuSelector.OnNext();
		}, 15)));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_SettingsInputLayer.AddButton(ShowGlossary, 11, m_HasGlossary, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.OpenGlossary));
		AddDisposable(m_GlossaryInputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, m_GlossaryMode), UIStrings.Instance.Dialog.CloseGlossary));
		AddDisposable(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 11, m_GlossaryMode, InputActionEventType.ButtonJustReleased));
		return m_SettingsInputLayer;
	}

	private void ShowGlossary(InputActionEventData data)
	{
		m_NavigationBehavior.UnFocusCurrentEntity();
		(m_NavigationBehavior.CurrentEntity as ExpandableElement)?.SetCustomLayer("On");
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
		m_NavigationBehavior.FocusOnCurrentEntity();
	}

	private void CalculateGlossary()
	{
		if (m_GlossaryNavigationBehavior != null)
		{
			m_GlossaryNavigationBehavior.Clear();
			List<IConsoleEntity> entities = m_InfoView.GetNavigationBehaviour().Entities.Where((IConsoleEntity e) => e is FloatConsoleNavigationBehaviour).ToList();
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
		m_InfoView.Scroll(value);
	}

	private void Update()
	{
		bool flag = SettingsController.Instance.HasUnconfirmedSettings();
		if (m_IsVisibleConfirm.Value != flag)
		{
			m_IsVisibleConfirm.Value = flag;
		}
	}

	protected virtual void BindHints()
	{
		AddDisposable(m_ResetToDefaultHint = m_ConsoleHintsWidget.BindHint(m_ResetToDefaultStruct));
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		AddDisposable(m_ConfirmHint = m_ConsoleHintsWidget.BindHint(m_ConfirmStruct, "", ConsoleHintsWidget.HintPosition.Right));
		m_ConfirmHint.SetLabel(UIStrings.Instance.CommonTexts.Accept);
		AddDisposable(m_DeclineHint = m_ConsoleHintsWidget.BindHint(m_DeclineStruct, "", ConsoleHintsWidget.HintPosition.Left));
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.BaseLayer?.Bind();
		Hide();
		m_ConsoleHintsWidget.Dispose();
		SettingsRoot.Display.SafeZoneOffset.OnTempValueChanged -= OnSafeZoneChanged;
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			Game.Instance.RequestPauseUi(isPaused: true);
			m_Animator.AppearAnimation();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
		}
	}

	public void Hide()
	{
		if (m_IsShowed)
		{
			m_Animator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
			Game.Instance.RequestPauseUi(isPaused: false);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.LocalMap.MapClose.Play();
		}
	}

	private void OnSafeZoneChanged(int value)
	{
		float num = (float)value / 200f;
		Rect rect = MainCanvas.Instance.RectTransform.rect;
		m_SafeZoneFrame.offsetMin = new Vector2(rect.width * num, rect.height * num);
		m_SafeZoneFrame.offsetMax = new Vector2((0f - rect.width) * num, (0f - rect.height) * num);
	}

	public void OnGlossaryFocusedChanged(IConsoleEntity entity)
	{
		MonoBehaviour monoBehaviour = ((!(entity is TMPLinkNavigationEntity tMPLinkNavigationEntity)) ? null : tMPLinkNavigationEntity.MonoBehaviour);
		MonoBehaviour monoBehaviour2 = monoBehaviour;
		if (monoBehaviour2 != null)
		{
			m_InfoView.ScrollRectExtended.EnsureVisibleVertical(monoBehaviour2.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
		ShowTooltip();
	}

	private void UpdateLocalization()
	{
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		m_ConfirmHint.SetLabel(UIStrings.Instance.CommonTexts.Accept);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != m_SettingsInputLayer && instance.CurrentInputLayer != m_GlossaryInputLayer && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == InfoWindowConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.LabelsDisposableString) && !(instance.CurrentInputLayer.ContextName == "BugReportDuplicatesViewInput") && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName))
		{
			instance.PopLayer(m_SettingsInputLayer);
			instance.PushLayer(m_SettingsInputLayer);
			if (m_GlossaryMode.Value)
			{
				instance.PopLayer(m_GlossaryInputLayer);
				instance.PushLayer(m_GlossaryInputLayer);
			}
		}
	}
}
