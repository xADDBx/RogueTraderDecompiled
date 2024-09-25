using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.RenderPipeline.Decals;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class WeatherLayerController : IWeatherEntityController, IDisposable
{
	private Transform m_Root;

	private WeatherLayer m_Layer;

	private FullScreenDecal m_Decal;

	private Volume m_PostProcessVolume;

	private List<WeatherEffectController> m_EffectControllers;

	private List<IWeatherEntityController> m_CustomEffectControllers;

	public IEnumerable<WeatherEffectController> EffectControllers => m_EffectControllers;

	public WeatherLayerController(Transform root, WeatherLayer layer)
	{
		m_Root = root;
		m_Layer = layer;
		m_Decal = CreateDecal(m_Layer.DecalSettings.DecalPrefab);
		m_PostProcessVolume = CreatePostProcessVolume(m_Layer.PostProcessVolumeSettings);
		m_EffectControllers = (from effect in m_Layer.Effects?.Where((WeatherEffect effect) => effect != null && effect.VisualEffectPrefab != null)
			select new WeatherEffectController(m_Root, effect)).ToList() ?? new List<WeatherEffectController>();
		m_CustomEffectControllers = (from effect in m_Layer.CustomEffects?.Where((WeatherCustomEntitySettings effect) => effect != null)
			select effect.GetController(m_Root)).ToList() ?? new List<IWeatherEntityController>();
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if (m_Layer.FogSettings.Enabled)
		{
			float t = m_Layer.FogSettings.FogIntensityOverRootIntensity.Evaluate(weatherIntensity);
			RenderSettings.fog = true;
			RenderSettings.fogMode = FogMode.Linear;
			RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, m_Layer.FogSettings.Color, t);
			RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, m_Layer.FogSettings.StartDistance, t);
			RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, m_Layer.FogSettings.EndDistance, t);
		}
		if (m_Decal != null && m_Decal.Material != null)
		{
			float num = m_Layer.DecalSettings.AlphaOverIntensity.Evaluate(weatherIntensity);
			m_Decal.gameObject.SetActive(num > 0f);
			m_Decal.Material.SetFloat("_AlphaScale", num);
		}
		if (m_PostProcessVolume != null)
		{
			m_PostProcessVolume.weight = m_Layer.PostProcessVolumeSettings.WeightOverLayerIntensity.Evaluate(weatherIntensity);
		}
		foreach (WeatherEffectController effectController in m_EffectControllers)
		{
			effectController.Update(camera, weatherIntensity, windDirection, windIntensity);
		}
		foreach (IWeatherEntityController customEffectController in m_CustomEffectControllers)
		{
			customEffectController.Update(camera, weatherIntensity, windDirection, windIntensity);
		}
	}

	public void UpdateBakedGroundParameters()
	{
		foreach (WeatherEffectController effectController in m_EffectControllers)
		{
			effectController.UpdateBakedGroundParameters();
		}
	}

	public void SetAreaBounds(Bounds bounds)
	{
		if (m_EffectControllers == null || m_EffectControllers.Count == 0)
		{
			return;
		}
		foreach (WeatherEffectController effectController in m_EffectControllers)
		{
			effectController.SetAreaBounds(bounds);
		}
	}

	private Volume CreatePostProcessVolume(PostProcessVolumeSettings settings)
	{
		if (settings == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("Post Process Volume '" + m_Layer.name + "'");
		gameObject.layer = settings.VolumeLayer;
		gameObject.transform.parent = m_Root;
		Volume volume = gameObject.AddComponent<Volume>();
		volume.profile = settings.Profile;
		volume.priority = settings.Priority;
		volume.weight = 0f;
		return volume;
	}

	private FullScreenDecal CreateDecal(GameObject decalPrefab)
	{
		if (decalPrefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(decalPrefab);
		gameObject.transform.parent = m_Root.transform;
		return gameObject.GetComponent<FullScreenDecal>();
	}

	public void Dispose()
	{
		foreach (WeatherEffectController effectController in m_EffectControllers)
		{
			((IDisposable)effectController).Dispose();
		}
		foreach (IWeatherEntityController customEffectController in m_CustomEffectControllers)
		{
			customEffectController.Dispose();
		}
		m_EffectControllers = null;
		m_CustomEffectControllers = null;
	}
}
