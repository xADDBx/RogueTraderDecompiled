using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.GameCommands;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using UniRx;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

public class SaveManager : IEnumerable<SaveInfo>, IEnumerable, ISaveManagerPostSaveCallback
{
	public enum State
	{
		None,
		Saving,
		Loading
	}

	private class EnumeratorWrapper : IEnumerator<SaveInfo>, IEnumerator, IDisposable
	{
		private readonly IEnumerator<SaveInfo> m_EnumeratorImplementation;

		private readonly object m_Lock;

		public SaveInfo Current => m_EnumeratorImplementation.Current;

		object IEnumerator.Current => ((IEnumerator)m_EnumeratorImplementation).Current;

		public EnumeratorWrapper(IEnumerator<SaveInfo> enumeratorImplementation, object @lock)
		{
			m_EnumeratorImplementation = enumeratorImplementation;
			m_Lock = @lock;
			Monitor.Enter(m_Lock);
		}

		public bool MoveNext()
		{
			return m_EnumeratorImplementation.MoveNext();
		}

		public void Reset()
		{
			m_EnumeratorImplementation.Reset();
		}

		public void Dispose()
		{
			m_EnumeratorImplementation.Dispose();
			Monitor.Exit(m_Lock);
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SaveManager");

	public const string SaveFolderName = "Saved Games";

	private readonly List<SaveInfo> m_SavedGames = new List<SaveInfo>();

	private readonly Queue<SaveInfo> m_SavesToDelete = new Queue<SaveInfo>();

	private Task m_DeleteSavesTask = Task.CompletedTask;

	private int m_QuickSaveCount;

	private SaveInfo m_downgradedIronManSave;

	private static readonly Regex ExtractNumber = new Regex("(Quick|Auto|Manual|ForImport|IronMan|Coop)_(\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private SaveInfo m_IronmanSave;

	private byte[] m_ScreenshotPNGBytes;

	private byte[] m_ScreenshotHighResPNGBytes;

	private const string DialogAutosaveName = "DialogAutosave";

	private readonly object m_Lock = new object();

	private Task m_UpdateTask = Task.FromException(new OperationCanceledException());

	private volatile bool m_CommitInProgress;

	public static readonly string SavePath = Path.Combine(ApplicationPaths.persistentDataPath, "Saved Games");

	public static string CopySaves;

	private static readonly TaskFactory ExclusiveScheduler = new TaskFactory(new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler);

	public List<SaveInfo> SavesQueuedForDeletion => m_SavesToDelete.ToList();

	[Cheat(Name = "allow_save_in_cutscenes_and_dialogs")]
	public static bool AllowSaveInCutscenesAndDialogs { get; set; }

	public SteamSavesReplicator SteamSavesReplicator { get; } = new SteamSavesReplicator();


	public State CurrentState { get; private set; }

	public bool AreSavesUpToDate => m_UpdateTask.IsCompleted;

	public bool SaveListUpdateInProgress => !m_UpdateTask.IsCompleted;

	public bool CommitInProgress => m_CommitInProgress;

	public void UpdateSaveListAsync()
	{
		MainThreadDispatcher.Initialize();
		SteamSavesReplicator.Initialize();
		_ = SaveScreenshotManager.Instance;
		Task updateTask = m_UpdateTask;
		m_UpdateTask = StartUpdatingSaveListImplAsync(updateTask);
		InvokeSaveListUpdated(m_UpdateTask);
		Logger.Log("Started new update task");
	}

	private async Task StartUpdatingSaveListImplAsync(Task prevTask)
	{
		if (EditorSafeThreading.AsyncSaves)
		{
			await Awaiters.ThreadPool;
		}
		try
		{
			await prevTask;
		}
		catch
		{
		}
		try
		{
			await UpdateSaveListTask().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			throw;
		}
	}

	private async Task UpdateSaveListTask()
	{
		using (CodeTimer.New("UpdateSaveListTask"))
		{
			int num2 = 2;
			try
			{
				if (!Directory.Exists(SavePath))
				{
					Directory.CreateDirectory(SavePath);
				}
				string[] directories = Directory.GetDirectories(SavePath, "*", SearchOption.TopDirectoryOnly);
				JsonSerializer serializer = SaveSystemJsonSerializer.Serializer;
				List<SaveInfo> list = (from v in directories
					select LoadFolderSave(serializer, v) into save
					where save != null
					select save).ToList();
				string[] source = Directory.GetFiles(SavePath, "*.zks", SearchOption.TopDirectoryOnly);
				if (BuildModeUtility.IsDevelopment)
				{
					source = source.Concat(Directory.GetFiles(SavePath, "*.zip", SearchOption.TopDirectoryOnly)).ToArray();
				}
				list.AddRange(from v in source
					select LoadZipSave(serializer, v) into save
					where save != null
					select save);
				await Awaiters.ThreadPool;
				SteamSavesReplicator.PullUpdates();
				if (Application.isEditor)
				{
					Task delay = Task.Delay(TimeSpan.FromSeconds(1.0));
					int i = 0;
					while (i < 10)
					{
						await Task.Yield();
						int num = i + 1;
						i = num;
					}
					await delay;
				}
				lock (m_Lock)
				{
					foreach (SaveInfo s in m_SavedGames)
					{
						SaveInfo saveInfo = list.FirstOrDefault((SaveInfo save) => save.SaveId == s.SaveId);
						if (saveInfo != null)
						{
							saveInfo.Screenshot = s.Screenshot;
							saveInfo.ScreenshotHighRes = s.ScreenshotHighRes;
						}
						else
						{
							s.Dispose();
						}
					}
					m_SavedGames.Clear();
					m_SavedGames.AddRange(list);
				}
				CopySaves = null;
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				throw;
			}
		}
	}

	private static async void InvokeSaveListUpdated(Task taskToWait)
	{
		try
		{
			await taskToWait;
		}
		catch (Exception ex)
		{
			Logger.Error(ex);
		}
		try
		{
			EventBus.RaiseEvent(delegate(ISavesUpdatedHandler h)
			{
				h.OnSaveListUpdated();
			});
		}
		catch (Exception ex2)
		{
			Logger.Error(ex2);
		}
	}

	public static void WaitCommit()
	{
		while (Game.Instance.SaveManager.CommitInProgress)
		{
			Thread.Sleep(100);
		}
	}

	[CanBeNull]
	public static SaveInfo LoadZipSave(JsonSerializer serializer, string file)
	{
		try
		{
			ISaver saver2;
			if (!File.Exists(file))
			{
				ISaver saver = new FolderSaver(file);
				saver2 = saver;
			}
			else
			{
				ISaver saver = new ZipSaver(file, ISaver.Mode.Read);
				saver2 = saver;
			}
			using ISaver saver3 = saver2;
			string text = saver3.ReadHeader();
			if (text == null)
			{
				Logger.Warning("Save file {0} looks broken: cannot read header", file);
				return null;
			}
			SaveInfo saveInfo = serializer.DeserializeObject<SaveInfo>(text);
			if (!Application.isEditor && !BuildModeUtility.IsDevelopment && saveInfo.CompatibilityVersion < 1)
			{
				return null;
			}
			saveInfo.FolderName = file;
			saveInfo.Saver = saver3;
			saveInfo.GameId = saveInfo.GameId ?? saveInfo.PlayerCharacterName;
			return saveInfo;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Save folder {0} looks broken: cannot deserialize header file", file);
		}
		return null;
	}

	[CanBeNull]
	public static SaveInfo LoadFolderSave(JsonSerializer jsonSerializer, string folder)
	{
		if (!File.Exists(Path.Combine(folder, "header.json")))
		{
			Logger.Warning("Save folder {0} looks broken: no header file", folder);
			return null;
		}
		FolderSaver folderSaver = new FolderSaver(folder);
		try
		{
			SaveInfo saveInfo = jsonSerializer.DeserializeObject<SaveInfo>(folderSaver.ReadHeader());
			saveInfo.FolderName = folder;
			saveInfo.Saver = folderSaver;
			saveInfo.GameId = saveInfo.GameId ?? saveInfo.PlayerCharacterName;
			return saveInfo;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Save folder {0} looks broken: cannot deserialize header file", folder);
			return null;
		}
	}

	public void UpdateSaveListIfNeeded(bool force = false)
	{
		if (!m_UpdateTask.IsCompletedSuccessfully || force)
		{
			Logger.Log($"UpdateSaveListIfNeeded, (m_UpdateTask == null) is in {m_UpdateTask.Status} status");
			UpdateSaveListAsync();
		}
		try
		{
			m_UpdateTask.Wait();
		}
		catch
		{
		}
	}

	public SaveInfo CreateNewSave(string name, bool extended = false)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = ((Game.Instance.CurrentlyLoadedArea != null) ? Game.Instance.CurrentlyLoadedArea.AreaName : UIStrings.Instance.SaveLoadTexts.SaveDefaultName);
		}
		SaveInfo saveInfo = new SaveInfo
		{
			Name = MakeNameUnique(name),
			SaveId = Guid.NewGuid().ToString("N"),
			Area = Game.Instance.Player.SavedInArea,
			AreaPart = Game.Instance.Player.SavedInAreaPart,
			QuestWithSaveDescription = GetQuestWithSaveDescription(),
			PlayerCharacterName = (Game.Instance.Player.MainCharacterOriginalEntity?.CharacterName ?? "Unnamed"),
			PlayerCharacterRank = (Game.Instance.Player.MainCharacterOriginalEntity?.Progression.CharacterLevel ?? 0),
			GameSaveTime = Game.Instance.TimeController.GameTime,
			GameSaveTimeText = "",
			GameId = Game.Instance.Player.GameId,
			GameTotalTime = Game.Instance.TimeController.RealTime,
			Type = SaveInfo.SaveType.Manual
		};
		if (extended)
		{
			List<BaseUnitEntity> list = new List<BaseUnitEntity>();
			UIUtility.GetActualGroup(list);
			saveInfo.PartyPortraits = list.Select((BaseUnitEntity r) => ExtractPortrait(r.ToBaseUnitEntity())).ToList();
		}
		try
		{
			saveInfo.GameStartSystemTime = Game.Instance.Player.StartDate;
			saveInfo.SystemSaveTime = DateTime.Now;
		}
		catch (Exception ex)
		{
			Logger.Error("Exception in DateTime: " + ex.Message);
			saveInfo.SystemSaveTime = DateTime.MinValue;
		}
		return saveInfo;
	}

