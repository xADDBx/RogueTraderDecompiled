using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
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
			{
				foreach (SceneEntitiesState allSceneState in state.GetAllSceneStates())
				{
					ClearJsonForArea(state.Blueprint.AssetGuidThreadSafe, (allSceneState == state.MainState) ? "" : allSceneState.SceneName);
				}
				return;
			}
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
		SaveFogBytesForArea(state, state.SavedFogOfWarMasks);
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
			areaPersistentState.SavedFogOfWarMasks.Clear();
			UnstashFogBytesForArea(area, areaPersistentState.SavedFogOfWarMasks);
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
		string path = Path(area, state);
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

	private static void UnstashFogBytesForArea(AreaPersistentState area, SavedFogMasks stateSavedFogOfWarMasks)
	{
		string assetGuidThreadSafe = area.Blueprint.AssetGuidThreadSafe;
		try
		{
			string[] files = Directory.GetFiles(Folder);
			foreach (string text in files)
			{
				string fileName = System.IO.Path.GetFileName(text);
				if (!fileName.StartsWith(assetGuidThreadSafe) || !fileName.EndsWith(".fog"))
				{
					continue;
				}
				string text2 = fileName.Split('.').Get(1);
				if (!string.IsNullOrEmpty(text2))
				{
					try
					{
						stateSavedFogOfWarMasks.Add(text2, text);
					}
					catch (Exception ex)
					{
						LogChannel.Default.Exception(ex);
						LogChannel.Default.Error("No fog state for " + assetGuidThreadSafe + " (" + text2 + ")");
					}
				}
			}
		}
		catch (Exception ex2)
		{
			LogChannel.Default.Exception(ex2);
			LogChannel.Default.Error("No fog state for " + assetGuidThreadSafe);
		}
	}

	public static JsonTextWriter GetJsonWriterForArea(string areaId, string sceneName)
	{
		if (!Directory.Exists(Folder))
		{
			Directory.CreateDirectory(Folder);
		}
		return new JsonTextWriter(new StreamWriter(Path(areaId, sceneName)));
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
		using StreamWriter streamWriter = new StreamWriter(Path(areaId, sceneName));
		streamWriter.Write(json);
	}

	public static void SaveFogBytesForArea(AreaPersistentState area, SavedFogMasks fowMasks)
	{
		if (!Directory.Exists(Folder))
		{
			Directory.CreateDirectory(Folder);
		}
		string assetGuidThreadSafe = area.Blueprint.AssetGuidThreadSafe;
		fowMasks.SaveAll(Folder, assetGuidThreadSafe);
	}

	public static string FileName(string areaId, string sceneName)
	{
		return areaId + sceneName + ".json";
	}

	public static string Path(string areaId, string sceneName)
	{
		return System.IO.Path.Combine(Folder, FileName(areaId, sceneName));
	}

	public static IEnumerable<string> EnumerateFogMasks(string areaId)
	{
		return Directory.EnumerateFiles(Folder, areaId + ".*.fog");
	}

	private static string Path(AreaPersistentState area, SceneEntitiesState state)
	{
		return System.IO.Path.Combine(Folder, FileName(area.Blueprint.AssetGuidThreadSafe, (state == area.MainState) ? "" : state.SceneName));
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
		if (!(sceneName == ""))
		{
			return;
		}
		foreach (string item in EnumerateFogMasks(areaId))
		{
			if (File.Exists(item))
			{
				File.Delete(item);
			}
		}
	}

	public static async Task EncodeActiveAreaFog(AreaPersistentState state)
	{
		FogOfWarArea active = FogOfWarArea.Active;
		if ((bool)active)
		{
			string sceneName = active.gameObject.scene.name;
			byte[] data = await active.RequestData();
			state.SavedFogOfWarMasks.Add(sceneName, data);
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
