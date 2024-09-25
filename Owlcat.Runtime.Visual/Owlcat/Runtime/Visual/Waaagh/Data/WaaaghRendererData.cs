using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

[Serializable]
[ReloadGroup]
[ExcludeFromPreset]
public class WaaaghRendererData : ScriptableRendererData, ISerializationCallbackReceiver
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
		public Shader CopyDepthPS;

		[Reload("Shaders/Utils/CopyDepthSimple.shader", ReloadAttribute.Package.Root)]
		public Shader CopyDepthSimplePS;

		[Reload("Shaders/Utils/DepthPyramid.compute", ReloadAttribute.Package.Root)]
		public ComputeShader DepthPyramidCS;

		[Reload("Shaders/Utils/FinalBlit.shader", ReloadAttribute.Package.Root)]
		public Shader FinalBlitShader;

		[Reload("Shaders/PostProcessing/EdgeAdaptiveSpatialUpsampling.shader", ReloadAttribute.Package.Root)]
		public Shader FsrEasuShader;

		[Reload("Runtime/Waaagh/Lighting/LightCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader LightCullingShader;

		[Reload("Runtime/Waaagh/Lighting/ComputeTilesMinMaxZ.compute", ReloadAttribute.Package.Root)]
		public ComputeShader ComputeTilesMinMaxZCS;

		[Reload("Runtime/Waaagh/Lighting/DeferredReflections.shader", ReloadAttribute.Package.Root)]
		public Shader DeferredReflectionsShader;

		[Reload("Runtime/Waaagh/Lighting/DeferredLighting.shader", ReloadAttribute.Package.Root)]
		public Shader DeferredLightingShader;

		[Reload("Runtime/Waaagh/Lighting/DeferredLighting.compute", ReloadAttribute.Package.Root)]
		public ComputeShader DeferredLightingCS;

		[Reload("Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader BlitShader;

		[Reload("Shaders/Utils/ColorPyramid.shader", ReloadAttribute.Package.Root)]
		public Shader ColorPyramidShader;

		[Reload("Shaders/Utils/ApplyDistortion.shader", ReloadAttribute.Package.Root)]
		public Shader ApplyDistortionShader;

		[Reload("Shaders/Utils/DBufferBlit.shader", ReloadAttribute.Package.Root)]
		public Shader DBufferBlitShader;

		[Reload("Shaders/Utils/Fog.shader", ReloadAttribute.Package.Root)]
		public Shader FogShader;

		[Reload("Shaders/HBAO/HBAO.shader", ReloadAttribute.Package.Root)]
		public Shader HbaoShader;

		[Reload("Runtime/IndirectRendering/IndirectCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader IndirectRenderingCullShader;

		[Reload("Shaders/PostProcessing/ScreenSpaceReflections/ScreenSpaceReflections.compute", ReloadAttribute.Package.Root)]
		public ComputeShader ScreenSpaceReflectionsShaderCS;

		[Reload("Shaders/PostProcessing/ScreenSpaceReflections/WaaaghSSR.shader", ReloadAttribute.Package.Root)]
		public Shader ScreenSpaceReflectionsShaderPS;

		[Reload("Shaders/PostProcessing/ScreenSpaceReflections/StochasticSSR.compute", ReloadAttribute.Package.Root)]
		public ComputeShader StochasticScreenSpaceReflectionsCS;

		[Reload("Runtime/Waaagh/BilateralUpsample/BilateralUpsample.compute", ReloadAttribute.Package.Root)]
		public ComputeShader BilateralUpsampleCS;

		[Reload("Shaders/PostProcessing/CameraMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader CameraMotionVectorsPS;

		[Reload("Shaders/PostProcessing/ObjectMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader ObjectMotionVectorsPS;

		[Reload("Shaders/Utils/CoreBlit.shader", ReloadAttribute.Package.Root)]
		public Shader CoreBlitPS;

		[Reload("Shaders/Utils/CoreBlitColorAndDepth.shader", ReloadAttribute.Package.Root)]
		public Shader CoreBlitColorAndDepthPS;

		[Reload("Shaders/Utils/MedianBlur.compute", ReloadAttribute.Package.Root)]
		public ComputeShader MedianBlurCS;

		[Reload("Shaders/Utils/CopyCachedShadows.shader", ReloadAttribute.Package.Root)]
		public Shader CopyCachedShadowsPS;
	}

	public ShaderResources Shaders;

	public PostProcessData PostProcessData;

	[SerializeField]
	private TileSize m_TileSize = TileSize.Tile16;

	[SerializeField]
	private DeferredLightingMode m_DeferredLightingMode;

	[SerializeField]
	private TileSize m_DeferredLightingComputeDispatchTileSize = TileSize.Tile16;

	public TileSize TileSize => m_TileSize;

	public DeferredLightingMode DeferredLightingMode
	{
		get
		{
			return m_DeferredLightingMode;
		}
		set
		{
			m_DeferredLightingMode = value;
		}
	}

	public TileSize DeferredLightingComputeDispatchTileSize
	{
		get
		{
			return m_DeferredLightingComputeDispatchTileSize;
		}
		set
		{
			m_DeferredLightingComputeDispatchTileSize = value;
		}
	}

	protected override ScriptableRenderer Create()
	{
		if (!Application.isPlaying)
		{
			ReloadAllNullProperties();
		}
		return new WaaaghRenderer(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Shaders != null)
		{
			ReloadAllNullProperties();
		}
	}

	private void ReloadAllNullProperties()
	{
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
