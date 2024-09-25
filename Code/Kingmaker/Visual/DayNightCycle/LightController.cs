using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.Root;
using Kingmaker.QA;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.View;
using Kingmaker.Visual.Particles.ForcedCulling;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.SceneHelpers;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Kingmaker.Visual.DayNightCycle;

[ExecuteInEditMode]
public class LightController : RegisteredBehaviour
{
	[Serializable]
	private class SphericalHarmonics
	{
		[SerializeField]
		public float[] Coefficients = new float[27];
	}

	[Serializable]
	private class CameraContourLightSettings
	{
		public float Intensity = 4f;

		public Color LightColor = new Color(0.495283f, 0.7442303f, 1f, 1f);
	}

	[Serializable]
	public class LocalLightSettings
	{
		public Light Light;

		public LightConfig MorningConfig;

		public LightConfig DayConfig;

		public LightConfig EveningConfig;

		public LightConfig NightConfig;
	}

	[Serializable]
	public class LocalObjectsSettings
	{
		public GameObject Obj;

		public ObjectConfig MorningConfig;

		public ObjectConfig DayConfig;

		public ObjectConfig EveningConfig;

		public ObjectConfig NightConfig;
	}

	[Serializable]
	public class LightConfig
	{
		public float intensity;

		public Color color = Color.black;

		public bool enabled;
	}

	[Serializable]
	public class ObjectConfig
	{
		public bool enabled;
	}

	public enum CameraLightInitStatus
	{
		notinit,
		enabled,
		disabled
	}

	private class BakingProbe
	{
		private int m_Id;

		private ReflectionProbe m_Probe;

		public bool IsBaked
		{
			get
			{
				if (!(m_Probe == null) && !(m_Probe.gameObject == null))
				{
					return m_Probe.IsFinishedRendering(m_Id);
				}
				return true;
			}
		}

		public BakingProbe(ReflectionProbe probe)
		{
			m_Probe = probe;
			m_Id = m_Probe.RenderProbe();
		}
	}

	[NonSerialized]
	public bool EditorUpdate;

	[HideInInspector]
	public TimeOfDay EditorTimeOfDay;

	[HideInInspector]
	public List<Light> LightsForEdit;

	private List<StaticPrefab> StaticPrefabs;

	[HideInInspector]
	public List<ShadowProxyCombinerBox> ShadowProxyBoxes = new List<ShadowProxyCombinerBox>();

	[Space(10f)]
	[Header("Camera Settings")]
	public CameraLightInitStatus CameraLight;

	public CameraClearFlags CameraClearFlag = CameraClearFlags.Color;

	[Tooltip("Состояние контурного лайта для персонажей. По дефолту должен быть всегда включен, это системная фича, которая должна работать на всех сценах, но могут быть исключения")]
	public bool CameraContourLightEnabled = true;

	[SerializeField]
	[Tooltip("Настройки дирекшенал лайта, что висит на камере. Нужен для контурного освещения персонажей и выделения их из окружения. Дефолтные настройки подобраны достаточно универсально, но при необходимости их можно менять здесь")]
	private CameraContourLightSettings ContourLightSettings = new CameraContourLightSettings();

	[Space(10f)]
	[Header("Base Settings")]
	public Light MainLight;

	[SerializeField]
	public SceneLightConfig m_SoloConfig;

	[SerializeField]
	private SceneLightConfig m_MorningConfig;

	[SerializeField]
	private SceneLightConfig m_DayConfig;

	[SerializeField]
	private SceneLightConfig m_EveningConfig;

	[SerializeField]
	private SceneLightConfig m_NightConfig;

	[Space(10f)]
	[Header("Reflection Probes")]
	[SerializeField]
	private List<ReflectionProbe> m_ReflectionProbes;

	[Space(10f)]
	[Header("Post Processing")]
	[SerializeField]
	private List<Volume> m_SoloPostProcessingVolumes;

	[SerializeField]
	private List<Volume> m_MorningPostProcessingVolumes;

	[SerializeField]
	private List<Volume> m_DayPostProcessingVolumes;

	[SerializeField]
	private List<Volume> m_EveningPostProcessingVolumes;

