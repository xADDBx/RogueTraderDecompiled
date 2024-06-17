using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Controllers.Net;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.Networking.Serialization;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Replay;

public static class Replay
{
	private enum State
	{
		None,
		WaitingForPlaying,
		Recording,
		Playing
	}

	[JsonObject]
	public struct GameCommandRecord
	{
		[JsonProperty]
		public int StepIndex;

		[JsonProperty]
		public GameCommand[] Commands;

		public GameCommandRecord(int stepIndex)
		{
			StepIndex = stepIndex;
			Commands = Array.Empty<GameCommand>();
		}

		public GameCommandRecord(int stepIndex, List<GameCommand> commands)
		{
			int num = 0;
			int i = 0;
			for (int count = commands.Count; i < count; i++)
			{
				if (commands[i].IsSynchronized)
				{
					num++;
				}
			}
			StepIndex = stepIndex;
			Commands = new GameCommand[num];
			int j = 0;
			int num2 = 0;
			for (int count2 = commands.Count; j < count2; j++)
			{
				if (commands[j].IsSynchronized)
				{
					Commands[num2] = commands[j];
					num2++;
				}
			}
		}

		public void AfterDeserialization()
		{
			if (Commands != null)
			{
				GameCommand[] commands = Commands;
				for (int i = 0; i < commands.Length; i++)
				{
					commands[i].AfterDeserialization();
				}
			}
		}
	}

	[JsonObject]
	public struct UnitCommandRecord
	{
		[JsonProperty]
		public int StepIndex;

		[JsonProperty]
		public UnitCommandParams[] Commands;

		public UnitCommandRecord(int stepIndex)
		{
			StepIndex = stepIndex;
			Commands = Array.Empty<UnitCommandParams>();
		}

		public UnitCommandRecord(int stepIndex, List<UnitCommandParams> commandWrappers)
		{
			StepIndex = stepIndex;
			Commands = commandWrappers.ToArray();
		}

		public void AfterDeserialization()
		{
			if (Commands != null)
			{
				UnitCommandParams[] commands = Commands;
				for (int i = 0; i < commands.Length; i++)
				{
					commands[i].AfterDeserialization();
				}
			}
		}
	}

	[JsonObject]
	public struct SynchronizedDataRecord
	{
		[JsonProperty]
		public int StepIndex;

		[JsonProperty]
		public SynchronizedData[] Commands;

		public SynchronizedDataRecord(int stepIndex)
		{
			StepIndex = stepIndex;
			Commands = Array.Empty<SynchronizedData>();
		}

		public SynchronizedDataRecord(int stepIndex, SynchronizedData[] commandWrappers)
		{
			StepIndex = stepIndex;
			Commands = commandWrappers;
		}
	}

	[JsonObject]
	public struct Record
	{
		[JsonProperty]
		public string ReplayName;

		[JsonProperty]
		public string SaveName;

		[JsonProperty]
		public List<UnitCommandRecord> UnitCommands;

		[JsonProperty]
		public List<GameCommandRecord> GameCommands;

		[JsonProperty]
		public List<SynchronizedDataRecord> SynchronizedData;

		public Record(string replayName, string saveName)
		{
			ReplayName = replayName;
			SaveName = saveName;
			UnitCommands = new List<UnitCommandRecord>();
			GameCommands = new List<GameCommandRecord>();
			SynchronizedData = new List<SynchronizedDataRecord>();
		}

		public void AfterDeserialization()
		{
			if (UnitCommands != null)
			{
				foreach (UnitCommandRecord unitCommand in UnitCommands)
				{
					unitCommand.AfterDeserialization();
				}
			}
			if (GameCommands == null)
			{
				return;
			}
			foreach (GameCommandRecord gameCommand in GameCommands)
			{
				gameCommand.AfterDeserialization();
			}
		}
	}

	public static class NeedToSaveState
	{
		private const string NeedToSaveStateKey = "Replay.NeedToSaveState";

		private const string MenuItem = "Tools/Network/Save State";

		public static bool Value { get; private set; }

		public static void Refresh()
		{
			Value = BuildModeUtility.ReplaySaveState;
		}

		[Cheat(Name = "replay_log_on")]
		public static void TurnOn()
		{
			SetValue(value: true);
		}

		[Cheat(Name = "replay_log_off")]
		public static void TurnOff()
		{
			SetValue(value: false);
		}

