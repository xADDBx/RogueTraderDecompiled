using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Controllers;

public class RealtimeLightsController : IControllerEnable, IController, IControllerDisable, IControllerTick, IAreaHandler, ISubscriber, ITimeOfDayChangedHandler
{
	public class LightData
	{
		public Light Light;

		public float DefaultIntensity;

		public LightShadows DefaultShadows;

		public int Importance;

		public float Priority;

		public bool IsActive;

		public AnimatedLight AnimatedLight;

		public string Name;

		public float Intensity
		{
			get
			{
				if (!AnimatedLight)
				{
					if (!Light)
					{
						return 0f;
					}
					return Light.intensity;
				}
				return AnimatedLight.IntensityMultiplier;
			}
			set
			{
				if ((bool)AnimatedLight)
				{
					AnimatedLight.IntensityMultiplier = value;
				}
				else if ((bool)Light)
				{
					Light.intensity = value;
				}
			}
		}
	}

	private readonly List<LightData> m_ShadowLights = new List<LightData>();

	private readonly List<LightData> m_Lights = new List<LightData>();

	private readonly List<LightData> m_ActiveLights = new List<LightData>();

	private bool m_HasZoneLights;

	private const float ScreenDistanceApprox = 20f;

	private const float FadeTime = 0.5f;

	private static readonly int[] Quality = new int[3] { 1, 3, 5 };

	private float m_LastMinimumPriority;

	public int LightSpellsCount { get; set; }

	public void RegisterPartyLight(Light l, int importance)
	{
		AnimatedLight component = l.GetComponent<AnimatedLight>();
		LightData item = new LightData
		{
			Light = l,
			AnimatedLight = component,
			DefaultIntensity = (component ? component.IntensityMultiplier : l.intensity),
			DefaultShadows = l.shadows,
			Importance = importance,
			Name = l.name
		};
		m_Lights.Add(item);
		if ((bool)component)
		{
			m_ShadowLights.Insert(0, item);
		}
		else
		{
			m_ShadowLights.Add(item);
		}
	}

	public void UnregisterPartyLight(Light light)
	{
		int num = m_Lights.FindIndex((LightData l) => l.Light == light);
		if (num >= 0)
		{
			LightData lightData = m_Lights[num];
			if ((bool)lightData.AnimatedLight)
			{
				lightData.Intensity = lightData.DefaultIntensity;
				lightData.Light.enabled = true;
			}
			m_Lights[num] = m_Lights[m_Lights.Count - 1];
			m_Lights.RemoveAt(m_Lights.Count - 1);
			m_ShadowLights.Remove(lightData);
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		UpdateActiveLights();
		for (int i = 0; i < m_Lights.Count; i++)
		{
			LightData lightData = m_Lights[i];
			if (!lightData.Light)
			{
				PFLog.Default.Error("Light is destroyed in RealtimeLightController: " + lightData.Name);
				continue;
			}
			lightData.Intensity = Mathf.MoveTowards(lightData.Intensity, lightData.IsActive ? lightData.DefaultIntensity : 0f, Time.unscaledDeltaTime * lightData.DefaultIntensity / 0.5f);
			if (!lightData.IsActive && lightData.Intensity < 0.001f)
			{
				lightData.Light.enabled = false;
			}
			if (lightData.IsActive && lightData.Intensity > 0f)
			{
				lightData.Light.enabled = true;
			}
		}
		bool flag = true;
		for (int j = 0; j < m_ShadowLights.Count; j++)
		{
			bool flag2 = m_ShadowLights[j].IsActive && flag && m_ShadowLights[j].DefaultShadows != LightShadows.None;
			m_ShadowLights[j].Light.shadows = (flag2 ? m_ShadowLights[j].DefaultShadows : LightShadows.None);
			if (flag2)
			{
				flag = false;
			}
		}
	}

	private void UpdateActiveLights()
	{
		CameraRig instance = CameraRig.Instance;
		if (!instance)
		{
			return;
		}
		Vector3 position = instance.transform.position;
		for (int i = 0; i < m_ActiveLights.Count; i++)
		{
			m_ActiveLights[i].IsActive = false;
		}
		m_ActiveLights.Clear();
		m_LastMinimumPriority = 0f;
		for (int j = 0; j < m_Lights.Count; j++)
		{
			LightData lightData = m_Lights[j];
			lightData.Priority = GetLightPriority(position, lightData);
			if (lightData.Priority > m_LastMinimumPriority)
			{
				MaybeAddToActiveLights(lightData);
			}
		}
		for (int k = 0; k < m_ActiveLights.Count; k++)
		{
			m_ActiveLights[k].IsActive = true;
		}
	}

	private void MaybeAddToActiveLights(LightData lightData)
	{
		bool flag = false;
		int num = Quality[1] - LightSpellsCount;
		for (int i = 0; i < m_ActiveLights.Count; i++)
		{
			if (m_ActiveLights[i].Priority < lightData.Priority)
			{
				m_ActiveLights.Insert(i, lightData);
				flag = true;
				break;
			}
		}
		if (m_ActiveLights.Count > num)
		{
			m_ActiveLights.RemoveAt(num);
			m_LastMinimumPriority = m_ActiveLights[num - 1].Priority;
		}
		else if (m_ActiveLights.Count < num && !flag)
		{
			m_ActiveLights.Add(lightData);
		}
	}

	private float GetLightPriority(Vector3 camPos, LightData lightData)
	{
		if (!lightData.Light)
		{
			return 0f;
		}
		float num = Vector2.Distance(camPos.To2D(), lightData.Light.transform.position.To2D());
		if (num > 20f + lightData.Light.range)
		{
			return 0f;
		}
		if (num > 20f + lightData.Light.range / 3f)
		{
			return 0.1f;
		}
		return ((1f - num / (20f + lightData.Light.range / 3f)) * 0.9f + 0.1f) * (float)(lightData.Importance + 1);
	}

	public void OnEnable()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		if (!m_HasZoneLights)
		{
			UpdateEnvironmentLights();
		}
	}

	private void UpdateEnvironmentLights()
	{
		m_Lights.RemoveAll((LightData l) => !l.Light);
		IEnumerable<Light> source = from l in Object.FindObjectsOfType<Light>()
			where l.gameObject.scene.name != "BaseMechanics" && l.type == LightType.Point && m_Lights.All((LightData d) => d.Light != l) && (l.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed || l.bakingOutput.lightmapBakeType == LightmapBakeType.Realtime)
			select l;
		m_Lights.AddRange(source.Select((Light l) => new LightData
		{
			Light = l,
			DefaultIntensity = l.intensity,
			Importance = 0,
			Name = l.name
		}));
		m_HasZoneLights = true;
	}

	public void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
	}

	private void OnActiveSceneChanged(Scene arg0, Scene arg1)
	{
		UpdateEnvironmentLights();
	}

	public void OnAreaBeginUnloading()
	{
		m_HasZoneLights = false;
	}

	public void OnAreaDidLoad()
	{
		UpdateEnvironmentLights();
	}

	public void OnTimeOfDayChanged()
	{
		UpdateEnvironmentLights();
	}
}
