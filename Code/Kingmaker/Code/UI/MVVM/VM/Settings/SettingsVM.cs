using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SafeZone;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings;

public class SettingsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IKeyBindingSetupDialogHandler, ISubscriber, ISettingsDescriptionUIHandler, IOptionsWindowUIHandler, ISaveSettingsHandler, ISettingsFontSizeUIHandler, ILocalizationHandler
{
	private readonly Action m_CloseAction;

	private readonly bool m_IsMainMenu;

	public readonly SelectionGroupRadioVM<SettingsMenuEntityVM> SelectionGroup;

	public readonly LensSelectorVM Selector;

	private readonly ReactiveProperty<SettingsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<SettingsMenuEntityVM>();

	public readonly List<SettingsMenuEntityVM> MenuEntitiesList = new List<SettingsMenuEntityVM>();

	public readonly InfoSectionVM InfoVM;

	public readonly ReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveCollection<VirtualListElementVMBase> m_SettingEntities = new ReactiveCollection<VirtualListElementVMBase>();

	private readonly ReactiveProperty<KeyBindingSetupDialogVM> m_CurrentKeyBindDialog = new ReactiveProperty<KeyBindingSetupDialogVM>();

	public readonly BoolReactiveProperty IsDefaultButtonInteractable = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsApplyButtonInteractable = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsCancelButtonInteractable = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsConsoleControls = new BoolReactiveProperty();

	public readonly ReactiveCommand OnApplyWindowClose = new ReactiveCommand();

	public readonly ReactiveCommand OnSwitchSettings = new ReactiveCommand();

	private SettingsMenuEntityVM m_PreviousSelectedMenuEntity;

	public readonly ReactiveCommand LanguageChanged = new ReactiveCommand();

	public ReactiveProperty<SettingsMenuEntityVM> SelectedMenuEntity => m_SelectedMenuEntity;

	public IReadOnlyReactiveCollection<VirtualListElementVMBase> SettingEntities => m_SettingEntities;

	public IReadOnlyReactiveProperty<KeyBindingSetupDialogVM> CurrentKeyBindDialog => m_CurrentKeyBindDialog;

