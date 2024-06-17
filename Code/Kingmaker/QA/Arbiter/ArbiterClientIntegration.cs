using System;
using System.Collections.Generic;
using System.IO;
using Core.Cheats;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public static class ArbiterClientIntegration
{
	public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
	{
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		Formatting = Formatting.Indented,
		TypeNameHandling = TypeNameHandling.Auto
	};

	public static void TakeScreenshot(string probeDataDataFolder, int getSampleId)
	{
		File.WriteAllBytes(Path.Combine(probeDataDataFolder, $"{getSampleId}.png"), CameraStackScreenshoter.TakePNG(Arbiter.Root.Resolution.x, Arbiter.Root.Resolution.y));
	}

	[Cheat(Name = "weather_set", Description = "Сменить погоду")]
	public static InclemencyType SetWeather(InclemencyType type = InclemencyType.Clear)
	{
		InclemencyType inclemency = VFXWeatherSystem.Instance.SystemForWeatherController.Inclemency;
		VFXWeatherSystem.Instance.SystemForWeatherController.SetInclemency(type);
		return inclemency;
	}

	[Cheat(Name = "clouds_disable", Description = "Отключить облака")]
	public static void DisableClouds()
	{
		UnityEngine.Object.FindObjectsOfType<ScreenSpaceCloudShadows>().ForEach(delegate(ScreenSpaceCloudShadows sscs)
		{
			sscs.active = false;
		});
	}

	[Cheat(Name = "clouds_enable", Description = "Включить облака")]
	public static void EnableClouds()
	{
		UnityEngine.Object.FindObjectsOfType<ScreenSpaceCloudShadows>().ForEach(delegate(ScreenSpaceCloudShadows sscs)
		{
			sscs.active = true;
		});
	}

	[Cheat(Name = "fog_disable", Description = "Отключить туманных объемы")]
	public static void DisableFog(bool disable = true)
	{
	}

	[Cheat(Name = "fog_enable", Description = "Включить туманных объемы")]
	public static void EnableFog(bool disable = true)
	{
	}

	[Cheat(Name = "wind_disable", Description = "Отключить ветер")]
	public static void DisableWind(bool disable = true)
	{
		VFXWeatherSystem vFXWeatherSystem = UnityEngine.Object.FindObjectOfType<VFXWeatherSystem>();
		vFXWeatherSystem.enabled = false;
		vFXWeatherSystem.Stop();
	}

	[Cheat(Name = "wind_enable", Description = "Включить ветер")]
	public static void EnableWind(bool disable = true)
	{
		VFXWeatherSystem vFXWeatherSystem = UnityEngine.Object.FindObjectOfType<VFXWeatherSystem>();
		vFXWeatherSystem.enabled = true;
		vFXWeatherSystem.Play();
	}

	[Cheat(Name = "fow_disable", Description = "Отключить туман войны")]
	public static void DisableFow()
	{
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = false;
		}
	}

	[Cheat(Name = "fow_enable", Description = "Включить туман войны")]
	public static void EnableFow()
	{
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = true;
		}
	}

	[Cheat(Name = "vsync_set", Description = "Установить количество v-синков между кадрами")]
	public static int SetVSync(int value)
	{
		int vSyncCount = QualitySettings.vSyncCount;
		QualitySettings.vSyncCount = value;
		return vSyncCount;
	}

	[Cheat(Name = "fx_disable", Description = "Отключить fx")]
	public static void DisableFx()
	{
		UnityEngine.Object.FindObjectsOfType<ParticleSystem>().ForEach(delegate(ParticleSystem ps)
		{
			ps.Stop();
		});
	}

	[Cheat(Name = "fx_enable", Description = "Включить fx")]
	public static void EnableFx()
	{
		UnityEngine.Object.FindObjectsOfType<ParticleSystem>().ForEach(delegate(ParticleSystem ps)
		{
			ps.Play();
		});
	}

	private static MonoBehaviour[] FindGameObjectsInLayer(int layer)
	{
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
		List<MonoBehaviour> list = new List<MonoBehaviour>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.layer == layer)
			{
				list.Add(array[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	[Cheat(Name = "units_hide", Description = "Скрыть юниты")]
	public static void HideUnits()
	{
		Game.Instance.DynamicRoot.Hide();
		Game.Instance.CrossSceneRoot.Hide();
		UnityEngine.Object.FindObjectsOfType<OccludedObjectDepthClipper>().ForEach(delegate(OccludedObjectDepthClipper oodc)
		{
			oodc.enabled = false;
		});
	}

	[Cheat(Name = "units_show", Description = "Показать юниты")]
	public static void ShowUnits()
	{
		Game.Instance.DynamicRoot.Unhide();
		Game.Instance.CrossSceneRoot.Unhide();
		UnityEngine.Object.FindObjectsOfType<OccludedObjectDepthClipper>().ForEach(delegate(OccludedObjectDepthClipper oodc)
		{
			oodc.enabled = true;
		});
	}

	[Cheat(Name = "ui_hide", Description = "Скрыть UI")]
	public static void HideUi()
	{
		UIVisibilityState.HideAllUI();
	}

	[Cheat(Name = "ui_show", Description = "Показать UI")]
	public static void ShowUi()
	{
		UIVisibilityState.ShowAllUI();
	}

	public static void AddCameraRig()
	{
		CameraRig.Instance.FixCamera++;
	}

	public static void RemoveCameraRig()
	{
		CameraRig.Instance.FixCamera--;
	}

	public static bool SetGamePause(bool value)
	{
		bool isPaused = Game.Instance.IsPaused;
		Game.Instance.IsPaused = value;
		return isPaused;
	}

	public static void EnableEnterPoints(IEnumerable<BlueprintAreaEnterPointReference> enterPointBlueprints)
	{
		AreaEnterPoint[] source = Resources.FindObjectsOfTypeAll<AreaEnterPoint>();
		foreach (BlueprintAreaEnterPointReference epb in enterPointBlueprints)
		{
			AreaEnterPoint areaEnterPoint = source.FirstOrDefault((AreaEnterPoint x) => x.Blueprint == epb.GetBlueprint());
			if ((object)areaEnterPoint == null)
			{
				throw new Exception($"Enter point [{epb}] is not found in area [{Game.Instance.CurrentlyLoadedArea}]");
			}
			if (!areaEnterPoint.gameObject.activeInHierarchy)
			{
				areaEnterPoint.gameObject.SetActive(value: true);
			}
		}
	}

	public static void TeleportToEnterPoint(BlueprintAreaEnterPointReference enterPoint)
	{
		Game.Instance.Teleport(enterPoint);
	}

	public static void TeleportToLocalPoint(Vector3 vector3)
	{
		CheatsTransfer.LocalTeleport(vector3);
	}

	public static void MoveCameraToPoint(ArbiterPoint point)
	{
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(point.Position);
		instance.RotateToImmediately(point.Rotation);
		instance.CameraZoom.ZoomToImmediate(point.Zoom);
	}

	public static void SetQaMode(bool b)
	{
		CheatsJira.QaMode = false;
	}

	public static bool IsMainMenuActive()
	{
		if (MainMenuUI.IsActive)
		{
			return MainMenuUI.Instance != null;
		}
		return false;
	}

	public static T DeserializeObject<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, JsonSettings);
	}

	public static string SerializeObject<T>(T @object)
	{
		return JsonConvert.SerializeObject(@object, JsonSettings);
	}

	public static void MoveCameraImmediately(Vector3 position, float rotation, float zoom)
	{
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(position);
		instance.RotateToImmediately(rotation);
		instance.CameraZoom.ZoomToImmediate(zoom);
		instance.FixCamera++;
	}

	public static SceneReference GetLightSceneFromAreaPart(BlueprintAreaPart area, TimeOfDay timeOfDay)
	{
		return area.GetLightScene();
	}

	public static Transform FindChildRecursive(Transform transform, string name)
	{
		return transform.FindChildRecursive(name);
	}

	public static void HandleFatalError(Exception exception)
	{
		Arbiter.Instance.HandleFatalError(exception);
	}
}