	[SerializeField]
	private List<Volume> m_NightPostProcessingVolumes;

	[Space(10f)]
	[SerializeField]
	private List<LocalLightSettings> m_LocalLights;

	[SerializeField]
	public List<LocalObjectsSettings> m_LocalObjects;

	private Dictionary<VisualStateEffectType, Volume> m_PostProcessingEffectVolumesMap = new Dictionary<VisualStateEffectType, Volume>();

	private LogChannel m_LogChannelTechArt;

	private SceneLightConfig m_OverrideConfig;

	private TimeOfDay m_CurrentTimeOfDay;

	private List<BakingProbe> m_BakingProbes = new List<BakingProbe>();

	public static LightController Active => ObjectRegistry<LightController>.Instance.SingleOrDefault((LightController c) => c.gameObject.scene == SceneManager.GetActiveScene());

	public List<ReflectionProbe> ReflectionProbes => m_ReflectionProbes;

	public List<LocalLightSettings> LocalLights => m_LocalLights;

	public bool IsProbeBaking => m_BakingProbes.Count > 0;

	public bool TryGetPostProcessingEffect(VisualStateEffectType type, out Volume effect)
	{
		effect = null;
		if (m_PostProcessingEffectVolumesMap.TryGetValue(type, out var value))
		{
			effect = value;
			return true;
		}
		return false;
	}

	private static void SetArCombatGridOverrides(SceneLightConfig lightConfig)
	{
		CombatHudSurfaceRenderer combatHudSurfaceRenderer = UnityEngine.Object.FindObjectOfType<CombatHudSurfaceRenderer>();
		if (combatHudSurfaceRenderer == null)
		{
			return;
		}
		if (lightConfig.ArCombatGridOverrideMaterials == null || lightConfig.ArCombatGridOverrideMaterials.Length < 1)
		{
			combatHudSurfaceRenderer.SetOverrideMaterials(null);
			return;
		}
		Material[] arCombatGridOverrideMaterials = lightConfig.ArCombatGridOverrideMaterials;
		for (int i = 0; i < arCombatGridOverrideMaterials.Length; i++)
		{
			if (arCombatGridOverrideMaterials[i] == null)
			{
				combatHudSurfaceRenderer.SetOverrideMaterials(null);
				return;
			}
		}
		combatHudSurfaceRenderer.SetOverrideMaterials(lightConfig.ArCombatGridOverrideMaterials);
	}

	public void OverrideConfig([CanBeNull] SceneLightConfig config)
	{
		if (m_OverrideConfig != null && config != null)
		{
			PFLog.TechArt.ErrorWithReport("SceneLightConfig already overridden");
		}
		m_OverrideConfig = config;
		ChangeDayTime(m_CurrentTimeOfDay);
	}

	[CanBeNull]
	private SceneLightConfig SelectConfig(TimeOfDay time)
	{
		SceneLightConfig sceneLightConfig = m_OverrideConfig.Or(null);
		if ((object)sceneLightConfig == null)
		{
			SceneLightConfig sceneLightConfig2 = m_SoloConfig.Or(null);
			if ((object)sceneLightConfig2 == null)
			{
				sceneLightConfig2 = time switch
				{
					TimeOfDay.Morning => m_MorningConfig, 
					TimeOfDay.Day => m_DayConfig, 
					TimeOfDay.Evening => m_EveningConfig, 
					TimeOfDay.Night => m_NightConfig, 
					_ => m_DayConfig, 
				};
			}
			sceneLightConfig = sceneLightConfig2;
		}
		return sceneLightConfig;
	}

	public void ChangeDayTime(TimeOfDay time)
	{
		m_CurrentTimeOfDay = time;
		if (SceneManager.GetActiveScene() != base.gameObject.scene)
		{
			LogChannel.TechArt.Error("Current scene isn't active. Set LightController scene as active scene for lighting purposes : " + base.gameObject.scene.name);
			SceneManager.SetActiveScene(base.gameObject.scene);
		}
		SceneLightConfig sceneLightConfig = SelectConfig(time);
		if (sceneLightConfig == null)
		{
			LogChannel.TechArt.Error("Missing Light Config in scene : " + base.gameObject.scene.name);
			EditorUpdate = false;
			return;
		}
		SetArCombatGridOverrides(sceneLightConfig);
		if (sceneLightConfig == m_SoloConfig)
		{
			ApplyConfig(sceneLightConfig);
		}
		else
		{
			ApplyConfig(sceneLightConfig, time);
		}
	}

