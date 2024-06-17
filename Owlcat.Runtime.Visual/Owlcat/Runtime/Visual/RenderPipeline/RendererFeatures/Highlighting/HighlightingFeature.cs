using System;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting.Passes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting;

[CreateAssetMenu(menuName = "Renderer Features/Highlighting")]
public class HighlightingFeature : ScriptableRendererFeature
{
	internal enum TestPlanesResults
	{
		Inside,
		Intersect,
		Outside
	}

	internal struct BoundsVisibility
	{
		public Bounds Bounds;

		public TestPlanesResults Visibility;
	}

	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/Highlighting/Shaders/Highlighter.shader", ReloadAttribute.Package.Root)]
		public Shader HighlighterShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/Highlighting/Shaders/HighlightingBlur.shader", ReloadAttribute.Package.Root)]
		public Shader BlurShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/Highlighting/Shaders/HighlightingCut.shader", ReloadAttribute.Package.Root)]
		public Shader CutShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/Highlighting/Shaders/HighlightingComposite.shader", ReloadAttribute.Package.Root)]
		public Shader CompositeShader;
	}

	public enum Downsample
	{
		None = 1,
		Half = 2,
		Quarter = 4
	}

	public enum BlurDirections
	{
		Diagonal,
		Straight,
		All
	}

	public ShaderResources Shaders;

	[SerializeField]
	private Downsample m_DownsampleFactor = Downsample.None;

	[SerializeField]
	[Range(0f, 50f)]
	private int m_BlurIterations = 2;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurMinSpread = 0.65f;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_BlurSpread = 0.25f;

	[SerializeField]
	private BlurDirections m_BlurDirections;

	[SerializeField]
	private bool m_ZTestEnabled;

	private HighlighterPass m_HighlighterPass;

	private static HighlightingFeature s_Instance;

	internal NativeArray<Plane> CameraPlanes;

	internal NativeArray<BoundsVisibility> Bounds;

	public static HighlightingFeature Instance => s_Instance;

	public Downsample DownsampleFactor
	{
		get
		{
			return m_DownsampleFactor;
		}
		set
		{
			m_DownsampleFactor = value;
		}
	}

	public int BlurIterations
	{
		get
		{
			return m_BlurIterations;
		}
		set
		{
			m_BlurIterations = Mathf.Clamp(value, 0, 50);
		}
	}

	public float BlurMinSpread
	{
		get
		{
			return m_BlurMinSpread;
		}
		set
		{
			m_BlurMinSpread = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public float BlurSpread
	{
		get
		{
			return m_BlurSpread;
		}
		set
		{
			m_BlurSpread = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public BlurDirections BlurDirectons
	{
		get
		{
			return m_BlurDirections;
		}
		set
		{
			m_BlurDirections = value;
		}
	}

	public bool ZTestEnabled
	{
		get
		{
			return m_ZTestEnabled;
		}
		set
		{
			m_ZTestEnabled = value;
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		CameraType cameraType = renderingData.CameraData.Camera.cameraType;
		if (cameraType != CameraType.Preview && cameraType != CameraType.Reflection)
		{
			if (!CameraPlanes.IsCreated)
			{
				CameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
			}
			ClusteredRenderer clusteredRenderer = renderer as ClusteredRenderer;
			m_HighlighterPass.Setup(renderingData.CameraData.Camera, clusteredRenderer.GetCurrentCameraFinalColorTexture(ref renderingData), clusteredRenderer.GetCurrentCameraDepthTexture());
			renderer.EnqueuePass(m_HighlighterPass);
		}
	}

	public override void Create()
	{
		Material highlighterMaterial = CoreUtils.CreateEngineMaterial(Shaders.HighlighterShader);
		Material blurMaterial = CoreUtils.CreateEngineMaterial(Shaders.BlurShader);
		Material cutMaterial = CoreUtils.CreateEngineMaterial(Shaders.CutShader);
		Material compositeMaterial = CoreUtils.CreateEngineMaterial(Shaders.CompositeShader);
		m_HighlighterPass = new HighlighterPass(RenderPassEvent.AfterRendering, this, highlighterMaterial, blurMaterial, cutMaterial, compositeMaterial);
		s_Instance = this;
	}

	public override void DisableFeature()
	{
	}

	public override string GetFeatureIdentifier()
	{
		return "HighlightingFeature";
	}

	protected override void Dispose(bool disposing)
	{
		if (CameraPlanes.IsCreated)
		{
			CameraPlanes.Dispose();
		}
		if (Bounds.IsCreated)
		{
			Bounds.Dispose();
		}
	}
}