	public SaveInfo GetNextQuickslot()
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		SaveInfo saveInfo = null;
		int num = 0;
		int num2 = SettingsRoot.Game.Save.QuicksaveSlots;
		lock (m_Lock)
		{
			string gameId = Game.Instance.Player.GameId;
			if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
			{
				return GetIronmanSave();
			}
			foreach (SaveInfo item in m_SavedGames.Where((SaveInfo s) => s.Type == SaveInfo.SaveType.Quick && s.GameId == Game.Instance.Player.GameId).TakeLast(num2))
			{
				if (item.Type == SaveInfo.SaveType.Quick && item.GameId.Equals(gameId))
				{
					if (saveInfo == null || saveInfo.SystemSaveTime > item.SystemSaveTime)
					{
						saveInfo = item;
					}
					num++;
				}
			}
			if (num < num2)
			{
				saveInfo = CreateNewSave(string.Concat(UIStrings.Instance.SaveLoadTexts.SavePrefixQuick, (num + 1).ToString()));
				saveInfo.Type = SaveInfo.SaveType.Quick;
			}
		}
		return saveInfo;
	}

	private string MakeNameUnique(string name)
	{
		lock (m_Lock)
		{
			string saveNameCandidate = name;
			int num = 1;
			while (Enumerable.FirstOrDefault(m_SavedGames, (SaveInfo s) => s.Name == saveNameCandidate) != null)
			{
				saveNameCandidate = name + " " + num;
				num++;
				if (num > 10000)
				{
					return null;
				}
			}
			return saveNameCandidate;
		}
	}

