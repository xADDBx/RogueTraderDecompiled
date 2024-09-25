using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;

[Serializable]
public class MaterialParametersOverrideSettings
{
	public OverrideMode BumpOverride;

	public OverrideMode SpecularOverride;

	[Range(0f, 1f)]
	public float Roughness;

	public bool MetallicOverride;

	[Range(0f, 1f)]
	public float Metallic;

	public OverrideMode EmissionOverride;

	[Range(0f, 10f)]
	public float Emission;

	[Header("Textures")]
	public Texture2D AlbedoMap;

	public Texture2D Masks;

	public Texture2D BumpMap;

	[Header("Tiling")]
	public TilingType TilingType;

	public Vector2 TilingMultiplier;

	[NonSerialized]
	public bool IsActivated;

	[NonSerialized]
	public bool IsDisabled;

	public void Reset()
	{
		IsActivated = false;
		IsDisabled = false;
	}
}
