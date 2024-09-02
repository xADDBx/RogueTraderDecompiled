using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Cheats.Logic;
using Kingmaker.Controllers.Clicks;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.RuleSystem;
using Kingmaker.UI;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.UIKitDependencies;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using Unity.Profiling.Memory;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Diagnostics;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Kingmaker.Cheats;

internal class CheatsDebug
{
	public class CategoryInfo
	{
		public long Count;

		public long Size;
	}

	private enum TextureCategoryByPrefix
	{
		DynamicCharacter,
		BakedCharacter,
		UI_NonAtlas,
		UI_Atlas,
		UI_Font,
		RenderTarget,
		Weapon,
		Other,
		Count
	}

	public struct UITexturesCountForArbiter
	{
		public CategoryInfo UI_Atlases;

		public CategoryInfo UI_NoAtlases;
	}

	private static HashSet<int> s_DeltaHistory;

	[Cheat(Name = "draw_fps", Description = "When false, FPS Counter is disabled")]
	public static bool DrawFps { get; set; } = true;


	[Cheat(Name = "draw_cutscenes", Description = "When false, Cutscenes debug info is disabled")]
	public static bool DrawCutscenes { get; set; } = true;


	[Cheat(Name = "draw_space_combat_debug_decals", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "When false, space combat debug decals are disabled")]
	public static bool DrawSpaceCombatDebugDecals { get; set; }

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("ExecuteFromBuffer", ExecuteFromBuffer);
			keyboard.Bind("ClipboardCopyBuildInfo", ClipboardCopyBuildInfoHotkey);
			keyboard.Bind("OpenGameLogFull", OpenGameLogFull);
			keyboard.Bind("ReloadUI", delegate
			{
				CheatsHelper.Run("reload_ui");
			});
			keyboard.Bind("ChangeUINextPlatform", delegate
			{
				CheatsHelper.Run("change_ui_next_platform");
			});
			keyboard.Bind("ChangeUIPrevPlatform", delegate
			{
				CheatsHelper.Run("change_ui_prev_platform");
			});
			SmartConsole.RegisterCommand("mainmenu", MainMenu);
			SmartConsole.RegisterCommand("heap_dump", HeapDump);
			SmartConsole.RegisterCommand("sleep_every_frame", SleepEveryFrame);
			SmartConsole.RegisterCommand("log_graphics_hardware", LogGraphicsHardware);
			SmartConsole.RegisterCommand("unity_analytics", UnityAnalytics);
			SmartConsole.RegisterCommand("measure_next_roll", MeasureNextRoll);
			SmartConsole.RegisterCommand("entity_dump", DumpState);
			SmartConsole.RegisterCommand("set_fov", SetCameraFov);
			SmartConsole.RegisterCommand("set_fov_min_max", SetCameraFovMinMax);
			SmartConsole.RegisterCommand("unscene", UnloadScene);
			SmartConsole.RegisterCommand("reload", ReloadLastSave);
			SmartConsole.RegisterCommand("check_build_removal", CheckBuildRemoval);
			SmartConsole.RegisterCommand("tex_dump", TexturesDumpBuild);
			SmartConsole.RegisterCommand("tex_dump_csv", TexturesDumpCSVNoResolve);
			SmartConsole.RegisterCommand("mesh_dump_csv", MeshesDumpCSVNoResolve);
			SmartConsole.RegisterCommand("heap", delegate
			{
				PFLog.SmartConsole.Log($"Heap: {GC.GetTotalMemory(forceFullCollection: false)}");
			});
			SmartConsole.RegisterCommand("beta_status", BetaStatus);
			SmartConsole.RegisterCommand("dump_static_colliders", DumpStaticColliders);
			SmartConsole.RegisterCommand("raycast", Raycast);
			SmartConsole.RegisterCommand("debug_pointer_controller", DebugPointerController);
			SmartConsole.RegisterCommand("set_log_severity", SetLogSeverity);
			SmartConsole.RegisterCommand("copy_build_info", ClipboardCopyBuildInfoCheat);
			SmartConsole.RegisterCommand("open_gamelogfull", OpenGameLogFull);
			SmartConsole.RegisterCommand("distance", ShowDistance);
			SmartConsole.RegisterCommand("print_current_dialog", DebugCurrentDialog);
			SmartConsole.RegisterCommand("debug_crash", CrashGame);
			SmartConsole.RegisterCommand("debug_exception", ExceptionGame);
			SmartConsole.RegisterCommand("debug_persistent_path", DebugPersistentPath);
		}
	}

	[Cheat(Name = "alloc_crash", Description = "Allocate memory until Unity crashes")]
	public static void DoRunOutOfMemory()
	{
		List<byte[]> list = new List<byte[]>();
		while (true)
		{
			list.Add(new byte[1048576]);
			PFLog.Default.Log($"Allocated {list.Count} MB, total Unity {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:# ###} MB");
		}
	}

	[Cheat(Name = "gc", Description = "Call GC.Collect()")]
	public static void DoGC()
	{
		GC.Collect();
	}

	[Cheat(Name = "gc_uua", Description = "Call GC.Collect() and UnloadUnusedAssets")]
	public static void DoGCUUA()
	{
		GC.Collect();
		Resources.UnloadUnusedAssets();
	}

	[Cheat(Name = "memstats", Description = "Show allocated memory amounts")]
	public static void MemStats()
	{
		PFLog.SmartConsole.Log("GPU:   " + Mb(Profiler.GetAllocatedMemoryForGraphicsDriver()));
		PFLog.SmartConsole.Log("Unity: " + Mb(Profiler.GetTotalAllocatedMemoryLong()) + " (" + Mb(Profiler.GetTotalReservedMemoryLong()) + ")");
		PFLog.SmartConsole.Log("Mono:  " + Mb(Profiler.GetMonoUsedSizeLong()) + " (" + Mb(Profiler.GetMonoHeapSizeLong()) + ")");
		static string Mb(long bytes)
		{
			return ((float)bytes / 1024f / 1024f).ToString("### ##0.0 mb");
		}
	}

	[Cheat(Name = "quit", Description = "Call SystemUtil.ApplicationQuit()")]
	public static void Quit()
	{
		SystemUtil.ApplicationQuit();
	}

	[Cheat(Name = "quit_force", Description = "Call Application.Quit(1)")]
	public static void QuitForce()
	{
		Application.Quit(1);
	}

	[Cheat(Name = "wwise_profile", Description = "Launch Wwise profiler session")]
	public static void WwiseProfilerCapture()
	{
		AKRESULT aKRESULT = AkSoundEngine.StartProfilerCapture(Path.Combine(ApplicationPaths.DevelopmentDataPath, "WwiseProfilerSession.prof"));
		PFLog.Audio.Log((aKRESULT == AKRESULT.AK_Success) ? "Wwise Profiler Started" : $"Failed to start Wwise Profiler {aKRESULT}");
	}

	private static void DebugPointerController(string parameters)
	{
		PointerController.DebugThisFrame = true;
	}

	private static void Raycast(string parameters)
	{
		Camera camera = Game.GetCamera();
		RaycastHit[] array = Physics.RaycastAll(camera.ScreenPointToRay(Input.mousePosition), camera.farClipPlane, 70014209);
		foreach (RaycastHit raycastHit in array)
		{
			Collider collider = raycastHit.collider;
			GameObject gameObject = collider.gameObject;
			SmartConsole.Print(GetPath(gameObject));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "enabled", collider.enabled));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "activeSelf", gameObject.activeSelf));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "activeInHierarchy", gameObject.activeInHierarchy));
			if (!gameObject.activeInHierarchy)
			{
				Transform parent = gameObject.transform.parent;
				while (parent != null && parent.gameObject.activeSelf)
				{
					parent = parent.transform.parent;
				}
				SmartConsole.Print(string.Format("\t{0,20}: {1}", "notActiveParent", ObjectExtensions.Or(parent, null)?.name));
			}
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "layer", gameObject.layer));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "hideFlags", gameObject.hideFlags));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "position", gameObject.transform.position));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "rotation", gameObject.transform.rotation));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "scale", gameObject.transform.lossyScale));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "bounds", collider.bounds));
			SmartConsole.Print(string.Format("\t{0,20}: {1}", "contactOffset", collider.contactOffset));
			SmartConsole.Print("--------");
		}
	}

	private static void DumpStaticColliders(string parameters)
	{
		if (File.Exists("static_colliders.txt"))
		{
			File.Delete("static_colliders.txt");
		}
		using StreamWriter streamWriter = new StreamWriter(File.OpenWrite("static_colliders.txt"));
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				if (gameObject.name != "Static")
				{
					continue;
				}
				Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					GameObject gameObject2 = collider.gameObject;
					streamWriter.WriteLine(GetPath(gameObject2));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "enabled", collider.enabled));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "activeSelf", gameObject2.activeSelf));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "activeInHierarchy", gameObject2.activeInHierarchy));
					if (!gameObject2.activeInHierarchy)
					{
						Transform parent = gameObject2.transform.parent;
						while (parent != null && parent.gameObject.activeSelf)
						{
							parent = parent.transform.parent;
						}
						streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "notActiveParent", ObjectExtensions.Or(parent, null)?.name));
					}
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "layer", gameObject2.layer));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "hideFlags", gameObject2.hideFlags));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "position", gameObject2.transform.position));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "rotation", gameObject2.transform.rotation));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "scale", gameObject2.transform.lossyScale));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "bounds", collider.bounds));
					streamWriter.WriteLine(string.Format("\t{0,20}: {1}", "contactOffset", collider.contactOffset));
					streamWriter.WriteLine();
				}
			}
		}
	}

	private static string GetPath(GameObject go)
	{
		string text = go.name;
		while (go.transform.parent != null)
		{
			go = go.transform.parent.gameObject;
			text = go.name + "." + text;
		}
		return text;
	}

	private static void ObjectsDeltaDump(string parameters)
	{
		bool flag = Utilities.GetParamBool(parameters, 1, "Update mode?") ?? true;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
		UnityEngine.Object[] array2;
		if (s_DeltaHistory == null)
		{
			PFLog.SmartConsole.Log("Creating initial state");
			s_DeltaHistory = new HashSet<int>();
			array2 = array;
			foreach (UnityEngine.Object @object in array2)
			{
				s_DeltaHistory.Add(@object.GetInstanceID());
			}
			return;
		}
		PFLog.SmartConsole.Log("Objects delta from last time:");
		HashSet<int> hashSet = new HashSet<int>(s_DeltaHistory);
		array2 = array;
		foreach (UnityEngine.Object object2 in array2)
		{
			int instanceID = object2.GetInstanceID();
			if (!s_DeltaHistory.Contains(instanceID))
			{
				PFLog.SmartConsole.Log($"New object: {object2.GetType().Name} {object2.name} ({instanceID})");
				if (flag)
				{
					s_DeltaHistory.Add(instanceID);
				}
			}
			else
			{
				hashSet.Remove(instanceID);
			}
		}
		foreach (int item in hashSet)
		{
			PFLog.SmartConsole.Log($"Removed object: ({item})");
			if (flag)
			{
				s_DeltaHistory.Remove(item);
			}
		}
	}

	private static void DumpState(string parameters)
	{
		foreach (Entity allEntityDatum in Game.Instance.Player.CrossSceneState.AllEntityData)
		{
			PFLog.Default.Log(string.Format("Entity {0} {1} in {2} View {3} ({4})", allEntityDatum.GetType().Name, allEntityDatum.UniqueId, allEntityDatum.HoldingState?.SceneName ?? "NULL", allEntityDatum.View, allEntityDatum.View != null));
		}
		foreach (Entity allEntityDatum2 in Game.Instance.State.LoadedAreaState.AllEntityData)
		{
			PFLog.Default.Log(string.Format("Entity {0} {1} in {2} View {3} ({4})", allEntityDatum2.GetType().Name, allEntityDatum2.UniqueId, allEntityDatum2.HoldingState?.SceneName ?? "NULL", allEntityDatum2.View, allEntityDatum2.View != null));
		}
	}

	private static void MeasureNextRoll(string parameters)
	{
		RulebookEvent.Dice.MeasureNextRoll = true;
	}

	private static void UnityAnalytics(string parameters)
	{
		bool? paramBool = Utilities.GetParamBool(parameters, 1, "Can't parse bool from given parameters");
		if (paramBool.HasValue)
		{
			PerformanceReporting.enabled = paramBool.Value;
		}
	}

	private static void LogGraphicsHardware(string parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"graphicsDeviceType: {SystemInfo.graphicsDeviceType}");
		stringBuilder.AppendLine($"graphicsDeviceID: {SystemInfo.graphicsDeviceID}");
		stringBuilder.AppendLine("graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
		stringBuilder.AppendLine("graphicsDeviceVendor: " + SystemInfo.graphicsDeviceVendor);
		stringBuilder.AppendLine($"graphicsDeviceVendorID: {SystemInfo.graphicsDeviceVendorID}");
		stringBuilder.AppendLine("graphicsDeviceVersion: " + SystemInfo.graphicsDeviceVersion);
		stringBuilder.AppendLine($"graphicsMemorySize: {SystemInfo.graphicsMemorySize}");
		stringBuilder.AppendLine($"graphicsMultiThreaded: {SystemInfo.graphicsMultiThreaded}");
		stringBuilder.AppendLine($"graphicsShaderLevel: {SystemInfo.graphicsShaderLevel}");
		stringBuilder.AppendLine($"graphicsUVStartsAtTop: {SystemInfo.graphicsUVStartsAtTop}");
		PFLog.SmartConsole.Log(stringBuilder.ToString());
	}

	public static void ProduceException(string parameters)
	{
		parameters[-1].ToString().CopyTo(0, new char[2], 2, 1);
	}

	private static void MainMenu(string parameters)
	{
		Game.Instance.ResetToMainMenu();
	}

	public static void ExecuteFromBuffer()
	{
		PFLog.SmartConsole.Log("Executing command: " + GUIUtility.systemCopyBuffer);
		string[] array = GUIUtility.systemCopyBuffer.Replace("\r\n", "\n").Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			SmartConsole.ExecuteLine(array[i]);
		}
	}

	public static void HeapDump(string parameters)
	{
		GameHeapSnapshot.HeapSnapshot();
	}

	public static void BetaStatus(string parameters)
	{
		PFLog.SmartConsole.Log(Utilities.BetaStatus());
	}

	public static void SleepEveryFrame(string parameters)
	{
		int? paramInt = Utilities.GetParamInt(parameters, 1, "Cant parse sleep time from string '" + parameters + "'");
		if (paramInt.HasValue)
		{
			int? paramInt2 = Utilities.GetParamInt(parameters, 2, null);
			FPSFreezer fPSFreezer = UnityEngine.Object.FindObjectOfType<FPSFreezer>(includeInactive: true);
			if (paramInt.Value <= 0)
			{
				fPSFreezer.gameObject.SetActive(value: false);
				return;
			}
			fPSFreezer.gameObject.SetActive(value: true);
			fPSFreezer.MinSleepMs = paramInt.Value;
			fPSFreezer.MaxSleepMs = paramInt2 ?? paramInt.Value;
		}
	}

	private static void SetCameraFov(string parameters)
	{
		float? paramFloat = Utilities.GetParamFloat(parameters, 1, "Can't parse camera fov value '" + parameters + "'");
		if (paramFloat.HasValue)
		{
			CameraZoom cameraZoom = CameraRig.Instance.CameraZoom;
			cameraZoom.FovMin = paramFloat.Value;
			cameraZoom.FovMax = paramFloat.Value;
		}
	}

	private static void SetCameraFovMinMax(string parameters)
	{
		float? paramFloat = Utilities.GetParamFloat(parameters, 1, "Can't parse camera fov min value '" + parameters + "'");
		float? paramFloat2 = Utilities.GetParamFloat(parameters, 2, "Can't parse camera fov max value '" + parameters + "'");
		if (paramFloat.HasValue && paramFloat2.HasValue)
		{
			CameraZoom cameraZoom = CameraRig.Instance.CameraZoom;
			cameraZoom.FovMin = paramFloat.Value;
			cameraZoom.FovMax = paramFloat2.Value;
		}
	}

	[Cheat(Name = "reload_ui", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ReloadUI()
	{
		Game.ResetUI();
	}

	[Cheat(Name = "reset_widget_stash", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ResetWidgetStash()
	{
		WidgetFactoryStash.ResetStash();
	}

	[Cheat(Name = "change_ui_next_platform", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChangeUINextPlatform()
	{
		Game.ChangeUIPlatform(nextPlatform: true);
	}

	[Cheat(Name = "change_ui_prev_platform", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChangeUIPrevPlatform()
	{
		Game.ChangeUIPlatform(nextPlatform: false);
	}

	private static void UnloadScene(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse string parameter \"name\" '" + parameters + "'");
		if (!(paramString == ""))
		{
			SceneManager.UnloadSceneAsync(paramString);
		}
	}

	private static void SetLogSeverity(string parameters)
	{
		if (Utilities.GetArguments(parameters).Length != 3)
		{
			PFLog.SmartConsole.Log("Usage: set_log_severity <ChannelName:string> <Severity:int>");
			PFLog.SmartConsole.Log("Severity:");
			LogSeverity[] array = (LogSeverity[])Enum.GetValues(typeof(LogSeverity));
			foreach (LogSeverity logSeverity in array)
			{
				PFLog.SmartConsole.Log($"\t{(int)logSeverity} - {logSeverity:G}");
			}
			PFLog.SmartConsole.Log("Channels:");
			{
				foreach (string channelName in LogChannelFactory.ChannelNames)
				{
					PFLog.SmartConsole.Log("\t" + channelName);
				}
				return;
			}
		}
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse string parameter \"ChannelName\" '" + parameters + "'");
		int? paramInt = Utilities.GetParamInt(parameters, 2, "Can't parse int parameter \"Severity\" '" + parameters + "'");
		if (!LogChannelFactory.ChannelNames.Contains(paramString))
		{
			PFLog.SmartConsole.Log("No log channel \"" + paramString + "\"");
			return;
		}
		LogChannelFactory.GetOrCreate(paramString).SetSeverity((LogSeverity)paramInt.Value);
		PFLog.SmartConsole.Log($"Severity is set to [{(LogSeverity)paramInt.Value:G}] for log channel [\"{paramString}\"] ");
	}

	private static Dictionary<TextureCategoryByPrefix, CategoryInfo> GetTexturesCountForBuild()
	{
		Texture[] array = Resources.FindObjectsOfTypeAll<Texture>();
		Sprite[] source = Resources.FindObjectsOfTypeAll<Sprite>();
		IEnumerable<Sprite> enumerable = source.Where((Sprite s) => s == null || s.texture == null);
		if (enumerable.Any())
		{
			PFLog.Default.Error("BadSprites found: " + string.Join("\n", enumerable));
		}
		HashSet<int> spriteTextureIds = new HashSet<int>(source.Select((Sprite s) => s?.texture.GetInstanceID() ?? (-1)));
		TMP_FontAsset[] source2 = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
		IEnumerable<TMP_FontAsset> enumerable2 = source2.Where((TMP_FontAsset s) => s == null || s.atlas == null);
		if (enumerable2.Any())
		{
			PFLog.Default.Error("BadFonts found: " + string.Join("\n", enumerable2));
		}
		HashSet<int> fontTextureIds = new HashSet<int>(source2.Select((TMP_FontAsset s) => s.atlas?.GetInstanceID() ?? (-1)));
		Dictionary<TextureCategoryByPrefix, CategoryInfo> dictionary = new Dictionary<TextureCategoryByPrefix, CategoryInfo>();
		foreach (object value in Enum.GetValues(typeof(TextureCategoryByPrefix)))
		{
			dictionary.Add((TextureCategoryByPrefix)value, new CategoryInfo());
		}
		Texture[] array2 = array;
		foreach (Texture texture in array2)
		{
			TextureCategoryByPrefix key = GuessTextureCategory(texture, spriteTextureIds, fontTextureIds);
			dictionary[key].Count++;
			dictionary[key].Size += Profiler.GetRuntimeMemorySizeLong(texture);
		}
		return dictionary;
	}

	private static TextureCategoryByPrefix GuessTextureCategory(Texture texture, HashSet<int> spriteTextureIds, HashSet<int> fontTextureIds)
	{
		bool flag = texture is RenderTexture;
		TextureCategoryByPrefix result = (flag ? TextureCategoryByPrefix.RenderTarget : TextureCategoryByPrefix.Other);
		if (!string.IsNullOrEmpty(texture.name) && !flag)
		{
			if (spriteTextureIds != null && spriteTextureIds.Contains(texture.GetInstanceID()))
			{
				result = (texture.name.Contains("Sprite_Atlas") ? TextureCategoryByPrefix.UI_Atlas : TextureCategoryByPrefix.UI_NonAtlas);
			}
			else if (fontTextureIds != null && fontTextureIds.Contains(texture.GetInstanceID()))
			{
				result = TextureCategoryByPrefix.UI_Font;
			}
			else
			{
				string name = texture.name;
				if (name.StartsWith("UI_"))
				{
					result = TextureCategoryByPrefix.UI_NonAtlas;
				}
				else if (name.StartsWith("BCT_"))
				{
					result = TextureCategoryByPrefix.BakedCharacter;
				}
				else
				{
					switch (name)
					{
					case "Masks_2D":
					case "Diffuse_2D":
					case "Normal_2D":
						result = TextureCategoryByPrefix.DynamicCharacter;
						break;
					default:
						if (name.Substring(2).StartsWith("W_"))
						{
							result = TextureCategoryByPrefix.Weapon;
						}
						else if (name.Contains("Sprite_Atlas"))
						{
							result = TextureCategoryByPrefix.UI_Atlas;
						}
						else if (name.EndsWith(" Atlas"))
						{
							result = TextureCategoryByPrefix.UI_Font;
						}
						else if (name.Length > 3 && name[2] == '_')
						{
							result = TextureCategoryByPrefix.DynamicCharacter;
						}
						break;
					}
				}
			}
		}
		return result;
	}

	public static void TexturesDumpBuild(string parameters)
	{
		Dictionary<TextureCategoryByPrefix, CategoryInfo> texturesCountForBuild = GetTexturesCountForBuild();
		StringBuilder stringBuilder = new StringBuilder();
		double num = 0.0;
		long num2 = 0L;
		foreach (KeyValuePair<TextureCategoryByPrefix, CategoryInfo> item in texturesCountForBuild)
		{
			if (item.Key == TextureCategoryByPrefix.Count)
			{
				break;
			}
			double num3 = (double)item.Value.Size / 1048576.0;
			num += num3;
			num2 += item.Value.Count;
			stringBuilder.AppendLine($"{item.Key,-20}: {num3,10:F2}Mb ({item.Value.Count} textures)");
		}
		string messageFormat = stringBuilder.ToString();
		PFLog.Default.Log(messageFormat);
		PFLog.Default.Log($"Total: {num,10:F2}Mb ({num2} textures)");
	}

	public static UITexturesCountForArbiter GetUITexturesCountForArbiter()
	{
		Dictionary<TextureCategoryByPrefix, CategoryInfo> texturesCountForBuild = GetTexturesCountForBuild();
		UITexturesCountForArbiter result = default(UITexturesCountForArbiter);
		result.UI_Atlases = texturesCountForBuild[TextureCategoryByPrefix.UI_Atlas];
		result.UI_NoAtlases = texturesCountForBuild[TextureCategoryByPrefix.UI_NonAtlas];
		return result;
	}

	public static void TexturesDumpCSVNoResolve(string parameters)
	{
		string text = Path.Combine(ApplicationPaths.DevelopmentDataPath, "textures.csv");
		PFLog.Default.Log("Trying to dump textures to " + text);
		StringBuilder stringBuilder = (Utilities.GetParamBool(parameters, 1, null).GetValueOrDefault() ? new StringBuilder() : null);
		StreamWriter streamWriter;
		try
		{
			streamWriter = new StreamWriter(text);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return;
		}
		string value = "Name, IsRT, Dimensions, Size, SizeText, Category, Readable";
		streamWriter.WriteLine(value);
		stringBuilder?.AppendLine(value);
		Texture[] array = Resources.FindObjectsOfTypeAll<Texture>();
		HashSet<int> spriteTextureIds = new HashSet<int>(from s in Resources.FindObjectsOfTypeAll<Sprite>()
			select s.texture.GetInstanceID());
		HashSet<int> fontTextureIds = new HashSet<int>(from s in Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
			select s.atlas?.GetInstanceID() ?? (-1));
		int num = 0;
		Texture[] array2 = array;
		foreach (Texture texture in array2)
		{
			string text2 = texture.name;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "<empty>";
			}
			bool flag = texture is RenderTexture;
			long runtimeMemorySizeLong = Profiler.GetRuntimeMemorySizeLong(texture);
			double num2 = (double)runtimeMemorySizeLong / 1048576.0;
			TextureCategoryByPrefix textureCategoryByPrefix = GuessTextureCategory(texture, spriteTextureIds, fontTextureIds);
			bool flag2 = !(texture is RenderTexture) && texture.isReadable;
			string value2 = $"{text2}, {flag}, {texture.width,5}x{texture.height,-5}, {runtimeMemorySizeLong}, \"{num2:F2}\", {textureCategoryByPrefix}, {(flag2 ? 'Y' : 'N')}";
			streamWriter.WriteLine(value2);
			stringBuilder?.AppendLine(value2);
			num++;
			if (num % 100 == 0)
			{
				PFLog.Default.Log($"Done: {num}/{array.Length}");
			}
		}
		PFLog.Default.Log($"Done: {num}/{array.Length}");
		PFLog.Default.Log($"Stats: \nTotal: {Texture.totalTextureMemory} \nCurrent: {Texture.currentTextureMemory} \nDesired: {Texture.desiredTextureMemory} \nTarget: {Texture.targetTextureMemory}");
		if (stringBuilder != null)
		{
			PFLog.Default.Log(stringBuilder.ToString());
		}
		streamWriter.Close();
	}

	public static void MeshesDumpCSVNoResolve(string parameters)
	{
		string text = Path.Combine(ApplicationPaths.DevelopmentDataPath, "meshes.csv");
		PFLog.Default.Log("Trying to dump meshes to " + text);
		StreamWriter streamWriter;
		try
		{
			streamWriter = new StreamWriter(text);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return;
		}
		streamWriter.WriteLine("Name, IsReadable, Size, SizeText");
		Mesh[] array = Resources.FindObjectsOfTypeAll<Mesh>();
		int num = 0;
		Mesh[] array2 = array;
		foreach (Mesh obj in array2)
		{
			string text2 = obj.name;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "<empty>";
			}
			bool isReadable = obj.isReadable;
			long runtimeMemorySizeLong = Profiler.GetRuntimeMemorySizeLong(obj);
			double num2 = (double)runtimeMemorySizeLong / 1048576.0;
			streamWriter.WriteLine($"{text2}, {isReadable}, {runtimeMemorySizeLong}, \"{num2:F2}\"");
			num++;
			if (num % 100 == 0)
			{
				PFLog.Default.Log($"Done: {num}/{array.Length}");
			}
		}
		PFLog.Default.Log($"Done: {num}/{array.Length}");
		streamWriter.Close();
	}

	private static void ClipboardCopyBuildInfoHotkey()
	{
		GUIUtility.systemCopyBuffer = ReportVersionManager.GetBuildInfo();
	}

	private static void ClipboardCopyBuildInfoCheat(string parameters = null)
	{
		GUIUtility.systemCopyBuffer = ReportVersionManager.GetBuildInfo();
	}

	private static void OpenGameLogFull()
	{
		OpenGameLogFull(null);
	}

	private static void OpenGameLogFull(string parameters = null)
	{
		try
		{
			Application.OpenURL(Path.Combine(ApplicationPaths.persistentDataPath, "GameLogFull.txt"));
		}
		catch
		{
		}
	}

	private static void ShowDistance(string parameters = null)
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			SmartConsole.Print("Cant get unit under mouse!");
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (UIAccess.SelectionManager.IsSelected(partyAndPet))
			{
				SmartConsole.Print($"{partyAndPet.CharacterName}<->{unitUnderMouse.CharacterName}: {partyAndPet.DistanceTo(unitUnderMouse)}");
			}
		}
	}

	private static void CrashGame(string parameters = null)
	{
		UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.FatalError);
	}

	private static void ExceptionGame(string parameters = null)
	{
		Runner.ReportException(new Exception("DEBUG EXCEPTION " + parameters));
	}

	private static void DebugPersistentPath(string parameters = null)
	{
		SmartConsole.Print("Folder: " + ApplicationPaths.persistentDataPath);
	}

	private static void DebugCurrentDialog(string parameters = null)
	{
		try
		{
			BlueprintDialog dialog = Game.Instance.DialogController.Dialog;
			SmartConsole.Print((dialog != null) ? ("Current dialogue is " + dialog.NameSafe()) : "Cannot find current dialogue");
		}
		catch
		{
			SmartConsole.Print("Cannot find current dialogue");
		}
	}

	private static void ReloadLastSave(string parameters = null)
	{
		if (Game.Instance.SaveManager.GetLatestSave() != null)
		{
			Game.Instance.LoadGame(Game.Instance.SaveManager.GetLatestSave());
		}
	}

	private static void CheckBuildRemoval(string parameters = null)
	{
		RemoveOnBuild[] array = UnityEngine.Object.FindObjectsOfType<RemoveOnBuild>(includeInactive: true);
		SmartConsole.Print("Removal objects found : " + array.Length);
	}

	[Cheat(Name = "return_to_main_menu")]
	public static void ReturnToMainMenu()
	{
		Game.Instance.ResetToMainMenu();
	}

	[Cheat(Name = "log_disposables")]
	public static void LogDisposables()
	{
		BaseDisposable.LogAllUndisposed(new LogChannelLoggerWrapper(PFLog.UI, "Undisposed"));
	}

	private static void TakeSnapshotInternal(CaptureFlags captureFlags, string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = "snapshot";
		}
		MemoryProfiler.TakeSnapshot(Path.Combine(Application.temporaryCachePath, name) + ".snap", delegate(string path, bool success)
		{
			PFLog.Default.Log($"TakeSnapshot result: {success}, path: {path}");
		}, captureFlags);
	}

	[Cheat(Name = "snapshot")]
	public static void TakeSnapshot(string name = null)
	{
		TakeSnapshotInternal(CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations, name);
	}

	[Cheat(Name = "snapshot_full")]
	public static void TakeSnapshotFull(string name = null)
	{
		TakeSnapshotInternal(CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations | CaptureFlags.NativeAllocationSites | CaptureFlags.NativeStackTraces, name);
	}

	[Cheat(Name = "snapshot_objects")]
	public static void TakeSnapshotNativeOnly(string name = null)
	{
		TakeSnapshotInternal(CaptureFlags.NativeObjects, name);
	}

	[Cheat(Name = "debug_spam_start", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DebugSpamStart(string spamType = "exceptions", int intervalMs = 10, int depth = 10)
	{
		DebugStartSpam(spamType, intervalMs, depth);
	}

	[Cheat(Name = "debug_spam_exceptions", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DebugExceptionSpam(int count = 5, int depth = 10, int interval = 10)
	{
		GameObject gameObject = GameObject.Find("SpamGenerator");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		gameObject = new GameObject("SpamGenerator");
		gameObject.AddComponent<LogsSpammer>();
		gameObject.GetComponent<LogsSpammer>().SetExceptionSpam(count, depth, interval);
	}

	[Cheat(Name = "debug_spam_start_in_outer_thread", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DebugOffThread()
	{
		new Thread((ParameterizedThreadStart)delegate
		{
			PFLog.Default.Error("Debug log in other thread");
			Thread.Sleep(100);
			PFLog.Default.Error("Debug log in other thread after 100");
		}).Start();
	}

	[Cheat(Name = "debug_start_spam", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DebugStartSpam(string spamType = "exceptions", int intervalMs = 10, int depth = 10)
	{
		GameObject gameObject = GameObject.Find("SpamGenerator");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		gameObject = new GameObject("SpamGenerator");
		gameObject.AddComponent<LogsSpammer>();
		LogsSpammer component = gameObject.GetComponent<LogsSpammer>();
		if (!Enum.TryParse<SpamType>(spamType, ignoreCase: true, out var result))
		{
			result = SpamType.Exceptions;
		}
		component.StartSpam(result, intervalMs, depth);
	}

	[Cheat(Name = "debug_stop_spam", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DebugStopSpam()
	{
		GameObject gameObject = GameObject.Find("SpamGenerator");
		if (!(gameObject == null))
		{
			gameObject.AddComponent<LogsSpammer>();
			gameObject.GetComponent<LogsSpammer>().StopSpam();
		}
	}

	[Cheat(Name = "disable_logging", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DisableLogging()
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = false;
	}

	[Cheat(Name = "enable_logging", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void EnableLogging()
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = true;
	}
}
