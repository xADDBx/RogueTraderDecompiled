using System;
using System.IO;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Visual.FogOfWar;

namespace Kingmaker.EntitySystem.Persistence;

internal class AreaDataStash
{
	private static readonly AreaDataStashDirectoryManager Manager;

	public static GameHistoryFile GameHistoryFile => Manager.GameHistoryFile;

	public static string Folder => Manager.Folder;

	private static JsonSerializer Serializer => SaveSystemJsonSerializer.Serializer;

	public static Formatting Formatting => Formatting.None;

	static AreaDataStash()
	{
		Manager = new AreaDataStashDirectoryManager();
		Manager.Init();
	}

	public static AreaDataStashFileAccessor AccessFile(string filename)
	{
		return Manager.AccessFile(filename);
	}

	public static bool Exists(string filename)
	{
		return Manager.Exists(filename);
	}

	public static void ClearDirectory()
	{
		Manager.ClearDirectory();
	}

	public static void CloseAndDelete()
	{
		Manager.CloseAndDelete();
	}

	public static void PrepareFirstLaunch()
	{
	}

	public static void StashAreaState(AreaPersistentState state, bool dispose)
	{
		if (state.MainState.SkipSerialize && dispose)
		{
			state.Dispose();
			foreach (SceneEntitiesState allSceneState in state.GetAllSceneStates())
			{
				ClearJsonForArea(state.Blueprint.AssetGuidThreadSafe, (allSceneState == state.MainState) ? "" : allSceneState.SceneName);
			}
			SavedFogMasks.Get(state.AreaGuid).Wipe();
			return;
		}
		state.ShouldLoad = true;
		using (JsonTextWriter jsonWriter = GetJsonWriterForArea(state.Blueprint.AssetGuidThreadSafe, ""))
		{
			Serializer.Serialize(jsonWriter, state);
		}
		foreach (SceneEntitiesState additionalSceneState in state.GetAdditionalSceneStates())
		{
			if (!additionalSceneState.IsSceneLoadedThreadSafe)
			{
				continue;
			}
			if (additionalSceneState.SkipSerialize && dispose)
			{
				ClearJsonForArea(state.Blueprint.AssetGuidThreadSafe, additionalSceneState.SceneName);
				continue;
			}
			using JsonTextWriter jsonWriter2 = GetJsonWriterForArea(state.Blueprint.AssetGuidThreadSafe, additionalSceneState.SceneName);
			Serializer.Serialize(jsonWriter2, additionalSceneState);
		}
		if (dispose)
		{
			state.Dispose();
		}
	}

	public static AreaPersistentState UnstashAreaState(AreaPersistentState area)
	{
		try
		{
			AreaPersistentState areaPersistentState;
			using (JsonTextReader reader = GetJsonStreamForArea(area, area.MainState))
			{
				areaPersistentState = Serializer.Deserialize<AreaPersistentState>(reader);
			}
			foreach (SceneEntitiesState additionalSceneState in area.GetAdditionalSceneStates())
			{
				if (!additionalSceneState.IsSceneLoaded)
				{
					areaPersistentState.GetStateForScene(additionalSceneState.SceneName);
					continue;
				}
				using JsonTextReader jsonTextReader = GetJsonStreamForArea(area, additionalSceneState);
				if (jsonTextReader == null)
				{
					continue;
				}
				using (ProfileScope.New("Deserialize Side State: " + additionalSceneState.SceneName))
				{
					try
					{
						SceneEntitiesState deserializedSceneState = Serializer.Deserialize<SceneEntitiesState>(jsonTextReader);
						areaPersistentState.SetDeserializedSceneState(deserializedSceneState);
					}
					catch (IOException ex)
					{
						LogChannel.System.Exception(ex, "Exception occured while loading area state: {0} {1}", area.Blueprint.AssetGuidThreadSafe, additionalSceneState.SceneName);
						return null;
					}
				}
			}
			areaPersistentState.ShouldLoad = false;
			EntityService.Instance.GetProxy(areaPersistentState.Area.UniqueId).Entity?.Dispose();
			return areaPersistentState;
		}
		catch (Exception ex2)
		{
			LogChannel.System.Exception(ex2, "Exception unstash area state: {0}", area.Blueprint.AssetGuidThreadSafe);
			return area;
		}
		finally
		{
		}
	}

