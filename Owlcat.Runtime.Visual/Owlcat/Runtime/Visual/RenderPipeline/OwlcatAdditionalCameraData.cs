using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[ImageEffectAllowedInSceneView]
public class OwlcatAdditionalCameraData : MonoBehaviour
{
	[SerializeField]
	private CameraRenderType m_CameraType;

	[SerializeField]
	private List<Camera> m_Cameras = new List<Camera>();

	[SerializeField]
	private int m_RendererIndex = -1;

	[SerializeField]
	private LayerMask m_VolumeLayerMask = 1;

	[SerializeField]
	private Transform m_VolumeTrigger;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateModeOption = VolumeFrameworkUpdateMode.UsePipelineSettings;

	[SerializeField]
	private bool m_RenderPostProcessing;

	[SerializeField]
	private AntialiasingMode m_Antialiasing;

	[SerializeField]
	private AntialiasingQuality m_AntialiasingQuality = AntialiasingQuality.High;

	[SerializeField]
	private bool m_Dithering;

	[SerializeField]
	private bool m_ClearDepth = true;

	[SerializeField]
	private bool m_AllowLighting = true;

	[SerializeField]
	private bool m_AllowDecals = true;

	[SerializeField]
	private bool m_AllowDistortion = true;

	[SerializeField]
	private bool m_AllowIndirectRendering = true;

	[SerializeField]
	private bool m_AllowFog = true;

	[SerializeField]
	private bool m_AllowVfxPreparation = true;

	[SerializeField]
	private RenderTexture m_DepthTexture;

	[SerializeField]
	private List<RendererFeatureFlag> m_RendererFeaturesFlags = new List<RendererFeatureFlag>();

	[NonSerialized]
	private Camera m_Camera;

	private VolumeStack m_VolumeStack;

	internal Camera camera
	{
		get
		{
			if (!m_Camera)
			{
				base.gameObject.TryGetComponent<Camera>(out m_Camera);
			}
			return m_Camera;
		}
	}

	public CameraRenderType RenderType
	{
		get
		{
			return m_CameraType;
		}
		set
		{
			m_CameraType = value;
		}
	}

	public List<Camera> cameraStack
	{
		get
		{
			if (RenderType != 0)
			{
				Camera component = base.gameObject.GetComponent<Camera>();
				Debug.LogWarning($"{component.name}: This camera is of {RenderType} type. Only Base cameras can have a camera stack.");
				return null;
			}
			return m_Cameras;
		}
	}

	public bool clearDepth => m_ClearDepth;

	public ScriptableRenderer ScriptableRenderer
	{
		get
		{
			if ((object)OwlcatRenderPipeline.Asset == null)
			{
				return null;
			}
			if (!OwlcatRenderPipeline.Asset.ValidateRendererData(m_RendererIndex))
			{
				int defaultRendererIndex = OwlcatRenderPipeline.Asset.m_DefaultRendererIndex;
				Debug.LogWarning("Renderer at <b>index " + m_RendererIndex + "</b> is missing for camera <b>" + camera.name + "</b>, falling back to Default Renderer. <b>" + OwlcatRenderPipeline.Asset.m_RendererDataList[defaultRendererIndex].name + "</b>", OwlcatRenderPipeline.Asset);
				return OwlcatRenderPipeline.Asset.GetRenderer(defaultRendererIndex);
			}
			return OwlcatRenderPipeline.Asset.GetRenderer(m_RendererIndex);
		}
	}

	public LayerMask VolumeLayerMask
	{
		get
		{
			return m_VolumeLayerMask;
		}
		set
		{
			m_VolumeLayerMask = value;
		}
	}

	public Transform VolumeTrigger
	{
		get
		{
			return m_VolumeTrigger;
		}
		set
		{
			m_VolumeTrigger = value;
		}
	}

	internal VolumeFrameworkUpdateMode VolumeFrameworkUpdateMode
	{
		get
		{
			return m_VolumeFrameworkUpdateModeOption;
		}
		set
		{
			m_VolumeFrameworkUpdateModeOption = value;
		}
	}

	public bool RequiresVolumeFrameworkUpdate
	{
		get
		{
			if (m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.UsePipelineSettings)
			{
				return OwlcatRenderPipeline.Asset.VolumeFrameworkUpdateMode != VolumeFrameworkUpdateMode.ViaScripting;
			}
			return m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.EveryFrame;
		}
	}

