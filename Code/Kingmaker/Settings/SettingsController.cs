using System;
using System.Collections.Generic;
using System.IO;
using Kingmaker.Networking;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Settings.Entities.Persistence.Actions;
using Kingmaker.Settings.Graphics;
using Kingmaker.Settings.Interfaces;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.DisposableExtension;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine.SceneManagement;

namespace Kingmaker.Settings;

public class SettingsController : ISettingsController
{
	private static readonly SettingsController s_Instance = new SettingsController();

	public const string GeneralSettingsProviderFileName = "general_settings.json";

	private HashSet<string> s_Keys;

	private List<ISettingsEntity> s_AllSettingEntities;

	private List<ISettingsEntity> s_InSaveSettingsEntities;

	private List<ISettingsEntity> s_GeneralSettingsEntities;

	private List<ISettingsEntity> s_SettingsEntitiesForConfirmation;

	private DisposableBooleanFlag s_ConfirmingAllSettings = new DisposableBooleanFlag();

	private const int MAX_ITERATIONS_NUMBER = 10000;

	private ISettingsProvider s_InSaveSettingsProvider;

	private bool m_CanInitializeSoundController;

	public static SettingsController Instance => s_Instance;

	public string GeneralSettingsProviderPath { get; } = Path.Combine(ApplicationPaths.persistentDataPath, "general_settings.json");


	public DifficultySettingsController DifficultySettingsController { get; private set; }

	public DifficultyPresetsController DifficultyPresetsController { get; private set; }

	public GraphicsSettingsController GraphicsSettingsController { get; private set; }

	public GraphicsPresetsController GraphicsPresetsController { get; private set; }

	public DisplaySettingsController DisplaySettingsController { get; private set; }

	public AccessiabilitySettingsController AccessiabilitySettingsController { get; private set; }

	public SoundSettingsController SoundSettingsController { get; private set; }

	public GameSettingsController GameSettingsController { get; private set; }

	public ISettingsProvider GeneralSettingsProvider { get; private set; }

	public ISettingsProvider InSaveSettingsProvider
	{
		get
		{
			if (!IsInMainMenu())
			{
				return s_InSaveSettingsProvider;
			}
			return s_InSaveSettingsProvider ?? GeneralSettingsProvider;
		}
	}

	public bool SettingsSyncInProgress => false;

	public event Action OnConfirmedAllSettings;

	private SettingsController()
	{
		s_Keys = new HashSet<string>();
		s_AllSettingEntities = new List<ISettingsEntity>();
		s_InSaveSettingsEntities = new List<ISettingsEntity>();
		s_GeneralSettingsEntities = new List<ISettingsEntity>();
		s_SettingsEntitiesForConfirmation = new List<ISettingsEntity>();
	}

	public void InitializeControllers(DifficultyPresetsList difficultyPresetsList, GraphicsPresetsList graphicsPresetsList)
	{
		StartGeneralSettings();
		DifficultySettingsController = new DifficultySettingsController();
		DifficultyPresetsController = new DifficultyPresetsController(difficultyPresetsList);
		GraphicsPresetsController = new GraphicsPresetsController(graphicsPresetsList);
		GraphicsPresetsController.Initialize();
		GraphicsSettingsController = new GraphicsSettingsController(GraphicsPresetsController.MaximumAllowedResolution);
		DisplaySettingsController = new DisplaySettingsController();
		AccessiabilitySettingsController = new AccessiabilitySettingsController();
		GameSettingsController = new GameSettingsController();
		if (m_CanInitializeSoundController && SoundSettingsController == null)
		{
			SoundSettingsController = new SoundSettingsController();
		}
		ConfirmAllTempValues();
		SaveAll();
		PhotonManager.Settings.Init();
	}

	public void InitializeSoundController()
	{
		if (SettingsRoot.Initialized && SoundSettingsController == null)
		{
			SoundSettingsController = new SoundSettingsController();
			SaveAll();
		}
		else
		{
			m_CanInitializeSoundController = true;
		}
	}

	public void AccountSetting(ISettingsEntity settingsEntity)
	{
		try
		{
			if (s_Keys.Contains(settingsEntity.Key))
			{
				throw new ArgumentException("[Settings Data] Key '" + settingsEntity.Key + "' is already occupied");
			}
			s_Keys.Add(settingsEntity.Key);
			s_AllSettingEntities.Add(settingsEntity);
			if (settingsEntity.SaveDependent)
			{
				s_InSaveSettingsEntities.Add(settingsEntity);
			}
			else
			{
				s_GeneralSettingsEntities.Add(settingsEntity);
			}
		}
		catch (Exception arg)
		{
			PFLog.Settings.Error($"NullReferenceException {arg}");
		}
	}

	private void StartGeneralSettings()
	{
		ISettingsProvider settingsProvider = new GeneralSettingsProvider(GeneralSettingsProviderPath);
		SettingsUpgraderRoot.Apply(settingsProvider);
		SetGeneralSettingsProvider(settingsProvider);
	}

	private void StopGeneralSettings()
	{
		SetGeneralSettingsProvider(null);
	}

	public void StartInSaveSettings()
	{
		SetInSaveSettingsProvider(new InSaveSettingsProvider());
		DifficultyPresetsController.ApplyCurrentDifficultyPreset();
		DifficultyPresetsController.UpdateGameDifficultiesComparisons();
		ConfirmAllTempValues();
	}

