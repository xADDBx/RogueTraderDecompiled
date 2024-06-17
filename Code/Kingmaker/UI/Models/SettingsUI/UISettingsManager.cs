using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI;

public class UISettingsManager
{
	public enum SettingsScreen
	{
		Game,
		Difficulty,
		Sound,
		Graphics,
		Controls,
		StartGameDifficulty,
		Display,
		Accessiability,
		Language,
		SafeZone
	}

	private bool m_Initialized;

	private bool m_KeyBindingsCached;

	private Dictionary<string, UISettingsEntityKeyBinding> m_KeyBindCache = new Dictionary<string, UISettingsEntityKeyBinding>();

	public bool IsNewKeyBindingSelectionHappening;

	private readonly List<UISettingsGroup> m_GameSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_SoundSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_SafeZoneSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_DisplaySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_AccessiablitySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_LanguageFirstLaunchSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_GraphicsSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_DifficultySettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_ControlSettingsList = new List<UISettingsGroup>();

	private readonly List<UISettingsGroup> m_StartGameDifficultySettingsList = new List<UISettingsGroup>();

	private static UISettingsRoot UISettingsRoot => Game.Instance.BlueprintRoot.UISettingsRoot;

	public IEnumerable<UISettingsEntityKeyBinding> KeyBindings => (from k in UISettingsRoot.Controls.SelectMany((UISettingsGroup m) => m.VisibleSettingsList)
		where k != null && k.Type == SettingsListItemType.Keybind
		select k).OfType<UISettingsEntityKeyBinding>();

	public UISettingsEntityKeyBinding GetBindByName(string name)
	{
		if (!m_KeyBindingsCached)
		{
			BuildKeyBindCache();
		}
		UISettingsEntityKeyBinding value = null;
		m_KeyBindCache.TryGetValue(name, out value);
		return value;
	}

	private void BuildKeyBindCache()
	{
		if (m_KeyBindingsCached)
		{
			return;
		}
		foreach (UISettingsEntityKeyBinding keyBinding in KeyBindings)
		{
			m_KeyBindCache.Add(keyBinding.name, keyBinding);
		}
		m_KeyBindingsCached = true;
	}

	public void Initialize()
	{
		if (m_Initialized)
		{
			return;
		}
		UISettingsRoot.Instance.LinkToSettings();
		UISettingsRoot.Instance.InitializeSettings();
		m_Initialized = true;
		m_GameSettingsList.Clear();
		UISettingsGroup[] gameSettings = UISettingsRoot.GameSettings;
		foreach (UISettingsGroup uISettingsGroup in gameSettings)
		{
			if (!(uISettingsGroup == null))
			{
				m_GameSettingsList.Add(uISettingsGroup);
			}
		}
		m_DifficultySettingsList.Clear();
		gameSettings = UISettingsRoot.DifficultySettings;
		foreach (UISettingsGroup uISettingsGroup2 in gameSettings)
		{
			if (!(uISettingsGroup2 == null))
			{
				m_DifficultySettingsList.Add(uISettingsGroup2);
			}
		}
		m_SoundSettingsList.Clear();
		gameSettings = UISettingsRoot.SoundSettings;
		foreach (UISettingsGroup uISettingsGroup3 in gameSettings)
		{
			if (!(uISettingsGroup3 == null))
			{
				m_SoundSettingsList.Add(uISettingsGroup3);
			}
		}
		m_SafeZoneSettingsList.Clear();
		gameSettings = UISettingsRoot.SafeZone;
		foreach (UISettingsGroup uISettingsGroup4 in gameSettings)
		{
			if (!(uISettingsGroup4 == null))
			{
				m_SafeZoneSettingsList.Add(uISettingsGroup4);
			}
		}
		m_DisplaySettingsList.Clear();
		gameSettings = UISettingsRoot.DisplaySettings;
		foreach (UISettingsGroup uISettingsGroup5 in gameSettings)
		{
			if (!(uISettingsGroup5 == null))
			{
				m_DisplaySettingsList.Add(uISettingsGroup5);
			}
		}
		m_AccessiablitySettingsList.Clear();
		gameSettings = UISettingsRoot.AccessiabilitySettings;
		foreach (UISettingsGroup uISettingsGroup6 in gameSettings)
		{
			if (!(uISettingsGroup6 == null))
			{
				m_AccessiablitySettingsList.Add(uISettingsGroup6);
			}
		}
		m_LanguageFirstLaunchSettingsList.Clear();
		gameSettings = UISettingsRoot.GameSettings;
		foreach (UISettingsGroup uISettingsGroup7 in gameSettings)
		{
			if (!(uISettingsGroup7 == null))
			{
				m_LanguageFirstLaunchSettingsList.Add(uISettingsGroup7);
			}
		}
		m_GraphicsSettingsList.Clear();
		gameSettings = UISettingsRoot.GraphicsSettings;
		foreach (UISettingsGroup uISettingsGroup8 in gameSettings)
		{
			if (!(uISettingsGroup8 == null))
			{
				m_GraphicsSettingsList.Add(uISettingsGroup8);
			}
		}
		m_ControlSettingsList.Clear();
		gameSettings = UISettingsRoot.Controls;
		foreach (UISettingsGroup uISettingsGroup9 in gameSettings)
		{
			if (!(uISettingsGroup9 == null))
			{
				m_ControlSettingsList.Add(uISettingsGroup9);
			}
		}
		m_StartGameDifficultySettingsList.Clear();
		gameSettings = UISettingsRoot.StartGame;
		foreach (UISettingsGroup uISettingsGroup10 in gameSettings)
		{
			if (!(uISettingsGroup10 == null))
			{
				m_StartGameDifficultySettingsList.Add(uISettingsGroup10);
			}
		}
		BuildKeyBindCache();
	}

	public List<UISettingsGroup> GetSettingsList(SettingsScreen? screenId)
	{
		if (!screenId.HasValue)
		{
			return null;
		}
		Initialize();
		return screenId switch
		{
			SettingsScreen.Game => m_GameSettingsList, 
			SettingsScreen.Graphics => m_GraphicsSettingsList, 
			SettingsScreen.Difficulty => m_DifficultySettingsList, 
			SettingsScreen.Sound => m_SoundSettingsList, 
			SettingsScreen.Controls => m_ControlSettingsList, 
			SettingsScreen.StartGameDifficulty => m_StartGameDifficultySettingsList, 
			SettingsScreen.Display => m_DisplaySettingsList, 
			SettingsScreen.Accessiability => m_AccessiablitySettingsList, 
			SettingsScreen.Language => m_LanguageFirstLaunchSettingsList, 
			SettingsScreen.SafeZone => m_SafeZoneSettingsList, 
			_ => null, 
		};
	}

	public void OnSettingsApplied()
	{
		Game.Instance.Player.MinDifficultyController.UpdateMinDifficulty();
	}
}
