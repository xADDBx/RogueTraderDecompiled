using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Controllers;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore;

public static class ArbiterIntegration
{
	public static void TakeScreenshot(string probeDataDataFolder, int getSampleId)
	{
		int num = 1920;
		int num2 = 1080;
		RenderTexture renderTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32);
		CameraStackScreenshoter.TakeScreenshot(renderTexture);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
		texture2D.Apply();
		File.WriteAllBytes(bytes: texture2D.EncodeToPNG(), path: Path.Combine(probeDataDataFolder, $"{getSampleId}.png"));
		RenderTexture.active = active;
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(texture2D);
	}

	public static InclemencyType SetWeather(InclemencyType type)
	{
		InclemencyType inclemency = VFXWeatherSystem.Instance.SystemForWeatherController.Inclemency;
		VFXWeatherSystem.Instance.SystemForWeatherController.SetInclemency(type);
		return inclemency;
	}

	public static void DisableClouds()
	{
		UnityEngine.Object.FindObjectsOfType<ScreenSpaceCloudShadows>().ForEach(delegate(ScreenSpaceCloudShadows sscs)
		{
			sscs.active = false;
		});
	}

	public static void EnableClouds()
	{
		UnityEngine.Object.FindObjectsOfType<ScreenSpaceCloudShadows>().ForEach(delegate(ScreenSpaceCloudShadows sscs)
		{
			sscs.active = true;
		});
	}

	public static void DisableFog(bool disable = true)
	{
	}

	public static void EnableFog(bool disable = true)
	{
	}

	public static void DisableWind()
	{
		VFXWeatherSystem vFXWeatherSystem = UnityEngine.Object.FindObjectOfType<VFXWeatherSystem>();
		vFXWeatherSystem.enabled = false;
		vFXWeatherSystem.Stop();
	}

	public static void EnableWind()
	{
		VFXWeatherSystem vFXWeatherSystem = UnityEngine.Object.FindObjectOfType<VFXWeatherSystem>();
		vFXWeatherSystem.enabled = true;
		vFXWeatherSystem.Play();
	}

	public static void DisableFow()
	{
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = false;
		}
	}

	public static void EnableFow()
	{
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = true;
		}
	}

	public static void DisableFx()
	{
		UnityEngine.Object.FindObjectsOfType<ParticleSystem>().ForEach(delegate(ParticleSystem ps)
		{
			ps.Stop();
			ps.Clear();
		});
	}

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

	public static void HideUnits()
	{
		Game.Instance.DynamicRoot.Hide();
		Game.Instance.CrossSceneRoot.Hide();
		UnityEngine.Object.FindObjectsOfType<OccludedObjectDepthClipper>().ForEach(delegate(OccludedObjectDepthClipper oodc)
		{
			oodc.enabled = false;
		});
	}

	public static void ShowUnits()
	{
		Game.Instance.DynamicRoot.Unhide();
		Game.Instance.CrossSceneRoot.Unhide();
		UnityEngine.Object.FindObjectsOfType<OccludedObjectDepthClipper>().ForEach(delegate(OccludedObjectDepthClipper oodc)
		{
			oodc.enabled = true;
		});
	}

	public static void HideUi()
	{
		UIVisibilityState.HideAllUI();
	}

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
		Game.Instance.SetIsPauseForce(value);
		return isPaused;
	}

	public static void TeleportToEnterPoint(BlueprintAreaEnterPointReference enterPoint)
	{
		Game.Instance.Teleport(enterPoint);
	}

	public static void MoveCameraImmediately(Vector3 position, float rotation, float zoom)
	{
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(position);
		instance.RotateToImmediately(rotation);
		instance.CameraZoom.ZoomToImmediate(zoom);
		instance.FixCamera++;
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
		ArbiterService.Instance.HandleFatalError(exception);
	}

	public static void StartGame(BlueprintAreaPreset preset)
	{
		MainMenuUI.Instance.EnterGame(delegate
		{
			Game.Instance.LoadNewGame(preset);
		});
	}

	public static void AddRevealer(Transform transform)
	{
		FogOfWarControllerData.AddRevealer(transform);
	}

	public static void RemoveRevealer(Transform transform)
	{
		FogOfWarControllerData.RemoveRevealer(transform);
	}

	public static IEnumerable<BlueprintAreaPart> GetPartsToLoad(BlueprintArea area, BlueprintAreaPart part)
	{
		return area.PartsAndSelf;
	}

	public static SceneReference[] GetLightScenes(BlueprintAreaEnterPoint enterPoint)
	{
		BlueprintArea area = enterPoint.Area;
		BlueprintAreaPart areaPart = enterPoint.AreaPart;
		IEnumerable<BlueprintAreaPart> partsToLoad = GetPartsToLoad(area, areaPart);
		if (!area.HasLight)
		{
			return new SceneReference[0];
		}
		if (!area.IsSingleLightScene)
		{
			return partsToLoad.Select((BlueprintAreaPart p) => p.GetLightScene()).ToArray();
		}
		return new SceneReference[1] { areaPart.GetLightScene() };
	}

	public static bool EnableEnterPoint(BlueprintAreaEnterPointReference bpep)
	{
		AreaEnterPoint areaEnterPoint = Resources.FindObjectsOfTypeAll<AreaEnterPoint>().FirstOrDefault((AreaEnterPoint x) => x.Blueprint == bpep.GetBlueprint());
		if (areaEnterPoint == null)
		{
			return false;
		}
		if (!areaEnterPoint.gameObject.activeInHierarchy)
		{
			areaEnterPoint.gameObject.SetActive(value: true);
		}
		return true;
	}
}