	private void Update()
	{
		if (m_BakingProbes.Count <= 0)
		{
			return;
		}
		for (int num = m_BakingProbes.Count - 1; num >= 0; num--)
		{
			if (m_BakingProbes[num].IsBaked)
			{
				m_BakingProbes.RemoveAt(num);
			}
		}
	}

	protected override void OnEnabled()
	{
	}

	protected override void OnDisabled()
	{
	}

	private void Awake()
	{
		if (CameraLight == CameraLightInitStatus.notinit)
		{
			InitCameraDefaultLightStatusForScene();
		}
		SetLayerInPpVolumes();
		if (Application.isPlaying)
		{
			InitializeEffectsMap();
		}
	}

	private void SetLayerInPpVolumes()
	{
		if (m_SoloPostProcessingVolumes.Count == 0)
		{
			return;
		}
		foreach (Volume soloPostProcessingVolume in m_SoloPostProcessingVolumes)
		{
			if ((bool)soloPostProcessingVolume)
			{
				soloPostProcessingVolume.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			}
		}
	}

	private void InitializeEffectsMap()
	{
		foreach (KeyValuePair<VisualStateEffectType, VolumeProfile> getEffectProfile in BlueprintRoot.Instance.WarhammerRoot.PostProcessingEffectsLibrary.GetEffectProfiles)
		{
			Volume volume = new GameObject(getEffectProfile.Key.ToString()).AddComponent<Volume>();
			volume.profile = getEffectProfile.Value;
			volume.weight = 0f;
			volume.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			volume.gameObject.transform.SetParent(base.gameObject.transform);
			m_PostProcessingEffectVolumesMap[getEffectProfile.Key] = volume;
		}
	}

	private void ApplyConfig(SceneLightConfig config, TimeOfDay time)
	{
		ApplySceneRendreringSettings(config);
		ApplyPostProcesses(time);
		ApplyLocalLights(time);
		ApplyLocalObjects(time);
		BakeReflectionProbes();
		SetupCamera(config);
	}

	private void ApplyConfig(SceneLightConfig config)
	{
		ApplySceneRendreringSettings(config);
		ApplyPostProcesses();
		SetupCamera(config);
	}

