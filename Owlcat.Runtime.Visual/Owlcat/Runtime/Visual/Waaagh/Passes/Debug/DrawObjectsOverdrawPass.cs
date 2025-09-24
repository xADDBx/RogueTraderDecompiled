using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsOverdrawPass : DrawRendererListPass<DrawObjectsOverdrawPassData>
{
	private const string kForwardLitPassName = "FORWARD LIT";

	private ShaderTagId[] m_OverdrawShaderTags = new ShaderTagId[4]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit"),
		new ShaderTagId("DecalDeferred"),
		new ShaderTagId("DecalGUI")
	};

	private DebugOverdrawMode m_OverdrawMode;

	private Color m_ClearColor = new Color(0f, 0f, 0f, 0f);

	private Color m_DebugColor = new Color(0.1f, 0.01f, 0.01f, 1f);

	private RenderQueueRange m_RenderQueueRange;

	public override string Name => "DrawObjectsOverdrawPass";

	public DebugOverdrawMode OverdrawMode
	{
		get
		{
			return m_OverdrawMode;
		}
		set
		{
			m_OverdrawMode = value;
		}
	}

	public DrawObjectsOverdrawPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererList rendererList)
	{
		RendererListDesc desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_OverdrawShaderTags, -1, renderingData.PerObjectData, WaaaghRenderQueue.Transparent, SortingCriteria.CommonTransparent);
		switch (m_OverdrawMode)
		{
		case DebugOverdrawMode.All:
			desc.renderQueueRange = WaaaghRenderQueue.All;
			break;
		case DebugOverdrawMode.TransparentOnly:
			desc.renderQueueRange = WaaaghRenderQueue.Transparent;
			break;
		case DebugOverdrawMode.OpaqueOnly:
			desc.renderQueueRange = sharedRendererLists.OpaqueGBuffer.Desc.renderQueueRange;
			break;
		}
		desc.rendererConfiguration = PerObjectData.None;
		desc.stateBlock = new RenderStateBlock
		{
			depthState = new DepthState(writeEnabled: false, CompareFunction.Always),
			blendState = new BlendState(separateMRTBlend: false, alphaToMask: false)
			{
				blendState0 = new RenderTargetBlendState
				{
					writeMask = ColorWriteMask.All,
					colorBlendOperation = BlendOp.Add,
					alphaBlendOperation = BlendOp.Add,
					sourceColorBlendMode = BlendMode.One,
					sourceAlphaBlendMode = BlendMode.One,
					destinationColorBlendMode = BlendMode.One,
					destinationAlphaBlendMode = BlendMode.Zero
				}
			},
			stencilState = new StencilState(enabled: false),
			mask = (RenderStateMask.Blend | RenderStateMask.Depth | RenderStateMask.Stencil)
		};
		rendererList = renderingData.Context.CreateRendererList(desc);
		m_RenderQueueRange = desc.renderQueueRange;
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsOverdrawPassData data, ref RenderingData renderingData)
	{
		TextureHandle input = data.Resources.FinalTarget;
		data.RenderTarget = builder.UseColorBuffer(in input, 0);
		data.ClearColor = m_ClearColor;
		data.DebugColor = m_DebugColor;
		builder.AllowRendererListCulling(value: false);
		data.CameraType = renderingData.CameraData.CameraType;
		data.IsIndirectRenderingEnabled = renderingData.CameraData.IsIndirectRenderingEnabled;
		data.IsSceneViewInPrefabEditMode = renderingData.CameraData.IsSceneViewInPrefabEditMode;
		data.RenderQueueRange = m_RenderQueueRange;
	}

	protected override void Render(DrawObjectsOverdrawPassData data, RenderGraphContext context)
	{
		context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, data.ClearColor);
		context.cmd.SetGlobalColor(ShaderPropertyId._DebugColor, data.DebugColor);
		context.cmd.DrawRendererList(data.RendererList);
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, "FORWARD LIT", data.RenderQueueRange, debugOverdraw: true);
	}
}
