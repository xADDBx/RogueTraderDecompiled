using System;
using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings.ConstructionHelpers.KeyPrefix;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Utility.DisposableExtension;
using UnityEngine;

namespace Kingmaker.Settings;

public static class SettingsRoot
{
	private static SoundSettings s_Sound;

	private static GraphicsSettings s_Graphics;

	private static GameSettings s_Game;

	private static DifficultySettings s_Difficulty;

	private static ControlsSettings s_Controls;

	private static DisplaySettings s_Display;

	private static AccessiabilitySettings s_Accessiability;

	private static bool s_Initialized;

	private static readonly DisposableBooleanFlag Initializing = new DisposableBooleanFlag();

	public static SoundSettings Sound
	{
		get
		{
			EnsureInitialized();
			return s_Sound;
		}
	}

	public static GraphicsSettings Graphics
	{
		get
		{
			EnsureInitialized();
			return s_Graphics;
		}
	}

	public static GameSettings Game
	{
		get
		{
			EnsureInitialized();
			return s_Game;
		}
	}

	public static DifficultySettings Difficulty
	{
		get
		{
			EnsureInitialized();
			return s_Difficulty;
		}
	}

	public static ControlsSettings Controls
	{
		get
		{
			EnsureInitialized();
			return s_Controls;
		}
	}

	public static DisplaySettings Display
	{
		get
		{
			EnsureInitialized();
			return s_Display;
		}
	}

	public static AccessiabilitySettings Accessiability
	{
		get
		{
			EnsureInitialized();
			return s_Accessiability;
		}
	}

	public static bool Initialized => s_Initialized;

	public static void EnsureInitialized()
	{
		if (s_Initialized || (bool)Initializing)
		{
			return;
		}
		try
		{
			if (BlueprintRoot.Instance == null)
			{
				PFLog.Settings.Error("Can't initialize settings, because BlueprintRoot is null");
				return;
			}
		}
		catch (Exception)
		{
			PFLog.Settings.Error("Can't initialize settings, because BlueprintRoot is null");
			return;
		}
		Initialize(BlueprintRoot.Instance.SettingsValues);
	}

	public static void Initialize(SettingsValues settingsValues)
	{
		if (s_Initialized || (bool)Initializing)
		{
			return;
		}
		using (Initializing.Retain())
		{
			SettingsDefaultValues settingsDefaultValues = settingsValues.SettingsDefaultValues;
			ISettingsController instance = SettingsController.Instance;
			using (new SettingsKeyPrefix("settings"))
			{
				using (new SettingsKeyPrefix("sound"))
				{
					s_Sound = new SoundSettings(instance, settingsDefaultValues.Sound);
				}
				using (new SettingsKeyPrefix("graphics"))
				{
					s_Graphics = new GraphicsSettings(instance, settingsValues);
				}
				using (new SettingsKeyPrefix("game"))
				{
					s_Game = new GameSettings(instance, settingsDefaultValues.Game);
				}
				using (new SettingsKeyPrefix("difficulty"))
				{
					s_Difficulty = new DifficultySettings(instance, settingsValues);
				}
				using (new SettingsKeyPrefix("controls"))
				{
					s_Controls = new ControlsSettings(instance, settingsDefaultValues.Controls);
				}
				using (new SettingsKeyPrefix("display"))
				{
					s_Display = new DisplaySettings(instance, settingsValues);
				}
				using (new SettingsKeyPrefix("accessiability"))
				{
					s_Accessiability = new AccessiabilitySettings(instance, settingsValues);
				}
			}
			SettingsController.Instance.InitializeControllers(settingsValues.DifficultiesPresets, settingsValues.GraphicsPresetsList);
			Graphics.DumpToLog();
			s_Initialized = true;
		}
	}

	[Cheat(Name = "clear_prefs")]
	public static void DeletePlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}
}
