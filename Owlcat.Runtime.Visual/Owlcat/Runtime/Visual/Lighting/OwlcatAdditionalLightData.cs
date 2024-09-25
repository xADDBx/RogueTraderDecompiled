using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class OwlcatAdditionalLightData : MonoBehaviour, IAdditionalData
{
	private const LightLayerEnum kInvalidLightLayerMask = (LightLayerEnum)(-1);

	private const LightLayerEnum kFallbackDefaultLightLayerMask = LightLayerEnum.LightLayerDefault;

	[Tooltip("Controls the usage of pipeline settings.")]
	[SerializeField]
	private bool m_UsePipelineSettings = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_InnerRadius;

	[Tooltip("Falloff type (realtime and mixed only)")]
	[SerializeField]
	private LightFalloffType m_FalloffType;

	[Tooltip("Snap spacular flare to inner radius")]
	[SerializeField]
	private bool m_SnapSpecularToInnerRadius;

	[SerializeField]
	private LightLayerEnum m_LightLayerMask = (LightLayerEnum)(-1);

	[SerializeField]
	private bool m_VolumetricLighting;

	[SerializeField]
	private float m_VolumetricIntensity = 1f;

	[SerializeField]
	private bool m_VolumetricShadows = true;

	[Tooltip("Controls the size of the cookie mask currently assigned to the light.")]
	[SerializeField]
	private Vector2 m_LightCookieSize = Vector2.one;

	[Tooltip("Controls the offset of the cookie mask currently assigned to the light.")]
	[SerializeField]
	private Vector2 m_LightCookieOffset = Vector2.zero;

	[SerializeField]
	private LightShadowmapResolution m_ShadowmapResolution;

	[SerializeField]
	private ShadowmapUpdateMode m_ShadowmapUpdateMode;

	[SerializeField]
	private bool m_ShadowmapAlwaysDrawDynamicShadowCasters;

	[SerializeField]
	private bool m_ShadowmapUpdateOnLightMovement;

	public bool UsePipelineSettings
	{
		get
		{
			return m_UsePipelineSettings;
		}
		set
		{
			m_UsePipelineSettings = value;
		}
	}

	public float InnerRadius
	{
		get
		{
			return m_InnerRadius;
		}
		set
		{
			m_InnerRadius = value;
		}
	}

	public LightFalloffType FalloffType
	{
		get
		{
			return m_FalloffType;
		}
		set
		{
			m_FalloffType = value;
		}
	}

	public bool SnapSperularToInnerRadius
	{
		get
		{
			return m_SnapSpecularToInnerRadius;
		}
		set
		{
			m_SnapSpecularToInnerRadius = value;
		}
	}

	public LightLayerEnum LightLayerMask
	{
		get
		{
			if (m_LightLayerMask == (LightLayerEnum)(-1))
			{
				m_LightLayerMask = GetDefaultLightLayerMask();
			}
			return m_LightLayerMask;
		}
		set
		{
			m_LightLayerMask = value;
		}
	}

	public bool VolumetricLighting
	{
		get
		{
			return m_VolumetricLighting;
		}
		set
		{
			m_VolumetricLighting = value;
		}
	}

	public float VolumetricIntensity
	{
		get
		{
			return m_VolumetricIntensity;
		}
		set
		{
			m_VolumetricIntensity = value;
		}
	}

	public bool VolumetricShadows
	{
		get
		{
			return m_VolumetricShadows;
		}
		set
		{
			m_VolumetricShadows = value;
		}
	}

	public Vector2 LightCookieSize
	{
		get
		{
			return m_LightCookieSize;
		}
		set
		{
			m_LightCookieSize = value;
		}
	}

	public Vector2 LightCookieOffset
	{
		get
		{
			return m_LightCookieOffset;
		}
		set
		{
			m_LightCookieOffset = value;
		}
	}

	public LightShadowmapResolution ShadowmapResolution
	{
		get
		{
			return m_ShadowmapResolution;
		}
		set
		{
			m_ShadowmapResolution = value;
		}
	}

	public ShadowmapUpdateMode ShadowmapUpdateMode
	{
		get
		{
			return ShadowmapUpdateMode.Cached;
		}
		set
		{
			m_ShadowmapUpdateMode = value;
		}
	}

	public bool ShadowmapAlwaysDrawDynamicShadowCasters
	{
		get
		{
			return true;
		}
		set
		{
			m_ShadowmapAlwaysDrawDynamicShadowCasters = value;
		}
	}

	public bool ShadowmapUpdateOnLightMovement
	{
		get
		{
			return true;
		}
		set
		{
			m_ShadowmapUpdateOnLightMovement = value;
		}
	}

	private static LightLayerEnum GetDefaultLightLayerMask()
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if (asset != null)
		{
			return asset.DefaultLightLayerMask;
		}
		return LightLayerEnum.LightLayerDefault;
	}
}
