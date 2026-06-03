using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Networking.Settings;
using Kingmaker.Settings;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

public static class ThreadedGameLoader
{
	public static async Task<string> Load(SaveInfo saveInfo, bool isSmokeTest)
	{
		await Awaiters.ThreadPoolYield;
		ISaver mainSaver = saveInfo.Saver;
		try
		{
			saveInfo.Saver = null;
			List<string> allFiles = mainSaver.GetAllFiles();
			ConcurrentQueue<string> allFiles2 = new ConcurrentQueue<string>(allFiles);
			mainSaver.Save();
			Task[] array = new Task[Math.Min(allFiles.Count / 5 + 1, 6)];
			using ISaver mainThreadLoader = mainSaver.Clone();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ReadFiles(saveInfo, mainSaver, allFiles2, isSmokeTest);
			}
			Task<string> statisticsStateTask = LoadStatisticsAsync(saveInfo, mainSaver, "statistic");
			string text = LoadJson(saveInfo, mainThreadLoader, "fogIndex");
			string text2 = LoadJson(saveInfo, mainThreadLoader, "knownAreas");
			string text3 = LoadJson(saveInfo, mainThreadLoader, "settings");
			string text4 = LoadJson(saveInfo, mainThreadLoader, "player");
			string text5 = LoadJson(saveInfo, mainThreadLoader, "party");
			string json = LoadJson(saveInfo, mainThreadLoader, "coop");
			if (text3 == null)
			{
				throw new Exception("Save is broken: settings.json was not found");
			}
			if (text4 == null)
			{
				throw new Exception("Save is broken: player.json was not found");
			}
			if (text5 == null)
			{
				throw new Exception("Save is broken: party.json was not found");
			}
			Task<KnownAreaEntry[]> deserializeKnownAreaListJson = ((text2 != null) ? TryDeserialize<KnownAreaEntry[]>(text2, "known areas") : Task.FromResult(Array.Empty<KnownAreaEntry>()));
			Task<SavedFogMasks[]> deserializeFogIndexJson = ((text != null) ? Deserialize<SavedFogMasks[]>(text, "fog data") : Task.FromResult(Array.Empty<SavedFogMasks>()));
			Task<InGameSettings> deserializeSettingsTask = DeserializeInGameSettings(text3);
			Task<Player> deserializePlayerTask = DeserializePlayer(text4);
			Task<SceneEntitiesState> deserializeCrossSceneStateTask = DeserializeCrossSceneState(text5);
			Task<CoopData> deserializeCoopTask = DeserializeCoopSettings(json);
			await Task.WhenAll(array);
			string statistics = await statisticsStateTask;
			KnownAreaEntry[] array2 = await deserializeKnownAreaListJson;
			if (array2 != null && array2.Length != 0)
			{
				CreateKnownAreasState(array2);
			}
			else
			{
				CreateStateData(allFiles);
			}
			SavedFogMasks[] array3 = await deserializeFogIndexJson;
			if (array3.Length != 0)
			{
				SavedFogMasks.AllMasks = array3;
			}
			else
			{
				foreach (string item in allFiles)
				{
					if (item.EndsWith(".fog") && item.Length > 32 + ".fog".Length)
					{
						string areaId = item.Substring(0, 32);
						string text6 = item;
						string sceneName = text6.Substring(32, text6.Length - 4 - 32);
						SavedFogMasks.Get(areaId).Register(sceneName);
					}
				}
			}
			InGameSettings settings = await deserializeSettingsTask;
			SceneEntitiesState crossSceneState = await deserializeCrossSceneStateTask;
			Player player = await deserializePlayerTask;
			CoopData coopData = await deserializeCoopTask;
			Game.Instance.State.InGameSettings = settings;
			Game.Instance.State.CoopData = coopData;
			if (crossSceneState != null)
			{
				crossSceneState.SceneName = "[cross-area objects]";
				player.CrossSceneState = crossSceneState;
			}
			Game.Instance.State.PlayerState = player;
			return statistics;
		}
		finally
		{
			saveInfo.Saver = mainSaver;
		}
		static async Task<string> LoadStatisticsAsync(SaveInfo saveInfo, ISaver mainSaver, string fileName)
		{
			using ISaver saver = mainSaver.Clone();
			await Awaiters.ThreadPoolYield;
			return LoadJson(saveInfo, saver, fileName);
		}
	}

	private static Task<T> Deserialize<T>([NotNull] string json, string displayName) where T : class
	{
		return Deserialize<T>("Restore " + displayName, json, SaveSystemJsonSerializer.Serializer);
	}

	private static Task<T> TryDeserialize<T>([NotNull] string json, string displayName) where T : class
	{
		return TryDeserialize<T>("Try restore " + displayName, json, SaveSystemJsonSerializer.Serializer);
	}

	[NotNull]
	private static Task<Player> DeserializePlayer([NotNull] string json)
	{
		return Deserialize<Player>("Restore player state", json, SaveSystemJsonSerializer.Serializer);
	}

	private static Task<SceneEntitiesState> DeserializeCrossSceneState(string json)
	{
		return Deserialize<SceneEntitiesState>("Restore party state", json, SaveSystemJsonSerializer.Serializer);
	}

	private static Task<InGameSettings> DeserializeInGameSettings(string json)
	{
		return Deserialize<InGameSettings>("Restore settings state", json, SettingsJsonSerializer.Serializer);
	}

	private static async Task<CoopData> DeserializeCoopSettings(string json)
	{
		return (await Deserialize<CoopData>("Restore coop state", json, CoopSettingsSaveDataSerializer.Serializer)) ?? new CoopData();
	}

	private static async Task<T> Deserialize<T>(string timer, string json, JsonSerializer serializer) where T : class
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New(timer))
		{
			return (json == null) ? null : serializer.DeserializeObject<T>(json);
		}
	}

	private static async Task<T> TryDeserialize<T>(string timer, string json, JsonSerializer serializer) where T : class
	{
		await EditorSafeThreading.Awaitable;
		using (CodeTimer.New(timer))
		{
			return (json == null) ? null : serializer.TryDeserializeObject<T>(json);
		}
	}

	private static void CreateKnownAreasState(KnownAreaEntry[] knownAreas)
	{
		for (int i = 0; i < knownAreas.Length; i++)
		{
			KnownAreaEntry knownAreaEntry = knownAreas[i];
			AreaPersistentState state = GetState(knownAreaEntry.AreaGuid);
			state.ShouldLoad = true;
			if (knownAreaEntry.AddStateScenes == null || knownAreaEntry.AddStateScenes.Length == 0)
			{
				continue;
			}
			string[] addStateScenes = knownAreaEntry.AddStateScenes;
			foreach (string sceneName in addStateScenes)
			{
				if (!state.GetAdditionalSceneStates().HasItem((SceneEntitiesState s) => s.SceneName == sceneName))
				{
					state.GetAdditionalSceneStates().Add(new SceneEntitiesState(sceneName));
				}
			}
		}
	}

	private static void CreateStateData(List<string> allFiles)
	{
		foreach (string allFile in allFiles)
		{
			if (!allFile.EndsWith(".json"))
			{
				continue;
			}
			switch (allFile)
			{
			case "header.json":
			case "player.json":
			case "party.json":
			case "settings.json":
			case "statistic.json":
			case "coop.json":
				continue;
			}
			if (allFile.StartsWith("checksums"))
			{
				continue;
			}
			string sceneName = ((allFile.Length > 37) ? allFile.Substring(32, allFile.Length - 37) : null);
			AreaPersistentState areaPersistentState = ((allFile.Length >= 32) ? GetState(allFile.Substring(0, 32)) : null);
			if (areaPersistentState != null)
			{
				areaPersistentState.ShouldLoad = true;
				if (sceneName != null && areaPersistentState.GetAdditionalSceneStates().FirstOrDefault((SceneEntitiesState s) => s.SceneName == sceneName) == null)
				{
					areaPersistentState.GetAdditionalSceneStates().Add(new SceneEntitiesState(sceneName));
				}
			}
		}
	}

	private static async Task ReadFiles(SaveInfo saveInfo, ISaver mainSaver, ConcurrentQueue<string> allFiles, bool isSmokeTest)
	{
		using ISaver saver = mainSaver.Clone();
		await Awaiters.ThreadPoolYield;
		string result;
		while (allFiles.TryDequeue(out result))
		{
			switch (result)
			{
			case "header.json":
			case "player.json":
			case "party.json":
			case "settings.json":
			case "coop.json":
			case "history":
			case "statistic":
				continue;
			}
			if (isSmokeTest && !result.StartsWith(saveInfo.Area.AssetGuidThreadSafe))
			{
				continue;
			}
			if (result.EndsWith(".json"))
			{
				string text = result;
				string text2 = text.Substring(0, text.Length - 5);
				if (JsonUpgradeSystem.ShouldUpgrade(saveInfo, text2))
				{
					string contents = LoadJson(saveInfo, saver, text2);
					using (AreaDataStashFileAccessor accessor = AreaDataStash.AccessFile(result))
					{
						await File.WriteAllTextAsync(accessor.Path, contents);
					}
					continue;
				}
			}
			saver.CopyToStash(result);
		}
	}

	[CanBeNull]
	private static string LoadJson(SaveInfo saveInfo, [NotNull] ISaver saver, [NotNull] string path)
	{
		string text = saver.ReadJson(path);
		if (text == null)
		{
			return null;
		}
		return JsonUpgradeSystem.Upgrade(saveInfo, path, text);
	}

	private static AreaPersistentState GetState(string guid)
	{
		AreaPersistentState areaPersistentState = Game.Instance.State.SavedAreaStates.FirstOrDefault((AreaPersistentState s) => s.AreaGuid == guid);
		if (areaPersistentState == null)
		{
			Game.Instance.State.SavedAreaStates.Add(areaPersistentState = new AreaPersistentState(guid));
		}
		return areaPersistentState;
	}
}
