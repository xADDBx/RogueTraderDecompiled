using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.KeyBindSetupDialog;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Menu;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC;

public class SettingsPCView : ViewBase<SettingsVM>, IInitializable
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolPCView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownPCView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderPCView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyPCView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderGammaCorrectionViewPrefab;

		[SerializeField]
		private SettingsEntityStatisticsOptOutPCView m_SettingsEntityStatisticsOptOutViewPrefab;

		[SerializeField]
		private SettingEntityKeyBindingPCView m_SettingEntityKeyBindingViewPrefab;

		[SerializeField]
		private SettingsEntityDisplayImagesPCView m_SettingEntityDisplayImagesViewPrefab;

		[SerializeField]
		private SettingsEntityAccessibilityImagePCView m_SettingEntityAccessibilityImageViewPrefab;

		[SerializeField]
		private SettingsEntitySliderFontSizePCView m_SettingEntityFontSizeViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSavePCView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingEntityKeyBindingVM>(m_SettingEntityKeyBindingViewPrefab), new VirtualListElementTemplate<SettingsEntityDisplayImagesVM>(m_SettingEntityDisplayImagesViewPrefab), new VirtualListElementTemplate<SettingsEntityAccessibilityImageVM>(m_SettingEntityAccessibilityImageViewPrefab), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingEntityFontSizeViewPrefab, 2), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderGammaCorrectionViewPrefab, 1), new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(m_SettingsEntityStatisticsOptOutViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[SerializeField]
	private SettingsViews m_SettingsViews;

	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsMenuSelectorPCView m_MenuSelector;

	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private KeyBindingSetupDialogPCView m_KeyBindingSetupDialogView;

	[Header("Buttons")]
	[SerializeField]
	public OwlcatButton m_CloseButton;

	[SerializeField]
	public OwlcatButton m_DefaultButton;

	[SerializeField]
	public TextMeshProUGUI m_DefaultText;

	[SerializeField]
	public OwlcatButton m_CancelButton;

	[SerializeField]
	public TextMeshProUGUI m_CancelText;

	[SerializeField]
	public OwlcatButton m_ApplyButton;

	[SerializeField]
	public TextMeshProUGUI m_ApplyText;

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_Animator;

	private bool m_IsShowed;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_MenuSelector.Initialize();
		m_KeyBindingSetupDialogView.Initialize();
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
	}

	protected override void BindViewImplementation()
	{
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		m_SelectorView.Bind(base.ViewModel.Selector);
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.SettingEntities));
		AddDisposable(base.ViewModel.CurrentKeyBindDialog.Subscribe(OpenKeyBindSetupDialog));
		SetButtonsSounds();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(m_DefaultButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenDefaultSettingsDialog));
		AddDisposable(m_CancelButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenCancelSettingsDialog));
		AddDisposable(m_ApplyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OpenApplySettingsDialog));
		AddDisposable(base.ViewModel.IsApplyButtonInteractable.Subscribe(delegate(bool value)
		{
			m_ApplyButton.Interactable = value;
		}));
		AddDisposable(base.ViewModel.IsCancelButtonInteractable.Subscribe(delegate(bool value)
		{
			m_CancelButton.Interactable = value;
		}));
		AddDisposable(base.ViewModel.IsDefaultButtonInteractable.Subscribe(delegate(bool value)
		{
			m_DefaultButton.Interactable = value;
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}));
		SetBottomButtonsTexts();
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnSwitchSettings, delegate
		{
			OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.Value);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, delegate
		{
			m_MenuSelector.OnPrev();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, delegate
		{
			m_MenuSelector.OnNext();
		}));
		Show();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_DefaultButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CancelButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ApplyButton, UISounds.ButtonSoundsEnum.PlastickSound);
	}

	private void OpenKeyBindSetupDialog(KeyBindingSetupDialogVM dialogVM)
	{
		if (dialogVM != null)
		{
			m_KeyBindingSetupDialogView.Bind(dialogVM);
		}
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

	private void SetBottomButtonsTexts()
	{
		m_DefaultText.text = UIStrings.Instance.SettingsUI.Default;
		m_CancelText.text = UIStrings.Instance.SettingsUI.Cancel;
		m_ApplyText.text = UIStrings.Instance.SettingsUI.Apply;
	}

	private void UpdateLocalization()
	{
		SetBottomButtonsTexts();
	}

	public void OnSelectedMenuEntity(SettingsMenuEntityVM entity)
	{
		int index = base.ViewModel.MenuEntitiesList.IndexOf(base.ViewModel.MenuEntitiesList.FirstOrDefault((SettingsMenuEntityVM e) => e == base.ViewModel.SelectedMenuEntity.Value));
		m_SelectorView.ChangeTab(index);
	}
}