	public static JsonTextReader GetJsonStreamForArea(AreaPersistentState area, SceneEntitiesState state)
	{
		string path = Path(area.AreaGuid, (state == area.MainState) ? "" : state.SceneName);
		if (!File.Exists(path))
		{
			LogChannel.System.Log("No json state for " + area.Blueprint.AssetGuidThreadSafe + " " + state.SceneName);
			return null;
		}
		try
		{
			return new JsonTextReader(new StreamReader(path));
		}
		catch (IOException ex)
		{
			LogChannel.System.Exception(ex, "Exception occured while loading area state: {0} {1}", area.Blueprint.AssetGuidThreadSafe, state.SceneName);
			return null;
		}
	}

	public static JsonTextWriter GetJsonWriterForArea(string areaId, string sceneName)
	{
		if (!Directory.Exists(Folder))
		{
			Directory.CreateDirectory(Folder);
		}
		return new JsonTextWriter(new StreamWriter(new FileStream(Path(areaId, sceneName), FileMode.Create, FileAccess.Write, FileShare.Read, ISaver.BuffersSize, FileOptions.SequentialScan), ISaver.UTF8NoBom, ISaver.BuffersSize));
	}

	public static string FileName(string areaId, string sceneName)
	{
		return FileTitle(areaId, sceneName) + ".json";
	}

	private static string FileTitle(string areaId, string sceneName)
	{
		return Encode(areaId + sceneName);
	}

	public static string Encode(string title)
	{
		return title;
	}

	public static void SaveJsonForArea(string areaId, string sceneName, string json)
	{
		if (!Directory.Exists(Folder))
		{
			Directory.CreateDirectory(Folder);
		}
		if (json == null)
		{
			ClearJsonForArea(areaId, sceneName);
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(new FileStream(Path(areaId, sceneName), FileMode.Create, FileAccess.Write, FileShare.Read, ISaver.BuffersSize, FileOptions.SequentialScan), ISaver.UTF8NoBom, ISaver.BuffersSize);
		streamWriter.Write(json);
	}

	public static string Path(string areaId, string sceneName)
	{
		return System.IO.Path.Combine(Folder, FileName(areaId, sceneName));
	}

	public static bool HasData(string areaId, string sceneName)
	{
		return File.Exists(Path(areaId, sceneName));
	}

	public static void ClearJsonForArea(string areaId, string sceneName)
	{
		string path = Path(areaId, sceneName);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static async Task EncodeActiveAreaFog(AreaPersistentState state)
	{
		FogOfWarArea active = FogOfWarArea.Active;
		if ((bool)active)
		{
			string sceneName = active.gameObject.scene.name;
			byte[] data = await active.RequestData();
			await SavedFogMasks.Get(state.AreaGuid).Save(sceneName, data);
		}
	}

	public static void StashAreaSubState(AreaPersistentState area, SceneEntitiesState state)
	{
		using JsonTextWriter jsonWriter = GetJsonWriterForArea(area.Blueprint.AssetGuidThreadSafe, state.SceneName);
		Serializer.Serialize(jsonWriter, state);
	}

	public static SceneEntitiesState UnstashAreaSubState(AreaPersistentState areaState, SceneEntitiesState subState)
	{
		if (!subState.IsSceneLoaded)
		{
			return subState;
		}
		using (JsonTextReader jsonTextReader = GetJsonStreamForArea(areaState, subState))
		{
			if (jsonTextReader != null)
			{
				SceneEntitiesState sceneEntitiesState = Serializer.Deserialize<SceneEntitiesState>(jsonTextReader);
				areaState.SetDeserializedSceneState(sceneEntitiesState);
				sceneEntitiesState.PostLoad();
				return sceneEntitiesState;
			}
		}
		areaState.SetDeserializedSceneState(subState = new SceneEntitiesState(subState.SceneName));
		return subState;
	}
}