	public void StopInSaveSettings()
	{
		SetInSaveSettingsProvider(null);
	}

	private void SetGeneralSettingsProvider(ISettingsProvider settingsProvider)
	{
		GeneralSettingsProvider = settingsProvider;
		List<ISettingsEntity> list = ((InSaveSettingsProvider == null) ? s_AllSettingEntities : s_GeneralSettingsEntities);
		if (settingsProvider == null)
		{
			return;
		}
		if (settingsProvider.IsEmpty)
		{
			foreach (ISettingsEntity item in list)
			{
				item.SetCurrentValueInProvider();
			}
			return;
		}
		foreach (ISettingsEntity item2 in list)
		{
			item2.ResetCache();
		}
	}

	private void SetInSaveSettingsProvider(ISettingsProvider settingsProvider)
	{
		s_InSaveSettingsProvider = settingsProvider;
		foreach (ISettingsEntity s_InSaveSettingsEntity in s_InSaveSettingsEntities)
		{
			s_InSaveSettingsEntity.ResetCache();
		}
		if (settingsProvider == null || !settingsProvider.IsEmpty)
		{
			return;
		}
		foreach (ISettingsEntity s_InSaveSettingsEntity2 in s_InSaveSettingsEntities)
		{
			s_InSaveSettingsEntity2.SetCurrentValueInProvider();
		}
	}

	public void AddToConfirmationList(ISettingsEntity settingsEntity)
	{
		s_SettingsEntitiesForConfirmation.Add(settingsEntity);
	}

	public void RemoveFromConfirmationList(ISettingsEntity settingsEntity, bool confirming)
	{
		s_SettingsEntitiesForConfirmation.Remove(settingsEntity);
		if (confirming && !s_ConfirmingAllSettings && s_SettingsEntitiesForConfirmation.Count == 0)
		{
			this.OnConfirmedAllSettings?.Invoke();
		}
	}

	public bool ConfirmationListContains(ISettingsEntity settingsEntity)
	{
		return s_SettingsEntitiesForConfirmation.Contains(settingsEntity);
	}

	public bool HasUnconfirmedSettings()
	{
		return s_SettingsEntitiesForConfirmation.Count > 0;
	}

	public void Sync()
	{
		PhotonManager.Settings.Sync(s_SettingsEntitiesForConfirmation);
	}

	public void ConfirmAllTempValues()
	{
		if (s_SettingsEntitiesForConfirmation.Count == 0)
		{
			return;
		}
		using (s_ConfirmingAllSettings.Retain())
		{
			int num = 0;
			while (s_SettingsEntitiesForConfirmation.Count > 0)
			{
				ISettingsEntity settingsEntity = s_SettingsEntitiesForConfirmation[s_SettingsEntitiesForConfirmation.Count - 1];
				s_SettingsEntitiesForConfirmation.RemoveAt(s_SettingsEntitiesForConfirmation.Count - 1);
				settingsEntity.ConfirmTempValue();
				num++;
				if (num >= 10000)
				{
					PFLog.Settings.Error("Infinity cycle in confirm all, aborting");
					break;
				}
			}
		}
		this.OnConfirmedAllSettings?.Invoke();
	}

	public void RevertAllTempValues()
	{
		int num = 0;
		while (s_SettingsEntitiesForConfirmation.Count > 0)
		{
			ISettingsEntity settingsEntity = s_SettingsEntitiesForConfirmation[s_SettingsEntitiesForConfirmation.Count - 1];
			s_SettingsEntitiesForConfirmation.RemoveAt(s_SettingsEntitiesForConfirmation.Count - 1);
			settingsEntity.RevertTempValue();
			num++;
			if (num >= 10000)
			{
				PFLog.Settings.Error("Infinity cycle in revert all, aborting");
				break;
			}
		}
	}

	public void ResetToDefault(UISettingsManager.SettingsScreen settingsScreen)
	{
		RevertAllTempValues();
		foreach (UISettingsGroup settings in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
		{
			if (!settings.IsVisible)
			{
				continue;
			}
			foreach (UISettingsEntityBase visibleSettings in settings.VisibleSettingsList)
			{
				if (!visibleSettings.NoDefaultReset && visibleSettings is IUISettingsEntityWithValueBase iUISettingsEntityWithValueBase)
				{
					iUISettingsEntityWithValueBase.SettingsEntity.ResetToDefault(confirmChanges: false);
				}
			}
		}
	}

	public void SaveAll()
	{
		GeneralSettingsProvider?.SaveAll();
		InSaveSettingsProvider?.SaveAll();
	}

	public void Reset()
	{
		RevertAllTempValues();
		s_Keys.Clear();
		s_AllSettingEntities.Clear();
		s_InSaveSettingsEntities.Clear();
		s_GeneralSettingsEntities.Clear();
		s_SettingsEntitiesForConfirmation.Clear();
		StopInSaveSettings();
		StopGeneralSettings();
	}

	public void StartSyncSettings()
	{
	}

	private bool IsInMainMenu()
	{
		if (!Game.HasInstance)
		{
			return true;
		}
		if (Game.Instance.AlreadyInitialized && !Game.Instance.RootUiContext.IsMainMenu)
		{
			return SceneManager.GetSceneByName("MainMenu").isLoaded;
		}
		return true;
	}
}
