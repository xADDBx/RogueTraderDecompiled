using System;
using System.Collections;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.QA.Clockwork;

public class Clockwork
{
	public delegate void ClockworkReporterCreatedDelegate(Clockwork clockwork);

	private BlueprintClockworkScenario m_Scenario;

	private string m_LastGameOverArea;

	private int m_GameOverCount;

	public static Clockwork Instance = new Clockwork();

	public static ClockworkReporterCreatedDelegate OnClockworkReporterCreated;

	public ClockworkRunner Runner { get; set; }

	public ClockworkReporter Reporter { get; private set; }

	public bool ShowDebugEndMessage { get; private set; }

	public BlueprintClockworkScenario Scenario
	{
		get
		{
			return m_Scenario;
		}
		set
		{
			if (Runner == null)
			{
				m_Scenario = value;
			}
			else
			{
				PFLog.Clockwork.Error("Trying to replace scenario, but runner is not null!");
			}
		}
	}

	public static bool IsRunning
	{
		get
		{
			ClockworkRunner runner = Instance.Runner;
			if (runner == null)
			{
				return false;
			}
			return !runner.Paused;
		}
	}

	public static bool GameIsLoadingWithScenario => !PlayerPrefs.GetString("ClockworkScenario", "").IsNullOrEmpty();

	public static bool Enabled { get; set; } = false;


	public void Start(string scenarioName = "")
	{
		if (Runner != null)
		{
			Stop();
		}
		if (string.IsNullOrEmpty(scenarioName))
		{
			scenarioName = PlayerPrefs.GetString("ClockworkScenario", null) ?? CommandLineArguments.Parse().Get("-clockwork");
			PlayerPrefs.DeleteKey("ClockworkScenario");
			PlayerPrefs.Save();
		}
		BlueprintClockworkScenario instruction = ClockworkScenarioIndex.Instance.GetInstruction(scenarioName);
		if (instruction != null)
		{
			Scenario = instruction;
			PFLog.Clockwork.Log("Start Clockwork scenario " + Scenario.ScenarioName);
			try
			{
				Reporter = new ClockworkReporter();
				OnClockworkReporterCreated?.Invoke(this);
				Runner = new ClockworkRunner(Scenario);
				StartScenario();
				StartRunner();
				Reporter.ForceReport();
				return;
			}
			catch (Exception ex)
			{
				Reporter.HandleError(ex.Message + "\n\tStackTrace: " + ex.StackTrace);
				Stop();
				throw;
			}
		}
		string messageFormat = "Cannot start Clockwork: no scenario named [" + scenarioName + "]";
		PFLog.Clockwork.Error(messageFormat);
	}

	public void Stop()
	{
		StopRunner();
		Reporter?.ForceReport();
		Scenario = null;
	}

	public void Load(PlayData data)
	{
		StartFromSave(data.SaveName);
		StopRunner();
		Runner = new ClockworkRunner(Scenario, data);
		StartRunner();
	}

	private void StartScenario()
	{
		if (Scenario == null)
		{
			return;
		}
		Scenario.Initialize();
		if (Application.isPlaying)
		{
			if (Scenario.IsStartFromPreset)
			{
				LoadingProcess.Instance.StartCoroutine(StartFromPreset(Scenario.Preset));
			}
			else if (Scenario.IsStartFromSave)
			{
				StartFromSave(Scenario.Save);
			}
			else if (Scenario.IsStartFromRemoteSave)
			{
				SavesStorageAccess.Load(Scenario.RemoteSave);
			}
		}
	}

	private IEnumerator StartFromPreset(BlueprintAreaPreset preset)
	{
		if (!SceneManager.GetSceneByName("MainMenu").isLoaded)
		{
			Game.Instance.ResetToMainMenu();
		}
		yield return null;
		while (LoadingProcess.Instance.IsLoadingInProcess)
		{
			yield return null;
		}
		if (MainMenuUI.Instance != null)
		{
			preset.OverrideGameDifficulty = Scenario.OverridePresetDifficulty;
			MainMenuUI.Instance.EnterGame(delegate
			{
				Game.Instance.LoadNewGame(preset);
			});
			yield return null;
			yield return null;
			yield return null;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
		}
		else
		{
			Reporter.HandleError("Failed to start game from preset: MainMenu not found!");
		}
	}

	private void StartFromSave(string saveName)
	{
		SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault((SaveInfo s) => s.Name == saveName);
		if (saveInfo == null)
		{
			throw new Exception("Game cannot be loaded: no save file [" + saveName + "]");
		}
		SceneLoader.LoadObligatoryScenes();
		Game.Instance.LoadGame(saveInfo);
	}

	private void StopRunner()
	{
		if (Runner != null)
		{
			ClockworkController.OnTick -= Runner.Tick;
			EventBus.Unsubscribe(Runner);
			Runner = null;
		}
	}

	private void StartRunner()
	{
		EventBus.Subscribe(Runner);
		AttemptsCounter.Instance.Reset();
		ClockworkController.OnTick += Runner.Tick;
	}

	public static void SaveClockworkState(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			string saveName = "Clockwork_" + Path.GetFileNameWithoutExtension(path);
			SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault((SaveInfo s) => s.Name == saveName);
			if (saveInfo != null)
			{
				Game.Instance.SaveManager.DeleteSave(saveInfo);
			}
			saveInfo = Game.Instance.SaveManager.CreateNewSave(saveName);
			Game.Instance.SaveGame(saveInfo);
			Instance.Runner.Data.SaveName = saveInfo.Name;
			string value = JsonConvert.SerializeObject(Instance.Runner.Data, Formatting.Indented);
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine(value);
		}
	}

	public static void LoadClockworkState(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			PlayData data;
			using (StreamReader streamReader = new StreamReader(path))
			{
				data = JsonConvert.DeserializeObject<PlayData>(streamReader.ReadToEnd());
			}
			Instance.Load(data);
			Instance.Runner.Paused = true;
		}
	}

	public void LoadSaveOnGameOver()
	{
		if (m_LastGameOverArea != Game.Instance.CurrentlyLoadedArea.AreaName)
		{
			m_LastGameOverArea = Game.Instance.CurrentlyLoadedArea.AreaName;
			m_GameOverCount = 0;
		}
		m_GameOverCount++;
		if (m_GameOverCount >= m_Scenario.AreaGameOverLimit)
		{
			Reporter.HandleError("Reached area GameOver limit");
			return;
		}
		LoadClockworkState(Path.Combine(ApplicationPaths.persistentDataPath, "LastSave.json"));
		Instance.Runner.Paused = false;
	}
}
