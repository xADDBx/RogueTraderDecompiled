using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MainMenuLightController : MonoBehaviour
{
	public List<MainMenuLightSorce> LightsSorces;

	public void Update()
	{
		foreach (MainMenuLightSorce lightsSorce in LightsSorces)
		{
			if (lightsSorce == null || lightsSorce.CurrentTween == null || lightsSorce.PrevTween == null)
			{
				break;
			}
			lightsSorce.CurrentTween.TickTween(lightsSorce, Time.unscaledDeltaTime);
			if (lightsSorce.LightSorce.intensity == lightsSorce.CurrentTween.Intensivity && lightsSorce.LightSorce.range == lightsSorce.CurrentTween.Range)
			{
				lightsSorce.PrevTween = null;
			}
		}
	}

	public void StartLightTween(MainMenuLightSorce Light, LightTweenAnchor tweenAnchor, bool animation = true)
	{
		PFLog.MainMenuLight.Log($"MainMenuLightController.StartLightTween({Light.LightSorce}, {tweenAnchor}, {animation})");
		Light.PrevTween = Light.CurrentTween;
		if (animation)
		{
			Light.CurrentTween = tweenAnchor;
			if (Light.CurrentTween != null)
			{
				Light.CurrentTween.StartTween(Light.LightSorce, Light.CurrentTween);
			}
			PFLog.MainMenuLight.Log($"light tween started for {Light.LightSorce}, target = {Light.CurrentTween}");
			return;
		}
		Light.CurrentTween = tweenAnchor;
		if (Light.CurrentTween != null)
		{
			Light.LightSorce.intensity = Light.CurrentTween.Intensivity;
			Light.LightSorce.range = Light.CurrentTween.Range;
		}
		PFLog.MainMenuLight.Log($"light settings set for {Light.LightSorce}, target = {Light.CurrentTween}");
	}
}
