using System;
using System.IO;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.Debugging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

[ReloadGroup]
public class ClusteredRendererData : ScriptableRendererData
{
	[Serializable]
	[ReloadGroup]
	public class ShaderResources
	{
		[Reload("Runtime/RenderPipeline/Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
		public Shader CopyDepthShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/CopyDepthSimple.shader", ReloadAttribute.Package.Root)]
		public Shader CopyDepthFastShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/DepthPyramid.compute", ReloadAttribute.Package.Root)]
		public ComputeShader DepthPyramidShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader BlitShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/FinalBlit.shader", ReloadAttribute.Package.Root)]
		public Shader FinalBlitShader;

		[Reload("Runtime/RenderPipeline/Lighting/LightCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader LightCullingShader;

		[Reload("Runtime/RenderPipeline/Lighting/DeferredReflections.shader", ReloadAttribute.Package.Root)]
		public Shader DeferredReflectionsShader;

		[Reload("Runtime/RenderPipeline/Lighting/DeferredLighting.shader", ReloadAttribute.Package.Root)]
		public Shader DeferredLightingShader;

		[Reload("Runtime/RenderPipeline/Lighting/DeferredLighting.compute", ReloadAttribute.Package.Root)]
		public ComputeShader DeferredLightingComputeShader;

		[Reload("Runtime/IndirectRendering/IndirectCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader IndirectRenderingCullShader;

		[Reload("Runtime/RenderPipeline/Shaders/Skybox/Skybox-Procedural.shader", ReloadAttribute.Package.Root)]
		public Shader SkyboxShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/ColorPyramid.shader", ReloadAttribute.Package.Root)]
		public Shader ColorPyramidShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/ApplyDistortion.shader", ReloadAttribute.Package.Root)]
		public Shader ApplyDistortionShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/Fog.shader", ReloadAttribute.Package.Root)]
		public Shader FogShader;

		[Reload("Runtime/RenderPipeline/Shaders/HBAO/HBAO.shader", ReloadAttribute.Package.Root)]
		public Shader HbaoShader;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/ScreenSpaceCloudShadows.shader", ReloadAttribute.Package.Root)]
		public Shader ScreenSpaceCloudShadowsShader;

		[Reload("Runtime/RenderPipeline/Shaders/PostProcessing/ScreenSpaceReflections/ScreenSpaceReflections.shader", ReloadAttribute.Package.Root)]
		public Shader ScreenSpaceReflectionsShaderPS;

		[Reload("Runtime/RenderPipeline/Shaders/PostProcessing/ScreenSpaceReflections/ScreenSpaceReflections.compute", ReloadAttribute.Package.Root)]
		public ComputeShader ScreenSpaceReflectionsShaderCS;

		[Reload("Runtime/RenderPipeline/Shaders/Utils/DBufferBlit.shader", ReloadAttribute.Package.Root)]
		public Shader DBufferBlitShader;
	}

	public const int kMaxSliceCount = 64;

	public ShaderResources Shaders = new ShaderResources();

	public string ShadersBundlePath;

	[SerializeField]
	private PostProcessData m_PostProcessData;

	[SerializeField]
	private DebugData m_DebugData;

	[SerializeField]
	private TileSize m_TileSize = TileSize.Tile16;

	[SerializeField]
	private RenderPath m_RenderPath;

	[SerializeField]
	private bool m_UseComputeInDeferredPath;

	[SerializeField]
	private LayerMask m_OpaqueLayerMask = -1;

	[SerializeField]
	private LayerMask m_TransparentLayerMask = -1;

	[SerializeField]
	private StencilStateData m_DefaultStencilState;

	[SerializeField]
	private Material m_DefaultUIMaterial;

	public PostProcessData PostProcessData
	{
		get
		{
			return m_PostProcessData;
		}
		internal set
		{
			m_PostProcessData = value;
		}
	}

	public DebugData DebugData => m_DebugData;

	public TileSize TileSize => m_TileSize;

	public RenderPath RenderPath
	{
		get
		{
			return m_RenderPath;
		}
		set
		{
			m_RenderPath = value;
		}
	}

	public bool UseComputeInDeferredPath
	{
		get
		{
			return m_UseComputeInDeferredPath;
		}
		set
		{
			m_UseComputeInDeferredPath = value;
		}
	}

	public LayerMask OpaqueLayerMask => m_OpaqueLayerMask;

	public LayerMask TransparentLayerMask => m_TransparentLayerMask;

	public StencilStateData DefaultStencilState => m_DefaultStencilState;

	public Material DefaultUIMaterial => m_DefaultUIMaterial;

	protected override ScriptableRenderer Create()
	{
		try
		{
			TryLoadMissingShadersFromBundle(Shaders, ShadersBundlePath);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		return new ClusteredRenderer(this);
	}

	private static void TryLoadMissingShadersFromBundle(ShaderResources resources, string assetBundlePath)
	{
		if (!string.IsNullOrEmpty(assetBundlePath) && File.Exists(assetBundlePath))
		{
			AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
			resources.CopyDepthShader = LoadShaderIfNull(resources.CopyDepthShader, "Hidden/Owlcat/CopyDepth", assetBundle);
			resources.CopyDepthFastShader = LoadShaderIfNull(resources.CopyDepthFastShader, "Hidden/Owlcat/CopyDepthSimple", assetBundle);
			resources.DepthPyramidShader = LoadComputeIfNull(resources.DepthPyramidShader, "DepthPyramid.compute", assetBundle);
			resources.BlitShader = LoadShaderIfNull(resources.BlitShader, "Hidden/Owlcat/Blit", assetBundle);
			resources.FinalBlitShader = LoadShaderIfNull(resources.FinalBlitShader, "Hidden/Owlcat/FinalBlit", assetBundle);
			resources.LightCullingShader = LoadComputeIfNull(resources.LightCullingShader, "LightCulling.compute", assetBundle);
			resources.DeferredReflectionsShader = LoadShaderIfNull(resources.DeferredReflectionsShader, "Hidden/Owlcat/DeferredReflections", assetBundle);
			resources.DeferredLightingShader = LoadShaderIfNull(resources.DeferredLightingShader, "Hidden/Owlcat/DeferredLighting", assetBundle);
			resources.DeferredLightingComputeShader = LoadComputeIfNull(resources.DeferredLightingComputeShader, "DeferredLighting.compute", assetBundle);
			resources.IndirectRenderingCullShader = LoadComputeIfNull(resources.IndirectRenderingCullShader, "IndirectCulling.compute", assetBundle);
			resources.SkyboxShader = LoadShaderIfNull(resources.SkyboxShader, "Owlcat/Skybox/Procedural", assetBundle);
			resources.ColorPyramidShader = LoadShaderIfNull(resources.ColorPyramidShader, "Hidden/Owlcat/ColorPyramid", assetBundle);
			resources.ApplyDistortionShader = LoadShaderIfNull(resources.ApplyDistortionShader, "Hidden/Owlcat/ApplyDistortion", assetBundle);
			resources.FogShader = LoadShaderIfNull(resources.FogShader, "Hidden/Owlcat/Fog", assetBundle);
			resources.HbaoShader = LoadShaderIfNull(resources.HbaoShader, "Hidden/Owlcat/HBAO", assetBundle);
			resources.ScreenSpaceCloudShadowsShader = LoadShaderIfNull(resources.ScreenSpaceCloudShadowsShader, "Hidden/Owlcat/ScreenSpaceCloudShadows", assetBundle);
			resources.ScreenSpaceReflectionsShaderPS = LoadShaderIfNull(resources.ScreenSpaceReflectionsShaderPS, "Hidden/Owlcat/ScreenSpaceReflections", assetBundle);
			resources.ScreenSpaceReflectionsShaderCS = LoadComputeIfNull(resources.ScreenSpaceReflectionsShaderCS, "ScreenSpaceReflections.compute", assetBundle);
			resources.DBufferBlitShader = LoadShaderIfNull(resources.DBufferBlitShader, "Hidden/Owlcat/DBufferBlit", assetBundle);
			assetBundle.Unload(unloadAllLoadedObjects: false);
		}
		static ComputeShader LoadComputeIfNull(ComputeShader shader, string name, AssetBundle shadersBundle)
		{
			return shadersBundle.LoadAsset<ComputeShader>(name);
		}
		static Shader LoadShaderIfNull(Shader shader, string name, AssetBundle shadersBundle)
		{
			return shadersBundle.LoadAsset<Shader>(name);
		}
	}
}
