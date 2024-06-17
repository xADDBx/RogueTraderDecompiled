using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class VFXWeatherSystem : MonoBehaviour
{
	private IWeatherProfile m_Profile;

	public Camera Camera;

	private GameObject m_Root;

	private FogSettings m_DefaultFogSettings = new FogSettings();

	private List<WeatherLayerController> m_LayerControllers;

	private List<IWeatherEntityController> m_CustomEffectControllers;

	private List<Volume> m_PostProcessVolumes;

	private List<WeatherDirectionalLightController> m_DirectionalLightControllers;

	private Bounds m_AreaBounds;

	private bool m_WeatherIsPlaying;

	private float m_PreviousIntensity;

	private float m_NewIntensity;

	private float m_CurrentWeatherIntensity;

	private bool m_OverrideProfile;

	private IWeatherProfile m_ProfileForOverride;

	private VFXWindSystem m_WindSystem;

	private bool m_WindIsPlaying;

	public bool UseDebugParams;

	public float DebugWeatherIntensity;

	public bool LerpWindIntensity;

	public float DebugWindIntensity;

	public Vector2 DebugWindDirection;

	public WeatherProfileExtended DebugWeatherProfile;

	public static VFXWeatherSystem Instance { get; private set; }

	public InclemencyDependentSystem SystemForWeatherController { get; private set; } = new InclemencyDependentSystem();


	public InclemencyDependentSystem SystemForWindController { get; private set; } = new InclemencyDependentSystem();


	public IWeatherProfile Profile
	{
		get
		{
			if (!UseDebugParams || DebugWeatherProfile == null)
			{
				if (!m_OverrideProfile || m_ProfileForOverride == null)
				{
					return m_Profile;
				}
				return m_ProfileForOverride;
			}
			return DebugWeatherProfile;
		}
		set
		{
			m_Profile = value;
		}
	}

	public float CurrentWeatherIntensity => m_CurrentWeatherIntensity;

	public bool IsProfileOverriden => m_OverrideProfile;

	public VFXWindSystem WindSystem => m_WindSystem;

	public bool WeatherIsPlaying => m_WeatherIsPlaying;

	public bool WindIsPlaying => m_WindIsPlaying;

	public InclemencyType CurrentWeatherInclemency => SystemForWeatherController.Inclemency;

	public InclemencyType CurrentWindInclemency => SystemForWindController.Inclemency;

	public float WindIntensity => m_WindSystem?.CurrentIntensity ?? 0f;

	public Vector2 WindDirection => m_WindSystem?.CurrentDirection ?? Vector2.zero;

	public event Action<InclemencyType> OnDebugSetInclemency;

	private void Start()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Reset()
	{
		Stop();
		Play();
	}

	public void SetAreaBounds(Bounds bounds)
	{
		m_AreaBounds = bounds;
		if (m_LayerControllers == null || m_LayerControllers.Count == 0)
		{
			return;
		}
		foreach (WeatherLayerController layerController in m_LayerControllers)
		{
			layerController.SetAreaBounds(m_AreaBounds);
		}
	}

	private void Update()
	{
		if (!WindIsPlaying && !WeatherIsPlaying)
		{
			return;
		}
		if (WindIsPlaying)
		{
			if (UseDebugParams)
			{
				DebugUpdateWind();
			}
			else
			{
				UpdateWind();
			}
		}
		if (WeatherIsPlaying)
		{
			if (VFXTotalLocationWeatherData.AreaSetChanged)
			{
				UpdateBakedGroundParameters();
			}
			if (UseDebugParams)
			{
				DebugUpdateWeather();
			}
			else
			{
				UpdateWeather();
			}
		}
		SystemForWindController.InclemencyChanged = false;
		SystemForWeatherController.InclemencyChanged = false;
	}

	public void Play()
	{
		if (WeatherIsPlaying || Profile == null)
		{
			return;
		}
		if (Profile.Layers != null && Profile.Layers.Length != 0)
		{
			if (ValidateLocationWeatherData(Profile))
			{
				m_Root = new GameObject("[Root]");
				m_Root.transform.parent = base.transform;
				m_DirectionalLightControllers = ((Profile.DirectionalLightSettings == null) ? null : (from light in UnityEngine.Object.FindObjectsOfType<Light>()?.Where((Light layer) => layer != null)
					where light.type == LightType.Directional && light.gameObject.GetComponent<WeatherDirectionalLightExcluder>() == null
					select new WeatherDirectionalLightController(Profile.DirectionalLightSettings, light)).ToList()) ?? new List<WeatherDirectionalLightController>();
				m_LayerControllers = (from layer in Profile.Layers?.Where((WeatherLayer layer) => layer != null)
					select new WeatherLayerController(m_Root.transform, layer)).ToList() ?? new List<WeatherLayerController>();
				foreach (WeatherLayerController layerController in m_LayerControllers)
				{
					layerController.SetAreaBounds(m_AreaBounds);
				}
				m_CustomEffectControllers = (from effect in Profile.CustomEffects?.Where((WeatherCustomEntitySettings setting) => setting != null)
					select effect.GetController(m_Root.transform)).ToList() ?? new List<IWeatherEntityController>();
				m_DefaultFogSettings.Enabled = RenderSettings.fog;
				m_DefaultFogSettings.FogMode = RenderSettings.fogMode;
				m_DefaultFogSettings.Color = RenderSettings.fogColor;
				m_DefaultFogSettings.StartDistance = RenderSettings.fogStartDistance;
				m_DefaultFogSettings.EndDistance = RenderSettings.fogEndDistance;
				m_WeatherIsPlaying = true;
			}
			else
			{
				Debug.LogError("Location Weather Data is null");
			}
		}
		if (Profile.WindProfile != null)
		{
			m_WindSystem = new VFXWindSystem(Profile.WindProfile);
			m_WindSystem.SetNewWeatherInclemency(SystemForWeatherController.Inclemency);
			m_WindSystem.SetNewWindInclemency(SystemForWindController.Inclemency);
			m_WindIsPlaying = true;
		}
	}

	public void Stop()
	{
		if (WeatherIsPlaying)
		{
			foreach (WeatherLayerController layerController in m_LayerControllers)
			{
				((IDisposable)layerController).Dispose();
			}
			m_LayerControllers = null;
			foreach (IWeatherEntityController customEffectController in m_CustomEffectControllers)
			{
				customEffectController.Dispose();
			}
			m_CustomEffectControllers = null;
			foreach (WeatherDirectionalLightController directionalLightController in m_DirectionalLightControllers)
			{
				((IDisposable)directionalLightController).Dispose();
			}
			m_DirectionalLightControllers = null;
			ResetFog();
			UnityEngine.Object.DestroyImmediate(m_Root);
		}
		if (WindIsPlaying)
		{
			m_WindSystem.Dispose();
			m_WindSystem = null;
		}
		m_WeatherIsPlaying = false;
		m_WindIsPlaying = false;
	}

	private void UpdateWind()
	{
		if (SystemForWeatherController.InclemencyChanged)
		{
			m_WindSystem.SetNewWeatherInclemency(SystemForWeatherController.Inclemency);
		}
		if (SystemForWindController.InclemencyChanged)
		{
			m_WindSystem.SetNewWindInclemency(SystemForWindController.Inclemency);
		}
		m_WindSystem.Update(SystemForWeatherController.InclemencyChangePercentage, SystemForWindController.InclemencyChangePercentage);
	}

	private void DebugUpdateWind()
	{
		float num = WeatherMinMaxArray.TransferValue(Profile.InclemencyIntensityRanges, Profile.WindProfile.WindIntensityRanges, DebugWeatherIntensity);
		if (LerpWindIntensity)
		{
			InclemencyType correspondingInclemency = Profile.InclemencyIntensityRanges.GetCorrespondingInclemency(DebugWeatherIntensity);
			num = Mathf.Lerp(DebugWindIntensity, num, Profile.WindProfile.WindLerpValues[correspondingInclemency]);
		}
		m_WindSystem.DebugUpdate(num, DebugWindDirection);
	}

	private void UpdateWeather()
	{
		ResetFog();
		if (SystemForWeatherController.InclemencyChanged)
		{
			m_PreviousIntensity = m_NewIntensity;
			m_NewIntensity = Profile.InclemencyIntensityRanges.Sample(SystemForWeatherController.Inclemency);
		}
		m_CurrentWeatherIntensity = Mathf.Lerp(m_PreviousIntensity, m_NewIntensity, SystemForWeatherController.InclemencyChangePercentage);
		UpdateAllControllers();
	}

	private void DebugUpdateWeather()
	{
		ResetFog();
		m_CurrentWeatherIntensity = DebugWeatherIntensity;
		UpdateAllControllers();
	}

	private void UpdateAllControllers()
	{
		foreach (WeatherLayerController layerController in m_LayerControllers)
		{
			layerController.Update(Camera, CurrentWeatherIntensity, WindDirection, WindIntensity);
		}
		foreach (IWeatherEntityController customEffectController in m_CustomEffectControllers)
		{
			customEffectController.Update(Camera, CurrentWeatherIntensity, WindDirection, WindIntensity);
		}
		foreach (WeatherDirectionalLightController directionalLightController in m_DirectionalLightControllers)
		{
			directionalLightController.Update(Camera, CurrentWeatherIntensity, WindDirection, WindIntensity);
		}
	}

	private void UpdateBakedGroundParameters()
	{
		foreach (WeatherLayerController layerController in m_LayerControllers)
		{
			layerController.UpdateBakedGroundParameters();
		}
	}

	private void ResetFog()
	{
		RenderSettings.fog = m_DefaultFogSettings.Enabled;
		RenderSettings.fogMode = m_DefaultFogSettings.FogMode;
		RenderSettings.fogColor = m_DefaultFogSettings.Color;
		RenderSettings.fogStartDistance = m_DefaultFogSettings.StartDistance;
		RenderSettings.fogEndDistance = m_DefaultFogSettings.EndDistance;
	}

	private static bool ValidateLocationWeatherData(IWeatherProfile profile)
	{
		bool hasAreas = VFXTotalLocationWeatherData.HasAreas;
		if (profile.Layers != null && profile.Layers.Where((WeatherLayer layer) => layer != null && layer.Effects != null).SelectMany((WeatherLayer layer) => layer.Effects).Any((WeatherEffect effect) => effect?.UseBakedLocationData ?? false) && !hasAreas)
		{
			return false;
		}
		return true;
	}

	public void OverrideProfile(bool overrideProfile, IWeatherProfile profile)
	{
		m_OverrideProfile = overrideProfile;
		m_ProfileForOverride = profile;
		Reset();
	}

	public void DebugSetWeather(InclemencyType inclemencyType)
	{
		this.OnDebugSetInclemency?.Invoke(inclemencyType);
	}

	public string GetWeatherStats()
	{
		int num = 0;
		int num2 = 0;
		if (m_LayerControllers != null)
		{
			foreach (WeatherLayerController layerController in m_LayerControllers)
			{
				foreach (WeatherEffectController effectController in layerController.EffectControllers)
				{
					foreach (VisualEffect item in effectController.EnumerateCells())
					{
						num++;
						num2 += item.aliveParticleCount;
					}
				}
			}
		}
		return $"Visual Effects Count: {num}\nTotal Particles Count: {num2}";
	}
}
