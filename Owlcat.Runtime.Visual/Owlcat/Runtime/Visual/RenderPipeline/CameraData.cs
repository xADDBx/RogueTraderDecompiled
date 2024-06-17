using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public struct CameraData
{
	public Camera Camera;

	public RenderTargetIdentifier DepthTexture;

	public bool IsFirstInChain;

	public bool IsLastInChain;

	public RenderTextureDescriptor CameraTargetDescriptor;

	public float RenderScale;

	public bool IsSceneViewCamera;

	public bool IsSceneViewInPrefabEditMode;

	public bool IsDefaultViewport;

	public bool IsHdrEnabled;

	public float ShadowDistance;

	public bool IsDistortionEnabled;

	public bool IsLightingEnabled;

	public bool IsDecalsEnabled;

	public bool IsIndirectRenderingEnabled;

	public bool IsFogEnabled;

	public bool IsVfxEnabled;

	public bool IsStereoEnabled;

	public HashSet<string> DisabledRendererFeatures;

	public SortingCriteria DefaultOpaqueSortFlags;

	public LayerMask VolumeLayerMask;

	public Transform VolumeTrigger;

	public bool IsScreenSpaceReflectionsEnabled;

	public bool IsNeedDepthPyramid;

	public bool IsPostProcessEnabled;

	public bool IsStopNaNEnabled;

	public bool IsDitheringEnabled;

	public AntialiasingMode Antialiasing;

	public AntialiasingQuality AntialiasingQuality;
}