	public VolumeStack VolumeStack
	{
		get
		{
			return m_VolumeStack;
		}
		set
		{
			m_VolumeStack = value;
		}
	}

	public bool RenderPostProcessing
	{
		get
		{
			return m_RenderPostProcessing;
		}
		set
		{
			m_RenderPostProcessing = value;
		}
	}

	public AntialiasingMode Antialiasing
	{
		get
		{
			return m_Antialiasing;
		}
		set
		{
			m_Antialiasing = value;
		}
	}

	public AntialiasingQuality AntialiasingQuality
	{
		get
		{
			return m_AntialiasingQuality;
		}
		set
		{
			m_AntialiasingQuality = value;
		}
	}

	public bool Dithering
	{
		get
		{
			return m_Dithering;
		}
		set
		{
			m_Dithering = value;
		}
	}

	public bool AllowLighting
	{
		get
		{
			return m_AllowLighting;
		}
		set
		{
			m_AllowLighting = value;
		}
	}

	public bool AllowDecals
	{
		get
		{
			return m_AllowDecals;
		}
		set
		{
			m_AllowDecals = value;
		}
	}

	public bool AllowDistortion
	{
		get
		{
			return m_AllowDistortion;
		}
		set
		{
			m_AllowDistortion = value;
		}
	}

	public bool AllowIndirectRendering
	{
		get
		{
			return m_AllowIndirectRendering;
		}
		set
		{
			m_AllowIndirectRendering = value;
		}
	}

	public bool AllowFog
	{
		get
		{
			return m_AllowFog;
		}
		set
		{
			m_AllowFog = value;
		}
	}

	public bool AllowVfxPreparation
	{
		get
		{
			return m_AllowVfxPreparation;
		}
		set
		{
			m_AllowVfxPreparation = value;
		}
	}

	public RenderTexture DepthTexture
	{
		get
		{
			return m_DepthTexture;
		}
		set
		{
			m_DepthTexture = value;
		}
	}

	internal void UpdateCameraStack()
	{
		int count = m_Cameras.Count;
		m_Cameras.RemoveAll((Camera cam) => cam == null);
		int count2 = m_Cameras.Count;
		int num = count - count2;
		if (num != 0)
		{
			Debug.LogWarning(base.name + ": " + num + " camera overlay" + ((num > 1) ? "s" : "") + " no longer exists and will be removed from the camera stack.");
		}
	}

	public void SetRenderer(int index)
	{
		m_RendererIndex = index;
	}

	public void GetDisabledFeatures(HashSet<string> disabledFeatures)
	{
		foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
		{
			if (!rendererFeaturesFlag.Enabled && !disabledFeatures.Contains(rendererFeaturesFlag.FeatureIdentifier))
			{
				disabledFeatures.Add(rendererFeaturesFlag.FeatureIdentifier);
			}
		}
	}

	public void DisableFeature(ScriptableRendererFeature feature)
	{
		if (feature == null)
		{
			return;
		}
		bool flag = false;
		foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
		{
			if (rendererFeaturesFlag.FeatureIdentifier == feature.GetFeatureIdentifier())
			{
				rendererFeaturesFlag.Enabled = false;
				flag = true;
			}
		}
		if (!flag)
		{
			m_RendererFeaturesFlags.Add(new RendererFeatureFlag
			{
				FeatureIdentifier = feature.GetFeatureIdentifier(),
				Enabled = false
			});
		}
	}

	public void EnableFeature(ScriptableRendererFeature feature)
	{
		if (feature == null)
		{
			return;
		}
		foreach (RendererFeatureFlag rendererFeaturesFlag in m_RendererFeaturesFlags)
		{
			if (rendererFeaturesFlag.FeatureIdentifier == feature.GetFeatureIdentifier())
			{
				rendererFeaturesFlag.Enabled = true;
			}
		}
	}

	public void DisableAllFeatures()
	{
		OwlcatRenderPipelineAsset asset = OwlcatRenderPipeline.Asset;
		if (!(asset != null) || !(asset.ScriptableRendererData != null))
		{
			return;
		}
		foreach (ScriptableRendererFeature rendererFeature in asset.ScriptableRendererData.rendererFeatures)
		{
			DisableFeature(rendererFeature);
		}
	}
}
