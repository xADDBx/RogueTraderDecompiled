using System.Collections.Generic;
using System.Linq;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker;

public class ParticleSystemsQualityController
{
	private const string LightingKeyword = "PARTICLES_LIGHTING_ON";

	private const string NoShadowReceiveKeyword = "_RECEIVE_SHADOWS_OFF";

	private static readonly ParticleSystemsQualityController s_Instance = new ParticleSystemsQualityController();

	private bool m_Inited;

	private List<Material> m_MaterialsLighted;

	private List<Material> m_MaterialsShadowReceivers;

	private List<(ParticleSystem, ShadowCastingMode)> m_SystemsShadowCasters;

	public static ParticleSystemsQualityController Instance => s_Instance;

	public void Init()
	{
		if (!m_Inited)
		{
			SettingsRoot.Graphics.ParticleSystemsLightingEnabled.OnValueChanged += UpdateLightedSystems;
			SettingsRoot.Graphics.ParticleSystemsShadowsEnabled.OnValueChanged += UpdateShadowCastersReceivers;
		}
		m_Inited = true;
		List<ParticleSystem> list = FilterParticleSystems(GatherParticleSystems());
		FilterSystemsToCachedLists(in list, out m_MaterialsLighted, shadowCasters: out m_SystemsShadowCasters, shadowReceivers: out m_MaterialsShadowReceivers);
		SettingsEntityBool particleSystemsLightingEnabled = SettingsRoot.Graphics.ParticleSystemsLightingEnabled;
		SettingsEntityBool particleSystemsShadowsEnabled = SettingsRoot.Graphics.ParticleSystemsShadowsEnabled;
		UpdateLightedSystems(particleSystemsLightingEnabled);
		UpdateShadowCastersReceivers(particleSystemsShadowsEnabled);
	}

	private void UpdateShadowCastersReceivers(bool particleSystemsShadowsEnabled)
	{
		if (m_MaterialsShadowReceivers == null || m_SystemsShadowCasters == null)
		{
			return;
		}
		foreach (Material materialsShadowReceiver in m_MaterialsShadowReceivers)
		{
			ChangeShadowReceiving(materialsShadowReceiver, particleSystemsShadowsEnabled);
		}
		foreach (var systemsShadowCaster in m_SystemsShadowCasters)
		{
			ChangeShadowCasting(systemsShadowCaster, particleSystemsShadowsEnabled);
		}
	}

	public void UpdateLightedSystems(bool enableLighting)
	{
		if (m_MaterialsLighted == null)
		{
			return;
		}
		foreach (Material item in m_MaterialsLighted)
		{
			ChangeLightingOnMaterial(item, enableLighting);
		}
	}

	public void Deinit()
	{
		SettingsRoot.Graphics.ParticleSystemsLightingEnabled.OnValueChanged -= UpdateLightedSystems;
		SettingsRoot.Graphics.ParticleSystemsShadowsEnabled.OnValueChanged -= UpdateShadowCastersReceivers;
		m_MaterialsLighted = null;
		m_MaterialsShadowReceivers = null;
		m_SystemsShadowCasters = null;
		m_Inited = false;
	}

	private static ParticleSystem[] GatherParticleSystems()
	{
		return Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	}

	private static List<ParticleSystem> FilterParticleSystems(ParticleSystem[] particleSystems)
	{
		List<ParticleSystem> list = new List<ParticleSystem>();
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (!(particleSystem == null))
			{
				_ = particleSystem.gameObject.scene;
				if (particleSystem.gameObject.scene.isLoaded && !string.IsNullOrEmpty(particleSystem.gameObject.scene.name) && (particleSystem.gameObject.scene.name.Contains("Light") | particleSystem.gameObject.scene.name.Contains("Static")))
				{
					list.Add(particleSystem);
				}
			}
		}
		return list;
	}

	private static void FilterSystemsToCachedLists(in List<ParticleSystem> particleSystems, out List<Material> lighted, out List<Material> shadowReceivers, out List<(ParticleSystem, ShadowCastingMode)> shadowCasters)
	{
		lighted = new List<Material>(particleSystems.Count);
		shadowReceivers = new List<Material>(particleSystems.Count);
		shadowCasters = new List<(ParticleSystem, ShadowCastingMode)>(particleSystems.Count);
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component == null)
			{
				continue;
			}
			if (component.shadowCastingMode != 0)
			{
				shadowCasters.Add((particleSystem, component.shadowCastingMode));
			}
			Material material = component.material;
			if (!(material == null) && material.shaderKeywords.Contains("PARTICLES_LIGHTING_ON"))
			{
				lighted.Add(material);
				if (!material.shaderKeywords.Contains("_RECEIVE_SHADOWS_OFF"))
				{
					shadowReceivers.Add(material);
				}
			}
		}
	}

	private static void ChangeLightingOnMaterial(Material material, bool enableLighting)
	{
		if (!(material == null))
		{
			if (enableLighting)
			{
				material.EnableKeyword("PARTICLES_LIGHTING_ON");
			}
			else
			{
				material.DisableKeyword("PARTICLES_LIGHTING_ON");
			}
		}
	}

	private static void ChangeShadowReceiving(Material material, bool particleSystemsShadowReceivingEnabled)
	{
		if (!(material == null))
		{
			if (particleSystemsShadowReceivingEnabled)
			{
				material.DisableKeyword("_RECEIVE_SHADOWS_OFF");
			}
			else
			{
				material.EnableKeyword("_RECEIVE_SHADOWS_OFF");
			}
		}
	}

	private static void ChangeShadowCasting((ParticleSystem particleSystem, ShadowCastingMode shadowCastingMode) shadowCaster, bool particleSystemsShadowCastingEnabled)
	{
		if (shadowCaster.particleSystem == null)
		{
			return;
		}
		ParticleSystemRenderer component = shadowCaster.particleSystem.GetComponent<ParticleSystemRenderer>();
		if (!(component == null))
		{
			if (particleSystemsShadowCastingEnabled)
			{
				component.shadowCastingMode = shadowCaster.shadowCastingMode;
			}
			else
			{
				component.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}
}
