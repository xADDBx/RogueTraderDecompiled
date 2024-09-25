using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Debug;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugHandler
{
	internal class DebugMipMapTexture
	{
		private readonly Color[] m_DebugColors = new Color[6]
		{
			new Color(0f, 0f, 1f, 0.8f),
			new Color(0f, 0.5f, 1f, 0.4f),
			new Color(1f, 1f, 1f, 0f),
			new Color(1f, 0.7f, 0f, 0.2f),
			new Color(1f, 0.3f, 0f, 0.6f),
			new Color(1f, 0f, 0f, 0.8f)
		};

		private Texture2D m_MipMapTexture;

		public Texture2D MipMapTexture
		{
			get
			{
				if (m_MipMapTexture == null)
				{
					CreateTexture();
				}
				return m_MipMapTexture;
			}
		}

		public DebugMipMapTexture()
		{
			CreateTexture();
		}

		private void CreateTexture()
		{
			int num = 32;
			int num2 = 0;
			m_MipMapTexture = new Texture2D(num, num, TextureFormat.RGBA32, mipChain: true);
			m_MipMapTexture.name = "_MipMapDebugMap";
			while (num >= 1)
			{
				Color[] array = new Color[num * num];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = m_DebugColors[num2];
				}
				m_MipMapTexture.SetPixels(array, num2);
				num2++;
				num /= 2;
			}
			m_MipMapTexture.filterMode = FilterMode.Trilinear;
			m_MipMapTexture.Apply(updateMipmaps: false);
		}

		public void Dispose()
		{
			if (m_MipMapTexture != null)
			{
				Object.DestroyImmediate(m_MipMapTexture);
				m_MipMapTexture = null;
			}
		}
	}

	public const int kFullScreenDebugBufferBinding = 1;

	private ScriptableRendererData m_Data;

	private WaaaghDebugData m_DebugData;

	private ScriptableRenderer m_Renderer;

	private DebugMipMapTexture m_MipMapTexture;

	private bool m_IsCompletelyOverridesRendering;

	private Material m_FullscreenDebugMaterial;

	private Material m_ShadowsDebugMaterial;

	private Material m_ShowLightSortingCurveMaterial;

	private SetupDebugBuffersPass m_SetupDebugBuffersPass;

	private ApplyDebugSettingsPass m_ApplyDebugSettingsPass;

	private DrawObjectsWireframePass m_DrawObjectsWireframePass;

	private DrawObjectsOverdrawPass m_DrawObjectsOverdrawPass;

	private DebugQuadOverdrawPass m_DebugQuadOverdrawPass;

	private FullscreenDebugPass m_FullscreenDebugPass;

	private ShowLightSortingCurvePass m_ShowLightSortingCurvePass;

	private ShadowsDebugPass m_ShadowsDebugPass;

	private RenderGraphDebugResources m_Resources;

	public bool IsCompletelyOverridesRendering => m_IsCompletelyOverridesRendering;

	public RenderGraphDebugResources Resources => m_Resources;

	public DebugHandler(ScriptableRendererData data, ScriptableRenderer renderer)
	{
		m_Data = data;
		m_DebugData = WaaaghPipeline.Asset.DebugData;
		m_Renderer = renderer;
		m_MipMapTexture = new DebugMipMapTexture();
		m_Resources = new RenderGraphDebugResources();
		m_FullscreenDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Shaders.DebugFullscreenPS);
		m_ShadowsDebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Shaders.ShadowsDebugPS);
		m_ShowLightSortingCurveMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.Shaders.ShowLightSortingCurvePS);
		m_SetupDebugBuffersPass = new SetupDebugBuffersPass(m_Resources, m_DebugData);
		m_ApplyDebugSettingsPass = new ApplyDebugSettingsPass(RenderPassEvent.BeforeRendering, m_MipMapTexture);
		m_DrawObjectsWireframePass = new DrawObjectsWireframePass(RenderPassEvent.BeforeRenderingTransparents);
		m_DrawObjectsOverdrawPass = new DrawObjectsOverdrawPass(RenderPassEvent.AfterRenderingTransparents);
		m_DebugQuadOverdrawPass = new DebugQuadOverdrawPass(m_DebugData, m_Resources);
		m_FullscreenDebugPass = new FullscreenDebugPass((RenderPassEvent)1001, m_DebugData, m_Resources, m_FullscreenDebugMaterial);
		m_ShadowsDebugPass = new ShadowsDebugPass((RenderPassEvent)1001, m_DebugData, m_ShadowsDebugMaterial);
		WaaaghRenderer renderer2 = m_Renderer as WaaaghRenderer;
		m_ShowLightSortingCurvePass = new ShowLightSortingCurvePass((RenderPassEvent)1001, renderer2, m_DebugData, m_ShowLightSortingCurveMaterial);
	}

	internal void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingData.CameraData.CameraType != CameraType.Game && renderingData.CameraData.CameraType != CameraType.SceneView)
		{
			return;
		}
		bool wireframe = GL.wireframe;
		bool flag = false;
		bool num = m_DebugData.RenderingDebug.OverdrawMode == DebugOverdrawMode.QuadOverdraw;
		if (m_DebugData != null)
		{
			flag = m_DebugData.RenderingDebug.OverdrawMode != DebugOverdrawMode.None;
		}
		m_IsCompletelyOverridesRendering = wireframe || flag;
		if (num)
		{
			m_Renderer.EnqueuePass(m_SetupDebugBuffersPass);
		}
		m_Renderer.EnqueuePass(m_ApplyDebugSettingsPass);
		if (wireframe)
		{
			m_Renderer.EnqueuePass(m_DrawObjectsWireframePass);
		}
		else if (flag)
		{
			switch (m_DebugData.RenderingDebug.OverdrawMode)
			{
			case DebugOverdrawMode.All:
			case DebugOverdrawMode.TransparentOnly:
			case DebugOverdrawMode.OpaqueOnly:
				m_DrawObjectsOverdrawPass.OverdrawMode = m_DebugData.RenderingDebug.OverdrawMode;
				m_Renderer.EnqueuePass(m_DrawObjectsOverdrawPass);
				break;
			case DebugOverdrawMode.QuadOverdraw:
				m_Renderer.EnqueuePass(m_DebugQuadOverdrawPass);
				m_Renderer.EnqueuePass(m_FullscreenDebugPass);
				break;
			}
		}
		else
		{
			m_Renderer.EnqueuePass(m_FullscreenDebugPass);
			m_Renderer.EnqueuePass(m_ShadowsDebugPass);
			if (m_DebugData.LightingDebug.ShowLightSortingCurve)
			{
				m_Renderer.EnqueuePass(m_ShowLightSortingCurvePass);
			}
		}
	}

	internal void Dispose()
	{
		CoreUtils.Destroy(m_FullscreenDebugMaterial);
		CoreUtils.Destroy(m_ShadowsDebugMaterial);
		CoreUtils.Destroy(m_ShowLightSortingCurveMaterial);
		m_MipMapTexture.Dispose();
	}
}
