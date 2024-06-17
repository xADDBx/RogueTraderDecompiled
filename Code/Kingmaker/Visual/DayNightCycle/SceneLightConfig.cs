using System;
using Kingmaker.ResourceLinks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.DayNightCycle;

[CreateAssetMenu(fileName = "SceneLightConfig", menuName = "Techart/Scene Light Config")]
public class SceneLightConfig : ScriptableObject
{
	[Serializable]
	public class Link : WeakResourceLink<SceneLightConfig>
	{
	}

	[Header("Main Light")]
	public Vector3 MainLightRotation = new Vector3(-170.962f, -88.48801f, 80.463f);

	public Color MainLightColor = Color.white;

	public float MainLightIntensity = 1f;

	public float MainLightIndirectIntensity = 1f;

	[Range(0f, 1f)]
	public float MainLightShadowStrength = 0.7f;

	[Header("Ambient Colors")]
	public Color SkyAmbientColor = Color.blue;

	public Color EquatorAmbientColor = Color.gray;

	public Color GroundAmbientColor = Color.black;

	[Header("Skybox")]
	public Material SkyboxMaterial;

	public Color SkyboxColor = Color.gray;

	[HideInInspector]
	public Color SkyboxSkyTint = Color.blue;

	[HideInInspector]
	public Color SkyboxGround = Color.gray;

	public float SkyboxExposure = 1f;

	[Range(0f, 360f)]
	public float SkyboxRotation;

	[Header("Fog")]
	public Color FogColor = Color.gray;

	public float FogStartDistance = 25f;

	public float FogEndDistance = 65f;

	[Header("Post Processing")]
	public VolumeProfile PpProfile;

	[Space(10f)]
	[Header("AR Combat Grid Visual Overrides")]
	[Tooltip("Оверрайд материалы для комбатной сетки. Нужно, чтобы чинить ситуации когда сетку не видно из за особенностей арта конкретной зоны")]
	public Material[] ArCombatGridOverrideMaterials;
}
