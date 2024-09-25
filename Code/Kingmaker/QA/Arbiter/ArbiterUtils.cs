using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.LocalMap;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public class ArbiterUtils
{
	private static float m_LoadingTime;

	private static string m_LoadingName;

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

	public static IEnumerable<SceneReference> GetScenes(BlueprintAreaEnterPoint enterPoint)
	{
		if (enterPoint == null)
		{
			return Array.Empty<SceneReference>();
		}
		BlueprintArea area = enterPoint.Area;
		BlueprintAreaPart areaPart = enterPoint.AreaPart;
		return from x in (from p in GetPartsToLoad(area, areaPart)
				select new SceneReference[3]
				{
					p?.GetLightScene(),
					p?.DynamicScene,
					p?.MainStaticScene
				}).SelectMany((SceneReference[] x) => x).NotNull()
			where !Arbiter.Root.IsSceneIgnoredInReport(x)
			select x;
	}

	public static IEnumerable<SceneReference> GetScenes(string instructionName)
	{
		List<SceneReference> list = new List<SceneReference>();
		AreaCheckerComponent areaCheckerComponent = ArbiterInstructionIndex.Instance.GetInstruction(instructionName)?.GetTest<AreaCheckerComponent>();
		if (areaCheckerComponent != null)
		{
			foreach (AreaCheckerComponentPart element in areaCheckerComponent.AreaParts.ElementList)
			{
				list.AddRange(GetScenes(element.EnterPoint));
			}
		}
		return list;
	}

	public static string GetAreaName(string instructionName)
	{
		return (ArbiterInstructionIndex.Instance.GetInstruction(instructionName)?.GetTest<AreaCheckerComponent>())?.Area.GetBlueprint().name;
	}

	public static void CheckTimeout(string loadingName)
	{
		if (loadingName == m_LoadingName)
		{
			m_LoadingTime += Time.unscaledDeltaTime;
			if (m_LoadingTime > (float)Arbiter.ProcessTimeout)
			{
				throw new Exception(m_LoadingName + " - timeout is reached");
			}
		}
		else
		{
			m_LoadingTime = 0f;
			m_LoadingName = loadingName;
		}
	}

	public static void SaveAreaMapImageToFile(string path, bool cropToMapSize = false)
	{
		PFLog.Arbiter.Log("Making map screenshot");
		int num = 2048;
		int num2 = 2048;
		WarhammerLocalMapRenderer.DrawResults drawResults = WarhammerLocalMapRenderer.Instance.Draw();
		RenderTexture.active = drawResults.ColorRT;
		if (cropToMapSize)
		{
			num = drawResults.ColorRT.width;
			num2 = drawResults.ColorRT.height;
		}
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, drawResults.ColorRT.width, drawResults.ColorRT.height), (num - drawResults.ColorRT.width) / 2, (num2 - drawResults.ColorRT.height) / 2);
		texture2D.Apply();
		File.WriteAllBytes(path, texture2D.EncodeToPNG());
	}

	internal static string GetPlatform()
	{
		if (Application.isEditor)
		{
			return "Editor";
		}
		return Application.platform.ToString("G").Replace("Player", "");
	}
}