	public SettingsVM(Action closeAction, bool isMainMenu = false)
	{
		UISettingsRoot.Instance.UIGraphicsSettings.UpdateInteractable(initialize: true);
		UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		m_CloseAction = closeAction;
		m_IsMainMenu = isMainMenu;
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGame, UISettingsManager.SettingsScreen.Game);
		if (!isMainMenu)
		{
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDifficulty, UISettingsManager.SettingsScreen.Difficulty);
		}
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameControls, UISettingsManager.SettingsScreen.Controls);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGraphics, UISettingsManager.SettingsScreen.Graphics);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDisplay, UISettingsManager.SettingsScreen.Display);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSound, UISettingsManager.SettingsScreen.Sound);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameAccessiability, UISettingsManager.SettingsScreen.Accessiability);
		AddDisposable(SelectionGroup = new SelectionGroupRadioVM<SettingsMenuEntityVM>(MenuEntitiesList, m_SelectedMenuEntity, cyclical: true));
		AddDisposable(Selector = new LensSelectorVM());
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			ApplySettings();
		}
		m_SelectedMenuEntity.Value = MenuEntitiesList.FirstOrDefault();
		UISettingsManager.SettingsScreen settingsScreen = m_SelectedMenuEntity.Value?.SettingsScreenType ?? UISettingsManager.SettingsScreen.Game;
		SetSettingsList(settingsScreen);
		if (settingsScreen == UISettingsManager.SettingsScreen.Game)
		{
			UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		}
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
		AddDisposable(ObservableExtensions.Subscribe(LanguageChanged, delegate
		{
			InfoVM.SetTemplate(ReactiveTooltipTemplate.Value);
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		SettingsController.Instance.RevertAllTempValues();
		DisposeEntities();
		HandleHideSettingsDescription();
	}

	private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
	{
		SettingsMenuEntityVM settingsMenuEntityVM = new SettingsMenuEntityVM(localizedString, screenType, SetSettingsList);
		AddDisposable(settingsMenuEntityVM);
		MenuEntitiesList.Add(settingsMenuEntityVM);
	}

	private void SetSettingsList(UISettingsManager.SettingsScreen settingsScreen)
	{
		if (m_PreviousSelectedMenuEntity == m_SelectedMenuEntity.Value)
		{
			return;
		}
		IsDefaultButtonInteractable.Value = settingsScreen != UISettingsManager.SettingsScreen.Difficulty && settingsScreen != UISettingsManager.SettingsScreen.Graphics && RootUIContext.CanChangeLanguage() && RootUIContext.CanChangeInput();
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(delegate(DialogMessageBoxBase.BoxButton button)
			{
				OnApplyDialogAnswer(button);
				SwitchSettingsScreen(settingsScreen);
			});
		}
		else
		{
			SwitchSettingsScreen(settingsScreen);
		}
		m_PreviousSelectedMenuEntity = m_SelectedMenuEntity.Value;
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		DisposeEntities();
		IsConsoleControls.Value = Game.Instance.IsControllerGamepad && settingsScreen == UISettingsManager.SettingsScreen.Controls;
		foreach (UISettingsGroup item in from uiSettingsGroup in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen)
			where uiSettingsGroup.IsVisible
			select uiSettingsGroup)
		{
			m_SettingEntities.Add(new SettingsEntityHeaderVM(item.Title));
			foreach (UISettingsEntityBase visibleSettings in item.VisibleSettingsList)
			{
				m_SettingEntities.Add(GetVMForSettingsItem(visibleSettings));
			}
		}
		using (IEnumerator<UISettingsGroup> enumerator = (from uiSettingsGroup in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen)
			where uiSettingsGroup.IsVisible
			select uiSettingsGroup).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				using IEnumerator<UISettingsEntityBase> enumerator3 = enumerator.Current.VisibleSettingsList.Where((UISettingsEntityBase uiSettingsEntityBase) => !(uiSettingsEntityBase is UISettingsEntityDisplayImages) && !(uiSettingsEntityBase is UISettingsEntityAccessiabilityImage)).GetEnumerator();
				if (enumerator3.MoveNext())
				{
					UISettingsEntityBase current3 = enumerator3.Current;
					HandleShowSettingsDescription(current3);
				}
			}
		}
		OnSwitchSettings.Execute();
		if (settingsScreen == UISettingsManager.SettingsScreen.Game)
		{
			UISettingsRoot.Instance.UIGameMainSettings.UpdateInteractable();
		}
	}

	private void DisposeEntities()
	{
		m_SettingEntities.ForEach(delegate(VirtualListElementVMBase s)
		{
			s.Dispose();
		});
		m_SettingEntities.Clear();
	}

	public static VirtualListElementVMBase GetVMForSettingsItem(UISettingsEntityBase uiSettingsEntity, bool isNewGame = false)
	{
		if (!(uiSettingsEntity is UISettingsEntityGameDifficulty uiSettingsEntity2))
		{
			if (!(uiSettingsEntity is UISettingsEntityGammaCorrection uiSettingsEntity3))
			{
				if (!(uiSettingsEntity is UISettingsEntitySliderFontSize uiSettingsEntity4))
				{
					if (!(uiSettingsEntity is UISettingsEntityOptOut uiSettingsEntity5))
					{
						if (!(uiSettingsEntity is UISettingsEntityBool uiSettingsEntity6))
						{
							if (!(uiSettingsEntity is IUISettingsEntityDropdown uiSettingsEntity7))
							{
								if (!(uiSettingsEntity is IUISettingsEntitySlider uiSettingsEntity8))
								{
									if (!(uiSettingsEntity is UISettingsEntityKeyBinding uiSettingsEntity9))
									{
										if (!(uiSettingsEntity is UISettingsEntityDisplayImages uiSettingsEntity10))
										{
											if (!(uiSettingsEntity is UISettingsEntityAccessiabilityImage uiSettingsEntity11))
											{
												if (uiSettingsEntity is UISettingsEntityBoolOnlyOneSave uiSettingsEntity12)
												{
													return new SettingsEntityBoolOnlyOneSaveVM(uiSettingsEntity12, isNewGame);
												}
												UberDebug.LogError($"Error: SettingsVM: GetVMForSettingsItem: uiSettingsEntity {uiSettingsEntity} is undefined");
												return null;
											}
											return new SettingsEntityAccessibilityImageVM(uiSettingsEntity11);
										}
										return new SettingsEntityDisplayImagesVM(uiSettingsEntity10);
									}
									return new SettingEntityKeyBindingVM(uiSettingsEntity9, isNewGame);
								}
								return new SettingsEntitySliderVM(uiSettingsEntity8, SettingsEntitySliderVM.EntitySliderType.UsualSliderIndex, isNewGame);
							}
							return new SettingsEntityDropdownVM(uiSettingsEntity7, SettingsEntityDropdownVM.DropdownType.Default, isNewGame);
						}
						return new SettingsEntityBoolVM(uiSettingsEntity6, isNewGame);
					}
					return new SettingsEntityStatisticsOptOutVM(uiSettingsEntity5);
				}
				return new SettingsEntitySliderVM(uiSettingsEntity4, SettingsEntitySliderVM.EntitySliderType.FontSizeIndex, isNewGame);
			}
			return new SettingsEntitySliderVM(uiSettingsEntity3, SettingsEntitySliderVM.EntitySliderType.GammaCorrectionSliderIndex, isNewGame);
		}
		return new SettingsEntityDropdownGameDifficultyVM(uiSettingsEntity2, forceSetValue: false, isNewGame);
	}

	public void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex)
	{
		m_CurrentKeyBindDialog.Value = new KeyBindingSetupDialogVM(uiSettingsEntity, bindingIndex, CloseKeyBindSetupDialog);
		AddDisposable(m_CurrentKeyBindDialog.Value);
	}

	private void CloseKeyBindSetupDialog()
	{
		RemoveDisposable(m_CurrentKeyBindDialog.Value);
		m_CurrentKeyBindDialog.Value.Dispose();
		m_CurrentKeyBindDialog.Value = null;
	}

	public void Close()
	{
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(OnCloseDialogAnswer);
		}
		else
		{
			m_CloseAction?.Invoke();
		}
	}

	private void OnChangeSettingsList(Action<DialogMessageBoxBase.BoxButton> OnApplyDialogAction)
	{
		string questionText = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.SaveChangesMessage, (m_PreviousSelectedMenuEntity != null) ? ((object)m_PreviousSelectedMenuEntity.Title) : ((object)string.Empty));
		LocalizedString yesText = UIStrings.Instance.SettingsUI.DialogSave;
		LocalizedString noText = UIStrings.Instance.SettingsUI.DialogRevert;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(questionText, DialogMessageBoxBase.BoxType.Dialog, OnApplyDialogAction, null, yesText, noText);
		});
	}

	public void OpenApplySettingsDialog()
	{
		if (SettingsController.Instance.HasUnconfirmedSettings())
		{
			OnChangeSettingsList(OnApplyDialogAnswer);
		}
	}

	private void OnApplyDialogAnswer(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			ApplySettings();
		}
		else
		{
			RevertSettings();
		}
	}

	private void OnCloseDialogAnswer(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			ApplySettings();
		}
		else
		{
			RevertSettings();
		}
		m_CloseAction?.Invoke();
	}

	private void ApplySettings()
	{
		Locale value = SettingsRoot.Game.Main.Localization.GetValue();
		MainMenuTheme mainMenuTheme = SettingsRoot.Game.Main.MainMenuTheme.GetValue();
		float fontSizeMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		int value2 = SettingsRoot.Display.SafeZoneOffset.GetValue();
		SettingsController.Instance.Sync();
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		bool isMainMenu = Game.Instance.SceneLoader.LoadedUIScene == GameScenes.MainMenu;
		if (value != SettingsRoot.Game.Main.Localization.GetValue() || (mainMenuTheme != SettingsRoot.Game.Main.MainMenuTheme.GetValue() && isMainMenu))
		{
			Game.ResetUI(delegate
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					if (mainMenuTheme != SettingsRoot.Game.Main.MainMenuTheme.GetValue() && isMainMenu)
					{
						SoundState.Instance.ResetState(SoundStateType.MainMenu);
					}
					EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
					{
						h.HandleOpenSettings(isMainMenu);
					});
				}, 5);
			});
		}
		if (Math.Abs(fontSizeMultiplier - SettingsRoot.Accessiability.FontSizeMultiplier) > 0.01f)
		{
			EventBus.RaiseEvent(delegate(ISettingsFontSizeUIHandler h)
			{
				h.HandleChangeFontSizeSettings(SettingsRoot.Accessiability.FontSizeMultiplier);
			});
		}
		if (value2 != SettingsRoot.Display.SafeZoneOffset.GetValue())
		{
			EventBus.RaiseEvent(delegate(ISafeZoneUIHandler h)
			{
				h.OnSafeZoneChanged();
			});
		}
		HandleItemChanged(string.Empty);
		OnApplyWindowClose.Execute();
	}

	private void RevertSettings()
	{
		SettingsController.Instance.RevertAllTempValues();
		HandleItemChanged(string.Empty);
		OnApplyWindowClose.Execute();
	}

	public void OpenDefaultSettingsDialog()
	{
		string text = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage, m_SelectedMenuEntity.Value.Title);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxBase.BoxType.Dialog, OnDefaultDialogAnswer);
		});
	}

	private void OnDefaultDialogAnswer(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			SettingsController.Instance.ResetToDefault(m_SelectedMenuEntity.Value.SettingsScreenType);
			HandleItemChanged(string.Empty);
		}
	}

	public void OpenCancelSettingsDialog()
	{
		string text = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.CancelChangesMessage, m_SelectedMenuEntity.Value.Title);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxBase.BoxType.Dialog, OnCancelDialogAnswer);
		});
	}

	private void OnCancelDialogAnswer(DialogMessageBoxBase.BoxButton buttonType)
	{
		if (buttonType == DialogMessageBoxBase.BoxButton.Yes)
		{
			RevertSettings();
		}
	}

	public void HandleItemChanged(string key)
	{
		IsApplyButtonInteractable.Value = SettingsController.Instance.HasUnconfirmedSettings();
		IsCancelButtonInteractable.Value = SettingsController.Instance.HasUnconfirmedSettings();
		UISettingsRoot.Instance.UIDifficultySettings.UpdateInteractable();
		UISettingsRoot.Instance.UIGraphicsSettings.UpdateInteractable(initialize: false);
		UISettingsRoot.Instance.UIGameTutorialSettings.UpdateInteractable(key);
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	void ISaveSettingsHandler.HandleSaveSettings()
	{
		HandleItemChanged(string.Empty);
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SelectionGroup.SelectPrevValidEntity();
		SelectionGroup.SelectNextValidEntity();
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		ReactiveTooltipTemplate.Value = (IsConsoleControls.Value ? null : ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null));
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}

	public void HandleLanguageChanged()
	{
		LanguageChanged.Execute();
		IEnumerable<VirtualListElementVMBase> source = m_SettingEntities.Where((VirtualListElementVMBase e) => e is SettingsEntityHeaderVM);
		if (!source.Any())
		{
			return;
		}
		source.ForEach(delegate(VirtualListElementVMBase h)
		{
			if (h is SettingsEntityHeaderVM settingsEntityHeaderVM)
			{
				settingsEntityHeaderVM.UpdateLocalization();
			}
		});
	}
}