		private static void SetValue(bool value)
		{
			Value = value;
			PlayerPrefs.SetInt("Replay.NeedToSaveState", Value ? 1 : 0);
			PFLog.Replay.Log("State Logging on Replay is " + (value ? "enabled" : "disabled"));
		}
	}

	private static Record s_Record;

	private static string s_StateName;

	private static int s_StateSkipFrames;

	private static State s_State;

	private static bool s_UnitCommandsCompleted;

	private static bool s_GameCommandsCompleted;

	private static bool s_SynchronizedDataCompleted;

	private static int s_LastGameCommandStepIndex;

	private static bool s_ShowPopupOnEnd;

	private static bool s_IsFirstTick;

	private static JsonSerializer JsonSerializer => NetSystemJsonSerializer.Serializer;

	public static bool IsActive
	{
		get
		{
			if (!IsRecording)
			{
				return IsPlaying;
			}
			return true;
		}
	}

	public static bool IsRecording => s_State == State.Recording;

	public static bool IsPlaying => s_State == State.Playing;

	public static string Folder => ApplicationPaths.persistentDataPath + "/Replays";

	static Replay()
	{
		s_StateSkipFrames = 3;
		NeedToSaveState.Refresh();
	}

	private static void ResetState(bool success = true)
	{
		s_Record = default(Record);
		s_StateName = string.Empty;
		s_State = State.None;
		s_UnitCommandsCompleted = false;
		s_GameCommandsCompleted = false;
		s_SynchronizedDataCompleted = false;
		s_LastGameCommandStepIndex = 0;
		if (s_ShowPopupOnEnd)
		{
			s_ShowPopupOnEnd = false;
		}
	}

	[Cheat(Name = "replay_skip")]
	public static void SetStateSkipFrames(int n)
	{
		s_StateSkipFrames = n;
		PFLog.Replay.Log($"Replay.StateSkipFrames={s_StateSkipFrames}");
	}

	[Cheat(Name = "replay_skip_get")]
	public static void GetStateSkipFrames()
	{
		PFLog.Replay.Log($"Replay.StateSkipFrames={s_StateSkipFrames}");
	}

	[Cheat(Name = "replay_create_start", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string CreateReplayStart([NotNull] string replayName, string saveName = null)
	{
		replayName = MakeUniqueName(replayName);
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(CreateReplayStartCoroutine());
		return replayName;
		IEnumerator CreateReplayStartCoroutine()
		{
			if (saveName == null)
			{
				saveName = GetSaveName(replayName);
				SaveInfo result = Game.Instance.SaveManager.CreateNewSave(saveName);
				saveName = result.Name;
				Game.Instance.SaveGame(result);
				while (LoadingProcess.Instance.IsLoadingInProcess)
				{
					yield return null;
				}
				if (!Game.Instance.SaveManager.TryFind((SaveInfo e) => e.Name == saveName, out result))
				{
					PFLog.Replay.Error("replay_play: SaveInfo='" + saveName + "' was not found");
					yield break;
				}
				Game.Instance.LoadGame(result);
			}
			else
			{
				while (!LoadingProcess.Instance.IsLoadingInProcess)
				{
					yield return null;
				}
			}
			s_Record = new Record(replayName, saveName);
			s_StateName = GetStateName(replayName);
			s_State = State.Recording;
			s_IsFirstTick = true;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			PFLog.Replay.Log("replay_create: SaveInfo='" + saveName + "' created");
		}
		static string MakeUniqueName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = "R" + DateTime.Now.ToFileTime().ToString("X");
			}
			string replayPath = GetReplayPath(name);
			while (File.Exists(replayPath))
			{
				string[] array = name.Split('_');
				if (1 < array.Length)
				{
					if (int.TryParse(array[^1], out var result2))
					{
						array[^1] = (result2 + 1).ToString();
						name = string.Join('_', array);
						replayPath = GetReplayPath(name);
					}
					else if (string.IsNullOrEmpty(array[^1]))
					{
						name += "1";
						replayPath = GetReplayPath(name);
					}
					else
					{
						name = $"{name}{'_'}1";
						replayPath = GetReplayPath(name);
					}
				}
				else
				{
					name = $"{name}{'_'}1";
					replayPath = GetReplayPath(name);
				}
			}
			return name;
		}
	}

