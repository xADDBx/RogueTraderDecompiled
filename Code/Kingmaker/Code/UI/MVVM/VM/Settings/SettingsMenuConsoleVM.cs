using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Localization;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Settings;

public class SettingsMenuConsoleVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<SettingsMenuEntityVM> MenuEntitiesVMList = new List<SettingsMenuEntityVM>();

	private readonly Action<UISettingsManager.SettingsScreen?, bool> m_CloseAction;

	private readonly bool m_IsMainMenu;

	public SettingsMenuConsoleVM(Action<UISettingsManager.SettingsScreen?, bool> closeAction, bool isMainMenu = false)
	{
		m_CloseAction = closeAction;
		m_IsMainMenu = isMainMenu;
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGame, UISettingsManager.SettingsScreen.Game);
		if (!isMainMenu)
		{
			CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameDifficulty, UISettingsManager.SettingsScreen.Difficulty);
		}
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameControls, UISettingsManager.SettingsScreen.Controls);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameGraphics, UISettingsManager.SettingsScreen.Graphics);
		CreateMenuEntity(UIStrings.Instance.SettingsUI.SectionNameSound, UISettingsManager.SettingsScreen.Sound);
	}

	private void CreateMenuEntity(LocalizedString localizedString, UISettingsManager.SettingsScreen screenType)
	{
		SettingsMenuEntityVM settingsMenuEntityVM = new SettingsMenuEntityVM(localizedString, screenType, ShowSettings);
		AddDisposable(settingsMenuEntityVM);
		MenuEntitiesVMList.Add(settingsMenuEntityVM);
	}

	private void ShowSettings(UISettingsManager.SettingsScreen screenType)
	{
		m_CloseAction?.Invoke(screenType, m_IsMainMenu);
	}

	public void Close()
	{
		m_CloseAction?.Invoke(null, m_IsMainMenu);
	}

	protected override void DisposeImplementation()
	{
	}
}
