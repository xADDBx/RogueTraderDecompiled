using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

[Serializable]
public struct LocalVolumetricFogArtistParameters
{
	internal const float kMinFogDistance = 0.05f;

	[ColorUsage(false)]
	public Color Albedo;

	public float MeanFreePath;

	public float Anisotropy;

	public Texture VolumeMask;

	public Vector3 TextureScrollingSpeed;

	public Vector3 TextureTiling;

	[SerializeField]
	public Vector3 TextureOffset;

	[SerializeField]
	internal float m_EditorUniformFade;

	[SerializeField]
	internal Vector3 m_EditorPositiveFade;

	[SerializeField]
	internal Vector3 m_EditorNegativeFade;

	[SerializeField]
	internal bool m_EditorAdvancedFade;

	public float DistanceFadeStart;

	public float DistanceFadeEnd;

	public Vector3 Size;

	public bool InvertFade;

	public LocalVolumetricFogFalloffMode FalloffMode;

	public Vector3 PositiveFade;

	public Vector3 NegativeFade;

	public LocalVolumetricFogArtistParameters(Color albedo, float meanFreePath, float anisotropy)
	{
		Albedo = albedo;
		MeanFreePath = meanFreePath;
		Anisotropy = anisotropy;
		VolumeMask = null;
		TextureScrollingSpeed = Vector3.zero;
		TextureTiling = Vector3.one;
		TextureOffset = TextureScrollingSpeed;
		Size = Vector3.one;
		PositiveFade = Vector3.zero;
		NegativeFade = Vector3.zero;
		InvertFade = false;
		DistanceFadeStart = 10000f;
		DistanceFadeEnd = 10000f;
		FalloffMode = LocalVolumetricFogFalloffMode.Linear;
		m_EditorPositiveFade = Vector3.zero;
		m_EditorNegativeFade = Vector3.zero;
		m_EditorUniformFade = 0f;
		m_EditorAdvancedFade = false;
	}

	public void Update(float time)
	{
		if (VolumeMask != null)
		{
			TextureOffset = -(TextureScrollingSpeed * time);
		}
	}

	internal void Constrain()
	{
		Albedo.r = Mathf.Clamp01(Albedo.r);
		Albedo.g = Mathf.Clamp01(Albedo.g);
		Albedo.b = Mathf.Clamp01(Albedo.b);
		Albedo.a = 1f;
		MeanFreePath = Mathf.Clamp(MeanFreePath, 0.05f, float.MaxValue);
		Anisotropy = Mathf.Clamp(Anisotropy, -1f, 1f);
		TextureOffset = Vector3.zero;
	}

	public static float ExtinctionFromMeanFreePath(float meanFreePath)
	{
		return 1f / meanFreePath;
	}

	public static Vector3 ScatteringFromExtinctionAndAlbedo(float extinction, Vector3 albedo)
	{
		return extinction * albedo;
	}

	internal LocalVolumetricFogEngineData ConvertToEngineData()
	{
		LocalVolumetricFogEngineData result = default(LocalVolumetricFogEngineData);
		result.extinction = ExtinctionFromMeanFreePath(MeanFreePath);
		result.scattering = ScatteringFromExtinctionAndAlbedo(result.extinction, (Vector4)Albedo);
		RenderTexture atlas = LocalVolumetricFogManager.Instance.VolumeAtlas.GetAtlas();
		result.atlasOffset = LocalVolumetricFogManager.Instance.VolumeAtlas.GetTextureOffset(VolumeMask);
		result.atlasOffset.x /= atlas.width;
		result.atlasOffset.y /= atlas.height;
		result.atlasOffset.z /= atlas.volumeDepth;
		result.useVolumeMask = ((VolumeMask != null) ? 1 : 0);
		float num = ((VolumeMask != null) ? ((float)VolumeMask.width) : 0f);
		result.maskSize = new Vector4(num / (float)atlas.width, num / (float)atlas.height, num / (float)atlas.volumeDepth, num);
		result.textureScroll = TextureOffset;
		result.textureTiling = TextureTiling;
		Vector3 positiveFade = PositiveFade;
		Vector3 negativeFade = NegativeFade;
		result.rcpPosFaceFade.x = Mathf.Min(1f / positiveFade.x, float.MaxValue);
		result.rcpPosFaceFade.y = Mathf.Min(1f / positiveFade.y, float.MaxValue);
		result.rcpPosFaceFade.z = Mathf.Min(1f / positiveFade.z, float.MaxValue);
		result.rcpNegFaceFade.y = Mathf.Min(1f / negativeFade.y, float.MaxValue);
		result.rcpNegFaceFade.x = Mathf.Min(1f / negativeFade.x, float.MaxValue);
		result.rcpNegFaceFade.z = Mathf.Min(1f / negativeFade.z, float.MaxValue);
		result.invertFade = (InvertFade ? 1 : 0);
		result.falloffMode = FalloffMode;
		float num2 = Mathf.Max(DistanceFadeEnd - DistanceFadeStart, 1.526E-05f);
		result.rcpDistFadeLen = 1f / num2;
		result.endTimesRcpDistFadeLen = DistanceFadeEnd * result.rcpDistFadeLen;
		return result;
	}
}
