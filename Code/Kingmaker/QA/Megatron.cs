using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Utility;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.QA;

public class Megatron : MonoBehaviour
{
	private static readonly string CurrentTime;

	private static readonly JsonSerializerSettings JsonSettings;

	public static bool IsActive { get; private set; }

	static Megatron()
	{
		CurrentTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
		JsonSettings = new JsonSerializerSettings
		{
			PreserveReferencesHandling = PreserveReferencesHandling.None,
			Formatting = Formatting.Indented
		};
	}

	private void Awake()
	{
		StartCoroutine(LoadDataAsync());
	}

	private IEnumerator LoadDataAsync()
	{
		CommandLineArguments commandLineArguments = CommandLineArguments.Parse();
		string[] array = commandLineArguments.Get("-size").Split('x');
		if (array.Length == 2)
		{
			Screen.SetResolution(int.Parse(array[0]), int.Parse(array[1]), fullscreen: false);
		}
		string text = commandLineArguments.Get("-megatron");
		string text2 = File.ReadAllText((text.Length > 0) ? text : "megatron.json");
		if (text2.Length == 0)
		{
			yield break;
		}
		List<PointData> points = JsonConvert.DeserializeObject<List<PointData>>(text2, JsonSettings);
		if (points.Count == 0)
		{
			yield break;
		}
		IsActive = true;
		SceneLoader.LoadObligatoryScenes();
		Game.Instance.LoadNewGame();
		yield return new WaitWhile(() => LoadingProcess.Instance.IsLoadingInProcess);
		ILookup<string, PointData> lookup = points.ToLookup((PointData point) => point.Area, (PointData point) => point);
		foreach (IGrouping<string, PointData> areaGroup in lookup)
		{
			BlueprintArea area = (BlueprintArea)ResourcesLibrary.TryGetBlueprint(areaGroup.Key);
			if (area == null)
			{
				PFLog.Default.Log("Can't get area for " + areaGroup.Key);
				continue;
			}
			PFLog.Default.Log("Processing " + area.AssetGuid);
			Time.timeScale = 0f;
			Game.Instance.LoadArbiter(area);
			GameObject gameObject = GameObject.Find("ScreenSpaceCloudShadow");
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
			GameObject gameObject2 = GameObject.Find("ScreenSpaceCloudShadowUnblurred");
			if (gameObject != null)
			{
				gameObject2.SetActive(value: false);
			}
			yield return new WaitWhile(() => LoadingProcess.Instance.IsLoadingInProcess);
			try
			{
				Game.Instance.IsPaused = true;
				if (FogOfWarArea.Active != null)
				{
					FogOfWarArea.Active.enabled = false;
				}
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
			foreach (PointData item in areaGroup)
			{
				yield return StartCoroutine(CaptureScreenshot(area, item));
			}
		}
		yield return new WaitForSecondsRealtime(3f);
		Application.Quit();
	}

	public static string GetScreenshotPath(PointData point, long time)
	{
		string path = Path.Combine(Path.GetFullPath("."), "Screenshots");
		string area = point.Area;
		string text = Path.Combine(Path.Combine(path, area), CurrentTime);
		Directory.CreateDirectory(text);
		string path2 = point.Point + "-" + time;
		return Path.Combine(text, path2);
	}

	private static IEnumerator CaptureScreenshot(BlueprintArea area, PointData point)
	{
		CameraRig.Instance.ScrollToImmediately(point.Position);
		ScreenshotData data = new ScreenshotData
		{
			Point = point,
			Time = (long)Game.Instance.Player.GameTime.TotalHours,
			FPS = new int[128]
		};
		int i = 0;
		while (i < data.FPS.Length)
		{
			yield return new WaitForEndOfFrame();
			data.FPS[i] = Mathf.RoundToInt(1f / Time.deltaTime);
			int num = i + 1;
			i = num;
		}
		string screenshotPath = GetScreenshotPath(point, data.Time);
		File.WriteAllBytes(screenshotPath + ".png", Screenshot.CapturePNG(Game.GetCamera()));
		File.WriteAllText(screenshotPath + ".json", JsonConvert.SerializeObject(data, JsonSettings));
	}
}