	public SaveInfo GetNextAutoslot()
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			return GetIronmanSave();
		}
		SaveInfo saveInfo = null;
		lock (m_Lock)
		{
			int num = 0;
			int num2 = SettingsRoot.Game.Save.AutosaveSlots;
			foreach (SaveInfo item in m_SavedGames.Where((SaveInfo s) => s.Type == SaveInfo.SaveType.Auto && s.GameId == Game.Instance.Player.GameId).TakeLast(num2))
			{
				if (item.Type == SaveInfo.SaveType.Auto && item.GameId == Game.Instance.Player.GameId)
				{
					if (saveInfo == null || saveInfo.SystemSaveTime > item.SystemSaveTime)
					{
						saveInfo = item;
					}
					num++;
				}
			}
			int num3 = (BuildModeUtility.IsDevelopment ? 100000 : num2);
			if (num < num3 || saveInfo == null)
			{
				Regex regex = new Regex("\\d+", RegexOptions.Compiled);
				int result;
				List<int> list = (from save in m_SavedGames
					where save.Type == SaveInfo.SaveType.Auto && save.GameId == Game.Instance.Player.GameId && regex.IsMatch(save.Name)
					select regex.Matches(save.Name)[0].Value into saveNumberString
					select int.TryParse(saveNumberString, out result) ? result : 0).ToList();
				int i;
				for (i = 1; list.Contains(i); i++)
				{
				}
				saveInfo = CreateNewSave(string.Concat(UIStrings.Instance.SaveLoadTexts.SavePrefixAuto, i.ToString()));
				saveInfo.Type = SaveInfo.SaveType.Auto;
			}
			saveInfo.IsAutoLevelupSave = false;
			return saveInfo;
		}
	}

	public SaveInfo GetSpecialImportSlot()
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.Type == SaveInfo.SaveType.ForImport && savedGame.Campaign == Game.Instance.Player.Campaign && savedGame.GameId == Game.Instance.Player.GameId)
				{
					return savedGame;
				}
			}
			SaveInfo saveInfo = CreateNewSave(Game.Instance.Player.MainCharacter.Entity?.CharacterName ?? "(_import_)");
			saveInfo.Type = SaveInfo.SaveType.ForImport;
			return saveInfo;
		}
	}

	public SaveInfo GetNewestQuickslot()
	{
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			return GetLatestIronManSave((SaveInfo s) => s.GameId == Game.Instance.Player.GameId);
		}
		return GetLatestSave((SaveInfo s) => s.Type == SaveInfo.SaveType.Quick && s.GameId == Game.Instance.Player.GameId);
	}

	public SaveInfo GetLatestSave([CanBeNull] Func<SaveInfo, bool> predicate = null)
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		SaveInfo saveInfo = null;
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.IsActuallySaved && savedGame.Type != SaveInfo.SaveType.ForImport && !IsCoopSave(savedGame) && savedGame.CheckDlcAvailable() && (predicate == null || predicate(savedGame)) && (saveInfo == null || saveInfo.SystemSaveTime < savedGame.SystemSaveTime))
				{
					saveInfo = savedGame;
				}
			}
			return saveInfo;
		}
	}

	public SaveInfo GetLatestIronManSave([CanBeNull] Func<SaveInfo, bool> predicate = null)
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		SaveInfo saveInfo = null;
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.IsActuallySaved && savedGame.Type == SaveInfo.SaveType.IronMan && !IsCoopSave(savedGame) && savedGame.CheckDlcAvailable() && (predicate == null || predicate(savedGame)) && (saveInfo == null || saveInfo.SystemSaveTime < savedGame.SystemSaveTime))
				{
					saveInfo = savedGame;
				}
			}
			return saveInfo;
		}
	}

	public SaveInfo GetAnyLatestSave([CanBeNull] Func<SaveInfo, bool> predicate = null)
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		SaveInfo saveInfo = null;
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.IsActuallySaved && savedGame.Type != SaveInfo.SaveType.ForImport && !IsCoopSave(savedGame) && savedGame.CheckDlcAvailable() && (predicate == null || predicate(savedGame)) && (saveInfo == null || saveInfo.SystemSaveTime < savedGame.SystemSaveTime))
				{
					saveInfo = savedGame;
				}
			}
			return saveInfo;
		}
	}

	public bool HasAnySaves(bool includingCorrupted = false)
	{
		lock (m_Lock)
		{
			for (int i = 0; i < m_SavedGames.Count; i++)
			{
				SaveInfo saveInfo = m_SavedGames[i];
				if (saveInfo.IsActuallySaved && saveInfo.Type != SaveInfo.SaveType.ForImport && !IsCoopSave(saveInfo))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsIronmanSave(SaveInfo save)
	{
		if (save.Type == SaveInfo.SaveType.IronMan)
		{
			return save.GameId == Game.Instance.Player.GameId;
		}
		return false;
	}

	public SaveInfo GetIronmanSave()
	{
		if (m_IronmanSave == null || m_IronmanSave.GameId != Game.Instance.Player.GameId)
		{
			m_IronmanSave = GetIronManSave(Game.Instance.Player.GameId);
			m_IronmanSave.Type = SaveInfo.SaveType.IronMan;
		}
		return m_IronmanSave;
	}

	private SaveInfo GetIronManSave(string gameId)
	{
		SaveInfo saveInfo = GetLatestIronManSave((SaveInfo s) => s.GameId == gameId);
		if (saveInfo == null)
		{
			saveInfo = CreateNewSave(string.Concat(UIStrings.Instance.SaveLoadTexts.SavePrefixIronman, "_", gameId));
			saveInfo.Type = SaveInfo.SaveType.IronMan;
		}
		return saveInfo;
	}

	public SaveInfo GetDialogSaveSlot()
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.Name == "DialogAutosave")
				{
					return savedGame;
				}
			}
			return CreateNewSave("DialogAutosave");
		}
	}

	public void RequestDeleteSave(SaveInfo saveInfo)
	{
		PFLog.Default.Log("Request deletion " + saveInfo.FolderName);
		m_SavesToDelete.Enqueue(saveInfo);
		if (m_DeleteSavesTask.IsCompleted)
		{
			m_DeleteSavesTask = DeleteSavesAsync();
		}
	}

	private async Task DeleteSavesAsync()
	{
		if (m_SavesToDelete.Count > 0)
		{
			while (!AreSavesUpToDate)
			{
				await Task.Yield();
			}
			SaveInfo saveInfo = m_SavesToDelete.Dequeue();
			DeleteSave(saveInfo);
		}
	}

	public void DeleteSave(SaveInfo saveInfo)
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		lock (m_Lock)
		{
			Logger.Log("Deleting Save: " + saveInfo.FolderName + "..");
			if (saveInfo.IsActuallySaved)
			{
				saveInfo.Saver.Clear();
				SteamSavesReplicator.DeleteSave(saveInfo);
			}
			saveInfo.FolderName = null;
			saveInfo.Dispose();
			m_SavedGames.Remove(saveInfo);
		}
	}

	public void DowngradeSaveFromIronMan(SaveInfo saveInfo)
	{
		if (saveInfo.Type == SaveInfo.SaveType.IronMan)
		{
			Game.Instance.LoadGame(saveInfo, delegate
			{
				PFLog.System.Log("Loading ironman save: " + saveInfo.FolderName);
				SettingsRoot.Difficulty.OnlyOneSave.SetValueAndConfirm(value: false);
				PFLog.System.Log("Deleting ironman save: " + saveInfo.FolderName);
				DeleteSave(saveInfo);
				PFLog.System.Log("Creating autosave instead of ironman:  " + saveInfo.FolderName);
				Game.Instance.SaveGame(Game.Instance.SaveManager.GetNextAutoslot());
			});
		}
	}

	public void LoadScreenshot(SaveInfo save, bool highRes, Action callback)
	{
		if (save.Type == SaveInfo.SaveType.ForImport)
		{
			save.Screenshot = save.Campaign?.DlcReward?.ScreenshotForImportSave;
			callback();
		}
		else if ((highRes && (bool)save.ScreenshotHighRes) || (!highRes && (bool)save.Screenshot))
		{
			callback?.Invoke();
		}
		else if (save.IsActuallySaved)
		{
			SaveScreenshotManager.Instance.LoadScreenshot(save, highRes, callback);
		}
		else if (highRes)
		{
			save.ScreenshotHighRes = SaveScreenshotManager.MakeScreenshotHighResOnly();
			callback?.Invoke();
		}
		else
		{
			save.Screenshot = SaveScreenshotManager.MakeScreenshotLowResOnly();
			callback?.Invoke();
		}
	}

	public int FindUnusedSaveNumber(SaveInfo.SaveType type)
	{
		int num = 0;
		lock (m_Lock)
		{
			foreach (SaveInfo savedGame in m_SavedGames)
			{
				if (savedGame.IsActuallySaved && savedGame.Type == type)
				{
					num = CheckNumber(Path.GetFileNameWithoutExtension(savedGame.FolderName), num);
				}
			}
		}
		return num + 1;
		static int CheckNumber(string folderName, int currentMax)
		{
			if (int.TryParse(ExtractNumber.Match(folderName).Groups[2].Value, out var result))
			{
				currentMax = Math.Max(currentMax, result);
			}
			return currentMax;
		}
	}

	private void GenerateFolderName(SaveInfo save)
	{
		int num = FindUnusedSaveNumber(save.Type);
		string path;
		if (save.Type == SaveInfo.SaveType.Manual)
		{
			string arg = Regex.Replace(save.Name, "[^a-zA-Z0-9]", "_");
			path = $"{save.Type}_{num}_{arg}";
		}
		else
		{
			path = $"{save.Type}_{num}";
		}
		save.FolderName = Path.Combine(Path.Combine(ApplicationPaths.persistentDataPath, "Saved Games"), path);
	}

	private void GenerateTempFolderName(SaveInfo save)
	{
		FindUnusedSaveNumber(save.Type);
		save.FolderName = Path.Combine(ApplicationPaths.temporaryCachePath, save.SaveId);
	}

	private static void SetUpSaver(SaveInfo save)
	{
		if (true)
		{
			save.FolderName += ".zks";
			save.Saver = new ZipSaver(save.FolderName);
		}
		else
		{
			save.Saver = new FolderSaver(save.FolderName);
		}
	}

	private void PrepareSave(SaveInfo save)
	{
		if (save.Type == SaveInfo.SaveType.Quick)
		{
			save.QuickSaveNumber = ++m_QuickSaveCount;
		}
		if ((bool)save.Screenshot)
		{
			UnityEngine.Object.Destroy(save.Screenshot);
			save.Screenshot = null;
		}
		if ((bool)save.ScreenshotHighRes)
		{
			UnityEngine.Object.Destroy(save.ScreenshotHighRes);
			save.ScreenshotHighRes = null;
		}
		save.Area = Game.Instance.Player.SavedInArea ?? Game.Instance.Player.NextEnterPoint.Area;
		save.AreaPart = Game.Instance.Player.SavedInAreaPart ?? Game.Instance.Player.NextEnterPoint.AreaPart;
		save.GameSaveTime = Game.Instance.TimeController.GameTime;
		save.GameSaveTimeText = "";
		save.GameStartSystemTime = Game.Instance.Player.StartDate;
		save.GameId = Game.Instance.Player.GameId;
		save.SaveId = ((!string.IsNullOrEmpty(save.SaveId)) ? save.SaveId : Guid.NewGuid().ToString("N"));
		save.Campaign = Game.Instance.Player.Campaign;
		try
		{
			save.SystemSaveTime = DateTime.Now;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Exception in DateTime");
			save.SystemSaveTime = DateTime.MinValue;
		}
		save.PlayerCharacterName = Game.Instance.Player.MainCharacterOriginalEntity?.CharacterName ?? "Unnamed";
		save.PlayerCharacterRank = Game.Instance.Player.MainCharacterOriginalEntity?.Progression.CharacterLevel ?? 0;
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		UIUtility.GetActualGroup(list);
		save.PartyPortraits = list.Select((BaseUnitEntity r) => ExtractPortrait(r)).ToList();
		save.GameTotalTime = Game.Instance.Player.RealTime;
		save.QuestWithSaveDescription = GetQuestWithSaveDescription();
		save.Versions = JsonUpgradeSystem.KnownVersions;
		save.DlcRewards = Game.Instance.Player.DlcRewardsToSave;
		PFStatefulRandom.GetStates(ref save.StatefulRandomStates);
	}

	private static PortraitForSave ExtractPortrait(BaseUnitEntity u)
	{
		if (!u.UISettings.PortraitBlueprint)
		{
			return new PortraitForSave(u.UISettings.Portrait, u.IsMainCharacter);
		}
		return new PortraitForSave(u.UISettings.PortraitBlueprint, u.IsMainCharacter);
	}

	private BlueprintQuest GetQuestWithSaveDescription()
	{
		return (from q in Game.Instance.Player.QuestBook.Quests
			where q.State == QuestState.Started
			select q.Blueprint).Aggregate(null, (BlueprintQuest best, BlueprintQuest q) => (q.DescriptionPriority <= (best ? best.DescriptionPriority : 0)) ? best : q);
	}

	public bool IsSaveAllowed(SaveInfo.SaveType saveType, bool isMainMenu = false)
	{
		if (Game.Instance.CurrentlyLoadedArea == null && !isMainMenu && !Game.Instance.Player.NextEnterPoint)
		{
			return false;
		}
		if (Game.Instance.Player.GameOverReason.HasValue)
		{
			return false;
		}
		if (saveType == SaveInfo.SaveType.Bugreport)
		{
			return true;
		}
		if (Game.Instance.RootUiContext.IsGroupChangerWindowShow)
		{
			return false;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			if (Game.Instance.TurnController.IsPreparationTurn)
			{
				return true;
			}
			if (Game.Instance.TurnController.IsAiTurn)
			{
				return false;
			}
			PartUnitCommands partUnitCommands = Game.Instance.TurnController.CurrentUnit?.GetCommandsOptional();
			if (partUnitCommands == null || !partUnitCommands.Empty)
			{
				return false;
			}
		}
		if (Game.Instance.IsModeActive(GameModeType.Dialog) || Game.Instance.IsModeActive(GameModeType.Cutscene))
		{
			return AllowSaveInCutscenesAndDialogs;
		}
		return true;
	}

	public IEnumerator<object> SaveRoutine([NotNull] SaveInfo saveInfo, bool forceAuto = false, bool showNotification = false)
	{
		if (!IsSaveAllowed(saveInfo.Type) && saveInfo.Type != SaveInfo.SaveType.ForImport)
		{
			Logger.Error("Triggered save when saving is not allowed!");
			yield break;
		}
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave && !IsIronmanSave(saveInfo) && saveInfo.Type != SaveInfo.SaveType.ForImport && saveInfo.Type != SaveInfo.SaveType.Bugreport)
		{
			Logger.Error("Triggered save to non-ironman slot!");
			yield break;
		}
		if (!forceAuto && saveInfo.Type == SaveInfo.SaveType.Auto && !SettingsRoot.Game.Save.AutosaveEnabled)
		{
			Logger.Log("Autosave option is disabled. Skipping auto saving, but calling PreSave...");
			SaveGameCommand.PreSaveGame(saveInfo.Type);
			yield break;
		}
		if (showNotification)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.GameSavedInProgress);
			});
		}
		CurrentState = State.Saving;
		Game.Instance.EntitySpawner.Tick();
		Game.Instance.EntityDestroyer.Tick();
		yield return null;
		while (m_CommitInProgress || saveInfo.OperationState != 0)
		{
			yield return null;
		}
		while (SaveScreenshotManager.Instance.HasTasksRunning)
		{
			yield return null;
		}
		lock (m_Lock)
		{
			if (!m_SavedGames.Contains(saveInfo))
			{
				m_SavedGames.Add(saveInfo);
			}
		}
		LoadingProcess.Instance.SetLockedNotification(WarningNotificationType.GameSavedInProgress);
		Game.Instance.Player.SavedInArea = Game.Instance.CurrentlyLoadedArea;
		Game.Instance.Player.SavedInAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		SaveInfo originalSave = saveInfo;
		if (!originalSave.IsActuallySaved)
		{
			GenerateFolderName(originalSave);
			SetUpSaver(originalSave);
		}
		using (ProfileScope.New("Prepare Save"))
		{
			saveInfo = new SaveInfo
			{
				Type = originalSave.Type,
				Name = originalSave.Name,
				SaveId = originalSave.SaveId
			};
			GenerateTempFolderName(saveInfo);
			SetUpSaver(saveInfo);
			PrepareSave(saveInfo);
		}
		lock (m_Lock)
		{
			m_SavedGames.Remove(originalSave);
			m_SavedGames.Add(saveInfo);
			if (m_IronmanSave == originalSave)
			{
				m_IronmanSave = saveInfo;
			}
		}
		if (saveInfo.Type != SaveInfo.SaveType.ForImport)
		{
			using (ProfileScope.New("Make Screenshot"))
			{
				(saveInfo.ScreenshotHighRes, saveInfo.Screenshot) = SaveScreenshotManager.MakeScreenshot();
			}
		}
		try
		{
			if (saveInfo.Type != SaveInfo.SaveType.ForImport)
			{
				m_ScreenshotHighResPNGBytes = saveInfo.ScreenshotHighRes.EncodeToPNG();
				UnityEngine.Object.Destroy(saveInfo.ScreenshotHighRes);
				saveInfo.ScreenshotHighRes = null;
				m_ScreenshotPNGBytes = saveInfo.Screenshot.EncodeToPNG();
				SaveScreenshotManager.CompressScreenshot(saveInfo.Screenshot);
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.SavingFailed, addToLog: false, WarningNotificationFormat.Short);
			});
			CurrentState = State.None;
			yield break;
		}
		PreSave();
		if ((bool)Game.Instance.CurrentlyLoadedArea)
		{
			Task encodeFogOp = AreaDataStash.EncodeActiveAreaFog(Game.Instance.State.LoadedAreaState);
			while (!encodeFogOp.IsCompleted)
			{
				yield return null;
			}
		}
		_ = GameVersion.Revision;
		SaveCreateDTO dto;
		using (CodeTimer.New("SaveCreateDTO.Build"))
		{
			dto = SaveCreateDTO.Build(saveInfo, Game.Instance.Player);
		}
		GC.Collect();
		Game.Instance.Player.NextEnterPoint = null;
		Logger.Log("Begin threaded save");
		Task saveTask = SerializeAndSaveThread(saveInfo, dto, originalSave);
		WaitForSeconds wait = new WaitForSeconds(0.15f);
		while (!saveTask.IsCompleted)
		{
			yield return wait;
		}
		try
		{
			saveTask.Wait();
		}
		catch (Exception ex2)
		{
			Logger.Exception(ex2);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.SavingFailed, addToLog: false, WarningNotificationFormat.Short);
			});
			CurrentState = State.None;
			yield break;
		}
		m_ScreenshotPNGBytes = null;
		m_ScreenshotHighResPNGBytes = null;
		Logger.Log("Finished wait for threaded save");
		CurrentState = State.None;
	}

	public void PreSave()
	{
		using (ContextData<GameLogDisabled>.Request())
		{
			Game.Instance.State.LoadedAreaState?.PreSave();
			Game.Instance.Player.PreSave();
		}
	}

	private static void GCCollect()
	{
		StartupJson data = BuildModeUtility.Data;
		if (data != null && data.ForceGCOnSaves)
		{
			GC.Collect();
		}
	}

	private async Task SerializeAndSaveThread([NotNull] SaveInfo saveInfo, SaveCreateDTO dto, [NotNull] SaveInfo originalSave)
	{
		Logger.Log("SerializeAndSaveThread: started");
		StringBuilder checksumStringBuilder = new StringBuilder();
		try
		{
			saveInfo.OperationState = SaveInfo.StateType.Serializing;
			using (saveInfo.GetWriteScope(writeOnly: true))
			{
				saveInfo.Saver.Clear();
				Logger.Log("Saving to {0}", saveInfo.FolderName);
				saveInfo.Saver.SaveJson("header", SaveSystemJsonSerializer.Serializer.SerializeObject(saveInfo));
				if (saveInfo.Type != SaveInfo.SaveType.ForImport)
				{
					saveInfo.Saver.SaveBytes("highres.png", m_ScreenshotHighResPNGBytes);
					saveInfo.Saver.SaveBytes("header.png", m_ScreenshotPNGBytes);
				}
				Task playerTask = SerializePlayer(Game.Instance.Player);
				Task partyTask = SerializeParty(Game.Instance.Player.CrossSceneState);
				AreaPersistentState loadedAreaState = Game.Instance.State.LoadedAreaState;
				Task areaTask = ((saveInfo.Type != SaveInfo.SaveType.ForImport && loadedAreaState != null) ? SerializeLocalAreaState(loadedAreaState) : Task.CompletedTask);
				AreaDataStash.GameHistoryFile.DisableWrite = true;
				GameStatistic.AppStatus appStatus = new GameStatistic.AppStatus();
				Game.Instance.Statistic.PreSave(appStatus);
				Task<string> statTask = SerializeStatistics(saveInfo, appStatus);
				Task<string> settingsTask = SerializeInGameSettings(Game.Instance.State.InGameSettings);
				Game.Instance.CoopData.PreSave();
				Task<string> coopTask = SerializeCoopData(Game.Instance.CoopData);
				if (saveInfo.Type != SaveInfo.SaveType.ForImport)
				{
					await SaveStashedAreas(saveInfo, checksumStringBuilder);
				}
				Logger.Log("await Task.WhenAll");
				await Task.WhenAll(SavePlayer(saveInfo, playerTask, checksumStringBuilder), SaveArea(saveInfo, areaTask, checksumStringBuilder), SaveParty(saveInfo, partyTask, checksumStringBuilder), SaveStatistics(saveInfo, statTask), SaveSettings(saveInfo, settingsTask), SaveCoop(saveInfo, coopTask));
				Logger.Log("Task.WhenAll Done");
				if (ReportingCheats.IsDebugSaveFileChecksumEnabled)
				{
					byte[] bytes = Encoding.Default.GetBytes(checksumStringBuilder.ToString());
					saveInfo.Saver.SaveBytes("checksums", bytes);
				}
				AreaDataStash.GameHistoryFile.DisableWrite = false;
				m_CommitInProgress = true;
				saveInfo.OperationState = SaveInfo.StateType.Saving;
				Logger.Log("Commit save");
				using (CodeTimer.New("Commit save"))
				{
					saveInfo.Saver.Save();
				}
				Logger.Log("Commit done");
				AreaDataStash.ClearJsonForArea("player", "");
				AreaDataStash.ClearJsonForArea("party", "");
				originalSave.Saver.Clear();
				saveInfo.Saver.MoveTo(originalSave.FolderName);
				saveInfo.FolderName = originalSave.FolderName;
				saveInfo.OperationState = SaveInfo.StateType.None;
				m_CommitInProgress = false;
				Game.Instance.GameCommandQueue.SchedulePostSaveCallback(saveInfo, dto);
				Logger.Log("Saved successfully to {0}", saveInfo.FolderName);
			}
		}
		catch (Exception)
		{
			saveInfo.OperationState = SaveInfo.StateType.None;
			m_CommitInProgress = false;
			saveInfo.FolderName = null;
			AreaDataStash.GameHistoryFile.DisableWrite = false;
			try
			{
				saveInfo.Saver.Clear();
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
			throw;
		}
		finally
		{
			LoadingProcess.Instance.ClearLockedNotification();
		}
		Logger.Log("SerializeAndSaveThread: finished");
		static async Task SaveArea(SaveInfo saveInfo, Task areaTask, StringBuilder checksum)
		{
			Logger.Log("SaveArea.await playerTask");
			await areaTask;
			Logger.Log("SaveArea.await StartNew");
			await ExclusiveScheduler.StartNew(delegate
			{
				if (saveInfo.Type == SaveInfo.SaveType.ForImport || Game.Instance.State.LoadedAreaState == null)
				{
					Logger.Log("SaveArea early out");
				}
				else
				{
					Logger.Log("SaveArea SaveStashedArea");
					AreaPersistentState loadedAreaState2 = Game.Instance.State.LoadedAreaState;
					SaveStashedArea(saveInfo, loadedAreaState2, checksum);
					Logger.Log("SaveArea StartNew Done");
				}
			});
			Logger.Log("SaveArea Done");
		}
		static async Task SaveCoop(SaveInfo saveInfo, Task<string> coopTask)
		{
			Logger.Log("SaveCoop.await coopTask");
			string coopSave = await coopTask;
			if (string.IsNullOrEmpty(coopSave))
			{
				Logger.Log("SaveCoop early out");
			}
			else
			{
				Logger.Log("SaveCoop.await StartNew");
				await ExclusiveScheduler.StartNew(delegate
				{
					Logger.Log("SaveCoop SaveJson");
					saveInfo.Saver.SaveJson("coop", coopSave);
					Logger.Log("SaveCoop StartNew Done");
				});
				Logger.Log("SaveCoop Done");
			}
		}
		static async Task SaveParty(SaveInfo saveInfo, Task partyTask, StringBuilder checksum)
		{
			Logger.Log("SaveParty.await partyTask");
			await partyTask;
			Logger.Log("SaveParty.await StartNew");
			await ExclusiveScheduler.StartNew(delegate
			{
				Logger.Log("SaveParty CopyFromStash");
				if (!saveInfo.Saver.CopyFromStash(AreaDataStash.FileName("party", "")))
				{
					throw new Exception("Cant save party.json");
				}
				Logger.Log("SaveParty AppendSaveFilesMd5");
				AppendSaveFilesMd5(checksum, "party.json");
				Logger.Log("SaveParty StartNew Done");
			});
			Logger.Log("SaveParty Done");
		}
		static async Task SavePlayer(SaveInfo saveInfo, Task playerTask, StringBuilder checksum)
		{
			Logger.Log("SavePlayer.await playerTask");
			await playerTask;
			Logger.Log("SavePlayer.await StartNew");
			await ExclusiveScheduler.StartNew(delegate
			{
				Logger.Log("SavePlayer CopyFromStash");
				if (!saveInfo.Saver.CopyFromStash(AreaDataStash.FileName("player", "")))
				{
					throw new Exception("Cant save player.json");
				}
				Logger.Log("SavePlayer AppendSaveFilesMd5");
				AppendSaveFilesMd5(checksum, "player.json");
				Logger.Log("SavePlayer StartNew Done");
			});
			Logger.Log("SavePlayer Done");
		}
		static async Task SaveSettings(SaveInfo saveInfo, Task<string> settingsTask)
		{
			Logger.Log("SaveSettings.await settingsTask");
			string settingsSave = await settingsTask;
			Logger.Log("SaveSettings.await StartNew");
			await ExclusiveScheduler.StartNew(delegate
			{
				Logger.Log("SaveSettings SaveJson");
				saveInfo.Saver.SaveJson("settings", settingsSave);
				Logger.Log("SaveSettings StartNew Done");
			});
			Logger.Log("SaveSettings Done");
		}
		static async Task SaveStatistics(SaveInfo saveInfo, Task<string> statTask)
		{
			Logger.Log("SaveStatistics.await statTask");
			string statisticSave = await statTask;
			Logger.Log("SaveStatistics.await StartNew");
			await ExclusiveScheduler.StartNew(delegate
			{
				Logger.Log("SaveStatistics SaveJson");
				saveInfo.Saver.SaveJson("statistic", statisticSave);
				Logger.Log("SaveStatistics CopyFromStash");
				saveInfo.Saver.CopyFromStash("history");
				Logger.Log("SaveStatistics StartNew Done");
			});
			Logger.Log("SaveStatistics Done");
		}
	}

	private static async Task SaveStashedAreas(SaveInfo saveInfo, StringBuilder checksum)
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New("Save stashed areas"))
		{
			int num = 0;
			foreach (AreaPersistentState savedAreaState in Game.Instance.State.SavedAreaStates)
			{
				if (savedAreaState != Game.Instance.State.LoadedAreaState)
				{
					SaveStashedArea(saveInfo, savedAreaState, checksum);
					num++;
				}
			}
			Logger.Log($"#### Saved {num} areas");
		}
	}

	private static async Task SerializePlayer(Player player)
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New("Serializing player state"))
		{
			GCCollect();
			JsonSerializer jsonSerializer = JsonSerializer.Create(SaveSystemJsonSerializer.Settings);
			jsonSerializer.Formatting = AreaDataStash.Formatting;
			await using (StreamWriter textWriter = new StreamWriter(AreaDataStash.Path("player", "")))
			{
				using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
				jsonSerializer.Serialize(jsonWriter, player);
			}
			GCCollect();
		}
	}

	private static async Task SerializeParty(SceneEntitiesState css)
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New("Serializing cross-scene state"))
		{
			GCCollect();
			JsonSerializer serializer = SaveSystemJsonSerializer.Serializer;
			serializer.Formatting = AreaDataStash.Formatting;
			await using (StreamWriter textWriter = new StreamWriter(AreaDataStash.Path("party", "")))
			{
				using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
				serializer.Serialize(jsonWriter, css);
			}
			GCCollect();
		}
	}

	private static async Task SerializeLocalAreaState(AreaPersistentState areaPersistentState)
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New("Serializing local area state"))
		{
			GCCollect();
			AreaDataStash.StashAreaState(areaPersistentState, dispose: false);
			GCCollect();
		}
	}

	private static async Task<string> SerializeStatistics(SaveInfo saveInfo, GameStatistic.AppStatus appStatus)
	{
		await EditorSafeThreading.Awaitable;
		return GameStatistic.Serialize(saveInfo, appStatus);
	}

	private static async Task<string> SerializeInGameSettings(InGameSettings stateInGameSettings)
	{
		await EditorSafeThreading.Awaitable;
		return SettingsJsonSerializer.Serializer.SerializeObject(stateInGameSettings);
	}

	private static async Task<string> SerializeCoopData(CoopData instanceCoopData)
	{
		await EditorSafeThreading.Awaitable;
		return CoopSettingsSaveDataSerializer.Serializer.SerializeObject(instanceCoopData);
	}

	void ISaveManagerPostSaveCallback.PostSaveCallback(SaveInfo saveInfo, SaveCreateDTO dto)
	{
		PostSaveCallback(saveInfo, dto);
	}

	private void PostSaveCallback(SaveInfo saveInfo, SaveCreateDTO dto)
	{
		SteamSavesReplicator.RegisterSave(saveInfo);
		WarningNotificationType notificationType = saveInfo.Type switch
		{
			SaveInfo.SaveType.Quick => WarningNotificationType.GameSavedQuick, 
			SaveInfo.SaveType.Auto => WarningNotificationType.GameSavedAuto, 
			_ => WarningNotificationType.GameSaved, 
		};
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(notificationType);
		});
	}

	private static void SaveStashedArea(SaveInfo saveInfo, AreaPersistentState state, StringBuilder checksum)
	{
		string assetGuidThreadSafe = state.Blueprint.AssetGuidThreadSafe;
		saveInfo.Saver.CopyFromStash(AreaDataStash.FileName(assetGuidThreadSafe, ""));
		foreach (SceneEntitiesState additionalSceneState in state.GetAdditionalSceneStates())
		{
			saveInfo.Saver.CopyFromStash(AreaDataStash.FileName(assetGuidThreadSafe, additionalSceneState.SceneName));
			AppendSaveFilesMd5(checksum, AreaDataStash.FileName(assetGuidThreadSafe, additionalSceneState.SceneName));
		}
		foreach (string item in AreaDataStash.EnumerateFogMasks(assetGuidThreadSafe))
		{
			saveInfo.Saver.CopyFromStash(Path.GetFileName(item));
		}
	}

	public IEnumerator<object> LoadRoutine(SaveInfo saveInfo, bool isSmokeTest = false)
	{
		if (!saveInfo.IsActuallySaved)
		{
			Logger.Warning("Cannot load: not saved " + saveInfo.Name);
			yield break;
		}
		while (m_CommitInProgress || saveInfo.OperationState != 0 || !AreSavesUpToDate || CurrentState != 0)
		{
			yield return null;
		}
		CurrentState = State.Loading;
		using (ProfileScope.New("StrictReferenceResolver.Instance.ClearContexts"))
		{
			StrictReferenceResolver.Instance.ClearContexts();
		}
		lock (m_Lock)
		{
			using (saveInfo.GetReadScope(upgradeable: true))
			{
				if (!m_SavedGames.Contains(saveInfo))
				{
					using (saveInfo.Saver)
					{
						string source = saveInfo.Saver.ReadHeader();
						ISaver saver = saveInfo.Saver.Clone();
						string folderName = saveInfo.FolderName;
						saveInfo = SaveSystemJsonSerializer.Serializer.DeserializeObject<SaveInfo>(source);
						saveInfo.Saver = saver;
						saveInfo.FolderName = folderName;
					}
				}
				using (saveInfo.GetReadScope(upgradeable: true))
				{
					using (ProfileScope.New("Dispose State"))
					{
						Game.Instance.DisposeState();
					}
					Game.Instance.State.SavedAreaStates.Clear();
					AreaDataStash.ClearDirectory();
					using (CodeTimer.New("Threaded loading"))
					{
						Task<string> task = ThreadedGameLoader.Load(saveInfo, isSmokeTest);
						while (!task.IsCompleted)
						{
							yield return null;
						}
						string result;
						try
						{
							result = task.Result;
						}
						catch (Exception innerException)
						{
							throw new LoadGameException("Exception in loading thread", innerException);
						}
						using (CodeTimer.New("Statistics non-threaded deserialize"))
						{
							if (result != null)
							{
								saveInfo.Saver.CopyToStash("history");
								GameStatistic.Deserialize(saveInfo, result, new GameStatistic.AppStatus());
							}
						}
					}
				}
			}
		}
		PFStatefulRandom.SetStates(saveInfo.StatefulRandomStates);
		foreach (AreaPersistentState savedAreaState in Game.Instance.State.SavedAreaStates)
		{
			savedAreaState.RestoreAreaBlueprint();
		}
		using (ProfileScope.New("TurnOn Player State"))
		{
			SettingsController.Instance.StartInSaveSettings();
			Game.Instance.State.PlayerState.PostLoad();
			Game.Instance.State.PlayerState.Subscribe();
			Game.Instance.State.PlayerState.CrossSceneState.Subscribe();
			Game.Instance.Player.GameId = saveInfo.GameId;
		}
		if (saveInfo.Type == SaveInfo.SaveType.Quick)
		{
			m_QuickSaveCount = saveInfo.QuickSaveNumber;
		}
		using (ProfileScope.New("Apply Upgrades"))
		{
			Game.Instance.Player.ApplyUpgrades();
		}
		Game.Instance.CoopData.PostLoad();
		PhotonManager.Save.PostLoad();
		using (ProfileScope.New("StrictReferenceResolver.Instance.ClearContexts"))
		{
			StrictReferenceResolver.Instance.ClearContexts();
		}
		SettingsRoot.Difficulty.OnlyOneSave.SetTempValue(saveInfo.Type == SaveInfo.SaveType.IronMan);
		_ = Game.Instance.DefaultUnit;
		saveInfo.Saver.Dispose();
		CurrentState = State.None;
	}

	public IEnumerator<SaveInfo> GetEnumerator()
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		return new EnumeratorWrapper(m_SavedGames.GetEnumerator(), m_Lock);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public SaveInfo GetSaveByFile(string path)
	{
		path = path.ToLowerInvariant();
		lock (m_Lock)
		{
			return m_SavedGames.SingleOrDefault((SaveInfo s) => s.FileName.ToLowerInvariant() == path);
		}
	}

	public void RemoveSaveFromList(SaveInfo saveInfo)
	{
		lock (m_Lock)
		{
			m_SavedGames.Remove(saveInfo);
		}
	}

	private static void AppendSaveFilesMd5(StringBuilder checksum, string fileName)
	{
		if (!ReportingCheats.IsDebugSaveFileChecksumEnabled)
		{
			return;
		}
		try
		{
			using MD5 mD = MD5.Create();
			using FileStream inputStream = File.OpenRead(Path.Combine(AreaDataStash.Folder, fileName));
			string @string = Encoding.Default.GetString(mD.ComputeHash(inputStream));
			checksum.AppendFormat("{0}: {1}\n{2}", fileName, @string, "===========\n");
		}
		catch (Exception ex)
		{
			Logger.Error(ex);
		}
	}

	public bool PrecheckSaveCorruption(SaveInfo saveInfo)
	{
		return true;
	}

	private void ReplaceUserGeneratedContent()
	{
		SceneEntitiesState crossSceneState = Game.Instance.State.PlayerState.CrossSceneState;
		if (crossSceneState == null)
		{
			return;
		}
		foreach (Entity allEntityDatum in crossSceneState.AllEntityData)
		{
			if (!(allEntityDatum is BaseUnitEntity baseUnitEntity))
			{
				continue;
			}
			if (baseUnitEntity.Parts.GetAll<UnitPartMainCharacter>().Any())
			{
				if (!GetDescription(allEntityDatum, out var description2))
				{
					continue;
				}
				string name = BlueprintCharGenRoot.Instance.PregenCharacterNames.ReplaceCharacterNameIfCustom(Race.Human, description2.CustomGender.GetValueOrDefault(), CharGenConfig.CharGenMode.NewGame, description2.CustomName);
				description2.SetName(name);
				FixPetName(baseUnitEntity, description2);
			}
			if (baseUnitEntity.IsCustomCompanion() || baseUnitEntity.IsPregenCustomCompanion())
			{
				if (!GetDescription(allEntityDatum, out var description3))
				{
					continue;
				}
				string name2 = BlueprintCharGenRoot.Instance.PregenCharacterNames.ReplaceCharacterNameIfCustom(Race.Human, description3.CustomGender.GetValueOrDefault(), CharGenConfig.CharGenMode.NewCompanion, description3.CustomName);
				description3.SetName(name2);
				FixPetName(baseUnitEntity, description3);
			}
			if (baseUnitEntity.Parts.GetAll<PartStarship>().Any() && GetDescription(allEntityDatum, out var description4))
			{
				string name3 = BlueprintCharGenRoot.Instance.PregenCharacterNames.ReplaceShipNameIfCustom(description4.CustomName);
				description4.SetName(name3);
			}
		}
		static bool GetDescription(Entity character, out PartUnitDescription description)
		{
			description = character.Parts.GetAll<PartUnitDescription>().FirstOrDefault();
			return description != null;
		}
	}

	private void FixPetName(BaseUnitEntity baseUnit, PartUnitDescription baseUnitDescription)
	{
		if (baseUnit.IsMaster)
		{
			UnitPartPetOwner optional = baseUnit.GetOptional<UnitPartPetOwner>();
			optional.PetUnit.Description?.SetName(baseUnitDescription.CustomPetName = BlueprintCharGenRoot.Instance.PregenCharacterNames.ReplacePetNameIfCustom(optional.PetType, baseUnitDescription.CustomPetName));
		}
	}

	public bool TryFind(SaveInfoKey saveInfoKey, out SaveInfo saveInfo)
	{
		if (!AreSavesUpToDate)
		{
			Logger.Error("Saves are not up to date");
		}
		lock (m_Lock)
		{
			int i = 0;
			for (int count = m_SavedGames.Count; i < count; i++)
			{
				SaveInfo saveInfo2 = m_SavedGames[i];
				if (saveInfoKey.IsFit(saveInfo2))
				{
					saveInfo = saveInfo2;
					return true;
				}
			}
			saveInfo = null;
			return false;
		}
	}

	public static bool IsCoopSave([NotNull] SaveInfo saveInfo)
	{
		return saveInfo.Type == SaveInfo.SaveType.Coop;
	}

	public static bool IsImportSave(SaveInfo saveInfo)
	{
		return saveInfo.Type == SaveInfo.SaveType.ForImport;
	}
}