	public void SwitchShadowsInRendererWithProxy(ShadowCastingMode castingMode = ShadowCastingMode.Off)
	{
		StaticPrefab[] array = UnityEngine.Object.FindObjectsOfType<StaticPrefab>();
		foreach (StaticPrefab staticPrefab in array)
		{
			if (staticPrefab.ShadowProxies.Count < 1)
			{
				continue;
			}
			bool flag = false;
			foreach (ShadowProxy shadowProxy in staticPrefab.ShadowProxies)
			{
				if (shadowProxy == null)
				{
					UberDebug.LogError(staticPrefab, "Null shadow proxy in object.");
					flag = true;
				}
			}
			if (!flag)
			{
				Renderer[] componentsInChildren = staticPrefab.VisualRoot.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].shadowCastingMode = castingMode;
				}
			}
		}
	}

	public void ApplySceneRendreringSettings(SceneLightConfig config)
	{
		if ((bool)MainLight)
		{
			MainLight.color = config.MainLightColor;
			MainLight.intensity = config.MainLightIntensity;
			MainLight.bounceIntensity = config.MainLightIndirectIntensity;
			MainLight.shadowStrength = config.MainLightShadowStrength;
			MainLight.transform.rotation = Quaternion.Euler(config.MainLightRotation);
		}
		RenderSettings.ambientMode = AmbientMode.Trilight;
		RenderSettings.ambientSkyColor = config.SkyAmbientColor;
		RenderSettings.ambientEquatorColor = config.EquatorAmbientColor;
		RenderSettings.ambientGroundColor = config.GroundAmbientColor;
		if (RenderSettings.skybox != null)
		{
			if ((bool)config.SkyboxMaterial)
			{
				RenderSettings.skybox = config.SkyboxMaterial;
			}
			RenderSettings.skybox.SetColor(ShaderProps._Tint, config.SkyboxColor);
			RenderSettings.skybox.SetFloat(ShaderProps._Exposure, config.SkyboxExposure);
			RenderSettings.skybox.SetFloat(ShaderProps._Rotation, config.SkyboxRotation);
		}
		else
		{
			LogChannel.TechArt.Error("Missing Skybox in " + SceneManager.GetActiveScene().name + " scene render settings!");
		}
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogColor = config.FogColor;
		RenderSettings.fogStartDistance = config.FogStartDistance;
		RenderSettings.fogEndDistance = config.FogEndDistance;
		if (!Application.isPlaying)
		{
			LightProbes.Tetrahedralize();
		}
	}

	private void SetupCamera(SceneLightConfig config)
	{
		CameraRig cameraRig = null;
		if (Game.Instance != null && CameraRig.Instance != null)
		{
			cameraRig = CameraRig.Instance;
		}
		if (!(cameraRig == null))
		{
			cameraRig.Camera.clearFlags = CameraClearFlag;
			cameraRig.CameraLight.enabled = GetCameraLightStatus();
			cameraRig.CharacterContourLight.enabled = CameraContourLightEnabled;
			cameraRig.CharacterContourLight.color = ContourLightSettings.LightColor;
			cameraRig.CharacterContourLight.intensity = ContourLightSettings.Intensity;
		}
	}

	private bool GetCameraLightStatus()
	{
		if (CameraLight == CameraLightInitStatus.disabled)
		{
			return false;
		}
		if (CameraLight == CameraLightInitStatus.enabled)
		{
			return true;
		}
		return InitCameraDefaultLightStatusForScene();
	}

	private bool InitCameraDefaultLightStatusForScene()
	{
		if (MainLight == null)
		{
			CameraLight = CameraLightInitStatus.enabled;
			return true;
		}
		CameraLight = CameraLightInitStatus.disabled;
		return false;
	}

	private void ApplyLocalLights(TimeOfDay time)
	{
		if (m_LocalLights.Count <= 0)
		{
			return;
		}
		foreach (LocalLightSettings localLight in m_LocalLights)
		{
			if (localLight == null)
			{
				LogChannel.TechArt.Error("Local light config is missing in scene : " + base.gameObject.scene.name);
				EditorUpdate = false;
				continue;
			}
			switch (time)
			{
			case TimeOfDay.Morning:
				ApplyLightConfig(localLight.Light, localLight.MorningConfig);
				break;
			case TimeOfDay.Day:
				ApplyLightConfig(localLight.Light, localLight.DayConfig);
				break;
			case TimeOfDay.Evening:
				ApplyLightConfig(localLight.Light, localLight.EveningConfig);
				break;
			case TimeOfDay.Night:
				ApplyLightConfig(localLight.Light, localLight.NightConfig);
				break;
			}
		}
	}

	private void Start()
	{
		SetLayerInPpVolumes();
	}

	public void CombineShadowProxyMeshes()
	{
		ShadowProxyCombinerBox[] array = UnityEngine.Object.FindObjectsOfType<ShadowProxyCombinerBox>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].BakeShadowProxies();
		}
	}

	private void ApplyLocalObjects(TimeOfDay time)
	{
		if (m_LocalObjects.Count <= 0)
		{
			return;
		}
		foreach (LocalObjectsSettings localObject in m_LocalObjects)
		{
			if (localObject == null)
			{
				LogChannel.TechArt.Error("Local objects config is missing in scene : " + base.gameObject.scene.name);
				EditorUpdate = false;
				continue;
			}
			switch (time)
			{
			case TimeOfDay.Morning:
				ApplyObjectConfig(localObject.Obj, localObject.MorningConfig);
				break;
			case TimeOfDay.Day:
				ApplyObjectConfig(localObject.Obj, localObject.DayConfig);
				break;
			case TimeOfDay.Evening:
				ApplyObjectConfig(localObject.Obj, localObject.EveningConfig);
				break;
			case TimeOfDay.Night:
				ApplyObjectConfig(localObject.Obj, localObject.NightConfig);
				break;
			}
		}
	}

	private void ApplyLightConfig(Light light, LightConfig config)
	{
		if (!light || config == null)
		{
			LogChannel.TechArt.Error("Missing light object in light config in scene :" + base.gameObject.scene.name);
			EditorUpdate = false;
			return;
		}
		light.intensity = config.intensity;
		light.color = config.color;
		light.enabled = config.enabled;
		light.GetComponentInParent<ForcedCullingRadius>().Or(null)?.SetLightEnabledByDefault(light);
	}

	private void ApplyObjectConfig(GameObject obj, ObjectConfig config)
	{
		if (!obj || config == null)
		{
			LogChannel.TechArt.Error("Missing object in object config in scene :" + base.gameObject.scene.name);
			EditorUpdate = false;
		}
		else if (obj.GetComponent<Light>() != null)
		{
			obj.GetComponent<Light>().enabled = config.enabled;
		}
		else
		{
			obj.SetActive(config.enabled);
		}
	}

	public void DisableAllPostProcessVolumes()
	{
		SwitchPpVolumes(m_SoloPostProcessingVolumes, state: false);
		SwitchPpVolumes(m_MorningPostProcessingVolumes, state: false);
		SwitchPpVolumes(m_DayPostProcessingVolumes, state: false);
		SwitchPpVolumes(m_EveningPostProcessingVolumes, state: false);
		SwitchPpVolumes(m_NightPostProcessingVolumes, state: false);
	}

	private void SwitchPpVolumes(List<Volume> volumes, bool state = true)
	{
		if (volumes.Count <= 0)
		{
			return;
		}
		foreach (Volume volume in volumes)
		{
			if (!(volume == null))
			{
				volume.enabled = state;
			}
		}
	}

	private void ApplyPostProcesses(TimeOfDay time = TimeOfDay.Day)
	{
		DisableAllPostProcessVolumes();
		if ((bool)m_SoloConfig)
		{
			SwitchPpVolumes(m_SoloPostProcessingVolumes);
			return;
		}
		switch (time)
		{
		case TimeOfDay.Morning:
			SwitchPpVolumes(m_MorningPostProcessingVolumes);
			break;
		case TimeOfDay.Day:
			SwitchPpVolumes(m_DayPostProcessingVolumes);
			break;
		case TimeOfDay.Evening:
			SwitchPpVolumes(m_EveningPostProcessingVolumes);
			break;
		case TimeOfDay.Night:
			SwitchPpVolumes(m_NightPostProcessingVolumes);
			break;
		}
	}

	private void BakeReflectionProbes()
	{
		if (m_ReflectionProbes.Count <= 0)
		{
			return;
		}
		if (m_BakingProbes.Count > 0)
		{
			m_BakingProbes.Clear();
		}
		foreach (ReflectionProbe reflectionProbe in m_ReflectionProbes)
		{
			if (!(reflectionProbe == null))
			{
				m_BakingProbes.Add(new BakingProbe(reflectionProbe));
			}
		}
	}

	public void AssignReflectionProbes()
	{
		m_ReflectionProbes = UnityEngine.Object.FindObjectsOfType<ReflectionProbe>().ToList();
		foreach (ReflectionProbe reflectionProbe in m_ReflectionProbes)
		{
			reflectionProbe.farClipPlane = ((reflectionProbe.farClipPlane > 50f) ? 50f : reflectionProbe.farClipPlane);
			reflectionProbe.shadowDistance = ((reflectionProbe.shadowDistance > 50f) ? 50f : reflectionProbe.shadowDistance);
		}
	}

	public void ClearReflectionProbes()
	{
		m_ReflectionProbes.Clear();
	}

	public void CollectLocalLights()
	{
	}

	public void CollectLocalObjects()
	{
	}

	public void CollectLocalLightsProperties(TimeOfDay timeOfDay)
	{
	}

	public void CollectLocalObjectsProperties(TimeOfDay timeOfDay)
	{
	}

	private void SetLocalLightParameters(LocalLightSettings lightSettings, Light light, TimeOfDay timeOfDay)
	{
		switch (timeOfDay)
		{
		case TimeOfDay.Morning:
			lightSettings.MorningConfig.color = light.color;
			lightSettings.MorningConfig.enabled = light.enabled;
			lightSettings.MorningConfig.intensity = light.intensity;
			break;
		case TimeOfDay.Day:
			lightSettings.DayConfig.color = light.color;
			lightSettings.DayConfig.enabled = light.enabled;
			lightSettings.DayConfig.intensity = light.intensity;
			break;
		case TimeOfDay.Evening:
			lightSettings.EveningConfig.color = light.color;
			lightSettings.EveningConfig.enabled = light.enabled;
			lightSettings.EveningConfig.intensity = light.intensity;
			break;
		case TimeOfDay.Night:
			lightSettings.NightConfig.color = light.color;
			lightSettings.NightConfig.enabled = light.enabled;
			lightSettings.NightConfig.intensity = light.intensity;
			break;
		}
		BakeReflectionProbes();
	}

	private void SetLocalObjParameters(LocalObjectsSettings objSettings, GameObject obj, TimeOfDay timeOfDay)
	{
		switch (timeOfDay)
		{
		case TimeOfDay.Morning:
			objSettings.MorningConfig.enabled = obj.activeSelf;
			break;
		case TimeOfDay.Day:
			objSettings.DayConfig.enabled = obj.activeSelf;
			break;
		case TimeOfDay.Evening:
			objSettings.EveningConfig.enabled = obj.activeSelf;
			break;
		case TimeOfDay.Night:
			objSettings.NightConfig.enabled = obj.activeSelf;
			break;
		}
	}

	public void UpdateLocalLightParameters()
	{
		foreach (LocalLightSettings localLight in m_LocalLights)
		{
			if (localLight == null)
			{
				break;
			}
			SetLocalLightParameters(localLight, localLight.Light, EditorTimeOfDay);
		}
	}

	public void UpdateLocalObjParameters()
	{
		foreach (LocalObjectsSettings localObject in m_LocalObjects)
		{
			if (localObject == null)
			{
				break;
			}
			SetLocalObjParameters(localObject, localObject.Obj, EditorTimeOfDay);
		}
	}

	public void CollectLightsForEdit()
	{
		LightsForEdit = UnityEngine.Object.FindObjectsOfType<Light>().ToList();
	}

	public void CollectEnabledLightsForEdit()
	{
		LightsForEdit = (from a in UnityEngine.Object.FindObjectsOfType<Light>()
			where a.enabled
			select a).ToList();
	}

	public void ClearLightsForEdit()
	{
		LightsForEdit.Clear();
	}

	public void CollectStaticPrefabs()
	{
		StaticPrefabs = UnityEngine.Object.FindObjectsOfType<StaticPrefab>().ToList();
	}

	public void ClearStaticPrefabs()
	{
		StaticPrefabs.Clear();
	}

	private void ShowShadowProxiesGizmo()
	{
		if (StaticPrefabs == null || StaticPrefabs.Count < 1)
		{
			return;
		}
		foreach (StaticPrefab staticPrefab in StaticPrefabs)
		{
			if (!(staticPrefab.VisualRoot == null) && IsAnyRendererWithShadows(staticPrefab))
			{
				if (staticPrefab.ShadowProxies.Count < 1)
				{
					Gizmos.DrawIcon(staticPrefab.transform.position, "MountIkTarget_Gizmo.png", allowScaling: true);
				}
				if (staticPrefab.ShadowProxies.Count > 0)
				{
					Gizmos.DrawIcon(staticPrefab.transform.position, "bone.png", allowScaling: true);
				}
			}
		}
	}

	private bool IsAnyRendererWithShadows(StaticPrefab prefab)
	{
		bool result = false;
		Renderer[] componentsInChildren = prefab.VisualRoot.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].shadowCastingMode != 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		ShowShadowProxiesGizmo();
	}
}