	[Cheat(Name = "replay_create_cancel", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CreateReplayCancel()
	{
		if (s_State == State.Recording)
		{
			RemoveReplay(s_Record.ReplayName);
			ResetState(success: false);
		}
	}

	[Cheat(Name = "replay_create_stop", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CreateReplayStop()
	{
		if (s_State == State.Recording)
		{
			int stepIndex = Game.Instance.RealTimeController.CurrentNetworkTick + 1;
			UnitCommandRecord item = new UnitCommandRecord(stepIndex);
			s_Record.UnitCommands.Add(item);
			PFLog.Replay.Log("replay_create: saving...");
			string replayPath = GetReplayPath(s_Record.ReplayName);
			JsonSerializer.SerializeToFile(replayPath, s_Record);
			ResetState();
			PFLog.Replay.Log("replay_create: replay saving completed!");
		}
	}

	[Cheat(Name = "replay_play", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void PlayReplay([NotNull] string replayName, Action callbackAfterStart = null, bool popupOnEnd = false)
	{
		if (!PrepareToPlay(replayName, out var saveInfo, popupOnEnd))
		{
			ResetState(success: false);
			return;
		}
		Game.Instance.LoadGame(saveInfo);
		WaitForLoadingAndPlay(callbackAfterStart);
	}

	public static bool PrepareToPlay([NotNull] string replayName, out SaveInfo saveInfo, bool popupOnEnd = false)
	{
		s_ShowPopupOnEnd = popupOnEnd;
		if (string.IsNullOrEmpty(replayName))
		{
			PFLog.Replay.Error("replay_play: replay name is null or empty!");
			saveInfo = null;
			return false;
		}
		string source = File.ReadAllText(GetReplayPath(replayName));
		Record record = JsonSerializer.DeserializeObject<Record>(source);
		record.AfterDeserialization();
		string saveName = record.SaveName;
		if (!Game.Instance.SaveManager.TryFind((SaveInfo e) => e.Name == saveName, out saveInfo))
		{
			PFLog.Replay.Error("replay_play: SaveInfo='" + saveName + "' was not found");
			return false;
		}
		s_Record = record;
		s_StateName = GetStateName(replayName);
		s_State = State.WaitingForPlaying;
		s_IsFirstTick = true;
		return true;
	}

	public static void WaitForLoadingAndPlay(Action callbackAfterStart = null)
	{
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(WaitForLoadingAndPlayCoroutine());
		IEnumerator WaitForLoadingAndPlayCoroutine()
		{
			while (!LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			s_State = State.Playing;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			PFLog.Replay.Log("replay_play: replay='" + s_Record.SaveName + "' started to play");
			if (callbackAfterStart != null)
			{
				yield return new WaitForSeconds(9f);
				callbackAfterStart();
			}
		}
	}

	[Cheat(Name = "replay_cancel", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CancelReplay()
	{
		ResetState(success: false);
	}

	public static void SetSaveData([NotNull] string stateName)
	{
		if (s_State != State.WaitingForPlaying)
		{
			s_StateName = GetStateName(stateName);
			s_IsFirstTick = true;
		}
	}

	public static void SaveUnitCommandsInProcess(List<UnitCommandParams> unitCommands)
	{
		if (s_State == State.Recording && unitCommands.Count != 0)
		{
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			UnitCommandRecord item = CopyBySerialization(new UnitCommandRecord(currentNetworkTick, unitCommands));
			s_Record.UnitCommands.Add(item);
		}
	}

	public static void LoadUnitCommandsInProcess(List<UnitCommandParams> unitCommands)
	{
		if (s_State != State.Playing)
		{
			return;
		}
		unitCommands.Clear();
		if (s_Record.UnitCommands.Count == 0)
		{
			TryComplete(unitCommandsCompleted: true);
			return;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		int i = 0;
		for (int count = s_Record.UnitCommands.Count; i < count; i++)
		{
			UnitCommandRecord unitCommandRecord = s_Record.UnitCommands[i];
			if (unitCommandRecord.StepIndex == currentNetworkTick)
			{
				unitCommands.AddRange(unitCommandRecord.Commands);
				if (i == count - 1)
				{
					TryComplete(unitCommandsCompleted: true);
				}
				return;
			}
		}
		List<UnitCommandRecord> unitCommands2 = s_Record.UnitCommands;
		if (unitCommands2[unitCommands2.Count - 1].StepIndex < currentNetworkTick)
		{
			TryComplete(unitCommandsCompleted: true);
		}
	}

	public static void SaveGameCommands(List<(NetPlayer, GameCommand)> gameCommands)
	{
		if (s_State != State.Recording)
		{
			return;
		}
		int num = gameCommands.Count;
		if (num == 0)
		{
			return;
		}
		foreach (var gameCommand in gameCommands)
		{
			if (!gameCommand.Item2.IsSynchronized)
			{
				num--;
			}
		}
		if (num != 0)
		{
			List<GameCommand> commands = gameCommands.Select(((NetPlayer, GameCommand) elem) => elem.Item2).ToTempList();
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			GameCommandRecord item = CopyBySerialization(new GameCommandRecord(currentNetworkTick, commands));
			s_Record.GameCommands.Add(item);
		}
	}

	public static void LoadGameCommands(List<(NetPlayer, GameCommand)> gameCommands)
	{
		if (s_State != State.Playing)
		{
			return;
		}
		if (s_Record.GameCommands.Count == 0)
		{
			TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: true);
			return;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		if (s_LastGameCommandStepIndex == currentNetworkTick)
		{
			return;
		}
		s_LastGameCommandStepIndex = currentNetworkTick;
		int i = 0;
		for (int count = s_Record.GameCommands.Count; i < count; i++)
		{
			GameCommandRecord gameCommandRecord = s_Record.GameCommands[i];
			if (gameCommandRecord.StepIndex == currentNetworkTick)
			{
				GameCommand[] commands = gameCommandRecord.Commands;
				foreach (GameCommand item in commands)
				{
					gameCommands.Add((NetPlayer.Empty, item));
				}
				if (i == count - 1)
				{
					TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: true);
				}
				return;
			}
		}
		List<GameCommandRecord> gameCommands2 = s_Record.GameCommands;
		if (gameCommands2[gameCommands2.Count - 1].StepIndex < currentNetworkTick)
		{
			TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: true);
		}
	}

	public static void SaveSynchronizedData(PlayerCommandsCollection<SynchronizedData> synchronizedData)
	{
		if (s_State == State.Recording)
		{
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			SynchronizedDataRecord item = CopyBySerialization(new SynchronizedDataRecord(currentNetworkTick, synchronizedData[NetPlayer.Offline].Commands.ToArray()));
			s_Record.SynchronizedData.Add(item);
		}
	}

	public static void LoadSynchronizedData(PlayerCommandsCollection<SynchronizedData> synchronizedData)
	{
		if (s_State != State.Playing)
		{
			return;
		}
		if (s_Record.SynchronizedData.Count == 0)
		{
			TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: false, synchronizedDataCompleted: true);
			return;
		}
		synchronizedData.Clear();
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		int i = 0;
		for (int count = s_Record.SynchronizedData.Count; i < count; i++)
		{
			SynchronizedDataRecord synchronizedDataRecord = s_Record.SynchronizedData[i];
			if (synchronizedDataRecord.StepIndex == currentNetworkTick)
			{
				synchronizedData[NetPlayer.Offline].AddRange(synchronizedDataRecord.Commands);
				if (i == count - 1)
				{
					TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: false, synchronizedDataCompleted: true);
				}
				return;
			}
		}
		List<SynchronizedDataRecord> synchronizedData2 = s_Record.SynchronizedData;
		if (synchronizedData2[synchronizedData2.Count - 1].StepIndex < currentNetworkTick)
		{
			TryComplete(unitCommandsCompleted: false, gameCommandsCompleted: false, synchronizedDataCompleted: true);
		}
	}

	public static void RemoveReplay(string replayName)
	{
		string replayPath = GetReplayPath(replayName);
		string saveName = GetSaveName(replayName, replayPath);
		if (File.Exists(replayPath))
		{
			File.Delete(replayPath);
		}
		if (Game.HasInstance && Game.Instance.SaveManager != null && Game.Instance.SaveManager.TryFind((SaveInfo e) => e.Name == saveName, out var result))
		{
			Game.Instance.SaveManager.DeleteSave(result);
		}
		else
		{
			string path = ApplicationPaths.persistentDataPath + "/Saved Games/";
			if (Directory.Exists(path))
			{
				List<string> list = new List<string>(Directory.GetFiles(path));
				list.RemoveAll((string d) => !d.Contains("/Manual_") || !d.Contains("_" + saveName + ".zks"));
				list.ForEach(File.Delete);
			}
		}
		RemoveRecords(replayName);
	}

	public static string GetSaveName(string replayName, string replayFilePath = null, bool path = false)
	{
		if (replayFilePath == null)
		{
			replayFilePath = GetReplayPath(replayName);
		}
		string saveName2;
		if (File.Exists(replayFilePath))
		{
			string source = File.ReadAllText(replayFilePath);
			saveName2 = JsonSerializer.DeserializeObject<Record>(source).SaveName;
		}
		else
		{
			saveName2 = GetSaveName(replayName);
		}
		if (!path)
		{
			return saveName2;
		}
		return GetPath(saveName2);
		static string GetPath(string saveName)
		{
			string path2 = ApplicationPaths.persistentDataPath + "/Saved Games/";
			if (!Directory.Exists(path2))
			{
				return null;
			}
			List<string> list = new List<string>(Directory.GetFiles(path2));
			list.RemoveAll((string d) => !d.Contains("/Manual_") || !d.Contains("_" + saveName + ".zks"));
			if (!list.TryGet(0, out var element))
			{
				return null;
			}
			return element;
		}
	}

	public static void RemoveRecords(string replayName)
	{
		string path = ApplicationPaths.persistentDataPath + "/Replays/";
		if (Directory.Exists(path))
		{
			List<string> list = new List<string>(Directory.GetDirectories(path));
			list.RemoveAll((string d) => !d.Contains("/" + replayName + ".replay."));
			list.ForEach(delegate(string s)
			{
				Directory.Delete(s, recursive: true);
			});
		}
	}

	public static void RemoveRecord(string replayName, string recordName)
	{
		List<string> list = new List<string>(Directory.GetDirectories(ApplicationPaths.persistentDataPath + "/Replays/"));
		list.RemoveAll((string d) => !d.Contains("/" + replayName + ".replay." + recordName));
		list.ForEach(delegate(string s)
		{
			Directory.Delete(s, recursive: true);
		});
	}

	private static void TryComplete(bool unitCommandsCompleted = false, bool gameCommandsCompleted = false, bool synchronizedDataCompleted = false)
	{
		s_UnitCommandsCompleted |= unitCommandsCompleted;
		s_GameCommandsCompleted |= gameCommandsCompleted;
		s_SynchronizedDataCompleted |= synchronizedDataCompleted;
		if (s_UnitCommandsCompleted && s_GameCommandsCompleted && s_SynchronizedDataCompleted)
		{
			PFLog.Replay.Log("replay_play: replay is finished");
			ResetState();
		}
	}

	public static void SaveState()
	{
		if ((s_State == State.Recording || s_State == State.Playing || NetworkingManager.IsActive) && NeedToSaveState.Value)
		{
			SaveState(s_IsFirstTick);
			s_IsFirstTick = false;
		}
	}

	private static void SaveState(bool force)
	{
		if (force || Game.Instance.RealTimeController.IsNetworkTick)
		{
			int num = Game.Instance.RealTimeController.CurrentNetworkTick;
			if (s_IsFirstTick && Game.Instance.RealTimeController.IsNetworkTick)
			{
				num--;
			}
			if (force || num % (s_StateSkipFrames + 1) == 0)
			{
				StateSerializationController.SaveState(s_StateName, num);
			}
		}
	}

	private static string GetSaveName([NotNull] string replayName)
	{
		return "replay_" + replayName;
	}

	public static string GetReplayPath([NotNull] string replayName)
	{
		string folder = Folder;
		Directory.CreateDirectory(folder);
		return folder + "/" + GetReplayFileName(replayName);
	}

	public static string GetReplayFileName([NotNull] string replayName)
	{
		return replayName + ".replay.json";
	}

	private static string GetStateName([NotNull] string replayName)
	{
		string folder = Folder;
		Directory.CreateDirectory(folder);
		return Path.Combine(folder, $"{replayName}.replay.{DateTime.Now.ToFileTime()}");
	}

	private static T CopyBySerialization<T>([NotNull] T obj)
	{
		string source = JsonSerializer.SerializeObject(obj);
		return JsonSerializer.DeserializeObject<T>(source);
	}

	public static IEnumerator PlayReplayAndWait(string replayName, int maxTickIndex = 0, Action callbackAfterStart = null, bool popupOnEnd = false)
	{
		PFLog.Replay.Log("Init playback...");
		PlayReplay(replayName, callbackAfterStart, popupOnEnd);
		PFLog.Replay.Log("Waiting for start...");
		while (s_State != State.Playing)
		{
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		while (LoadingProcess.Instance.IsLoadingInProcess)
		{
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		PFLog.Replay.Log("Waiting for playback completion...");
		while (s_State == State.Playing)
		{
			yield return new WaitForSeconds(1f);
			if (0 < maxTickIndex && maxTickIndex <= Game.Instance.RealTimeController.CurrentNetworkTick)
			{
				ResetState();
				break;
			}
		}
		yield return new WaitForSeconds(1f);
	}
}
