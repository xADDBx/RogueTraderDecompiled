using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;

public class FirstLaunchSettingsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILocalizationHandler, ISubscriber
{
	private const string FIRST_LAUNCH_PREF_KEY = "first_open_first_launch_settings";

	private readonly Action m_CloseAction;

	public readonly SelectionGroupRadioVM<SettingsMenuEntityVM> SelectionGroup;

	private readonly ReactiveProperty<SettingsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<SettingsMenuEntityVM>();

	private readonly List<SettingsMenuEntityVM> m_MenuEntitiesList = new List<SettingsMenuEntityVM>();

	public readonly ReactiveProperty<FirstLaunchLanguagePageVM> LanguagePageVM = new ReactiveProperty<FirstLaunchLanguagePageVM>();

	public readonly ReactiveProperty<FirstLaunchSafeZonePageVM> SafeZonePageVM = new ReactiveProperty<FirstLaunchSafeZonePageVM>();

	public readonly ReactiveProperty<FirstLaunchDisplayPageVM> DisplayPageVM = new ReactiveProperty<FirstLaunchDisplayPageVM>();

	public readonly ReactiveProperty<FirstLaunchAccessiabilityPageVM> AccessiabilityPageVM = new ReactiveProperty<FirstLaunchAccessiabilityPageVM>();

	public readonly ReactiveCommand LanguageChanged = new ReactiveCommand();

	public readonly ReactiveCommand ShowPhotosensitivityScreen = new ReactiveCommand();

	public readonly BoolReactiveProperty BlockHints = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsVisibleHorizontalDPad = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsVisibleVerticalDPad = new BoolReactiveProperty();

	public static bool HasShown => PlayerPrefs.GetInt("first_open_first_launch_settings", 0) == 1;

	public FirstLaunchSettingsVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameLanguage, UISettingsManager.SettingsScreen.Language);
		if (Game.Instance.IsControllerGamepad)
		{
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSafeZone, UISettingsManager.SettingsScreen.SafeZone);
		}
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDisplay, UISettingsManager.SettingsScreen.Display);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameAccessiability, UISettingsManager.SettingsScreen.Accessiability);
		AddDisposable(SelectionGroup = new SelectionGroupRadioVM<SettingsMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity));
		SelectionGroup.TrySelectFirstValidEntity();
		AddDisposable(m_SelectedMenuEntity.Subscribe(delegate(SettingsMenuEntityVM e)
		{
			if (e != null)
			{
				SwitchSettingsScreen(e.SettingsScreenType);
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
	{
		SettingsMenuEntityVM settingsMenuEntityVM = new SettingsMenuEntityVM(localizedString, screenType, null);
		AddDisposable(settingsMenuEntityVM);
		m_MenuEntitiesList.Add(settingsMenuEntityVM);
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		ApplySettings();
		DisposeAll();
		switch (settingsScreen)
		{
		case UISettingsManager.SettingsScreen.Language:
			LanguagePageVM.Value = new FirstLaunchLanguagePageVM();
			break;
		case UISettingsManager.SettingsScreen.SafeZone:
			SafeZonePageVM.Value = new FirstLaunchSafeZonePageVM();
			break;
		case UISettingsManager.SettingsScreen.Display:
			DisplayPageVM.Value = new FirstLaunchDisplayPageVM();
			break;
		case UISettingsManager.SettingsScreen.Accessiability:
			AccessiabilityPageVM.Value = new FirstLaunchAccessiabilityPageVM();
			break;
		}
		SetHorizontalAndVerticalDPadVisible(settingsScreen);
	}

	private void SetHorizontalAndVerticalDPadVisible(UISettingsManager.SettingsScreen settingsScreen)
	{
		IsVisibleHorizontalDPad.Value = settingsScreen == UISettingsManager.SettingsScreen.SafeZone || settingsScreen == UISettingsManager.SettingsScreen.Display || settingsScreen == UISettingsManager.SettingsScreen.Accessiability;
		IsVisibleVerticalDPad.Value = settingsScreen == UISettingsManager.SettingsScreen.Language || settingsScreen == UISettingsManager.SettingsScreen.Display || settingsScreen == UISettingsManager.SettingsScreen.Accessiability;
	}

	private void DisposeAll()
	{
		LanguagePageVM.Value?.Dispose();
		LanguagePageVM.Value = null;
		SafeZonePageVM.Value?.Dispose();
		SafeZonePageVM.Value = null;
		DisplayPageVM.Value?.Dispose();
		DisplayPageVM.Value = null;
		AccessiabilityPageVM.Value?.Dispose();
		AccessiabilityPageVM.Value = null;
	}

	protected override void DisposeImplementation()
	{
		DisposeAll();
		PlayerPrefs.Save();
	}

	private void ApplySettings()
	{
		float fontSizeMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		if (Math.Abs(fontSizeMultiplier - SettingsRoot.Accessiability.FontSizeMultiplier) > 0.01f)
		{
			EventBus.RaiseEvent(delegate(ISettingsFontSizeUIHandler h)
			{
				h.HandleChangeFontSizeSettings(SettingsRoot.Accessiability.FontSizeMultiplier);
			});
		}
	}

	public void RevertSettings()
	{
		SettingsController.Instance.ResetToDefault(m_SelectedMenuEntity.Value.SettingsScreenType);
	}

	public void NextPage()
	{
		if (!SelectionGroup.SelectNextValidEntity())
		{
			ApplySettings();
			BlockHints.Value = true;
			ShowPhotosensitivityScreen.Execute();
		}
	}

	public void PreviousPage()
	{
		SelectionGroup.SelectPrevValidEntity();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
		BlockHints.Value = false;
	}

	[Cheat(Name = "clear_first_launch")]
	public static void ClearFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_first_launch_settings", 0);
		PlayerPrefs.Save();
	}

	[Cheat(Name = "set_first_launch")]
	public static void SetFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_first_launch_settings", 1);
		PlayerPrefs.Save();
	}

	public void HandleLanguageChanged()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(SettingsMenuEntityVM e)
		{
			e.UpdateTitle();
		});
		LanguageChanged.Execute();
	}
}
