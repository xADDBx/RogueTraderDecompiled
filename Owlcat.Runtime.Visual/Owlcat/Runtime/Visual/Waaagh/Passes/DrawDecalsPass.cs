using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDecalsPass : DrawRendererListPass<DrawDecalsPassData>
{
	private Material m_DBufferBlitMaterial;

	private bool m_DrawGUIDecals;

	private ShaderTagId[] m_ShaderTags;

	private FilteringSettings m_OpaqueFilterSettings;

	public override string Name => "DrawDecalsPass";

	public DrawDecalsPass(RenderPassEvent evt, Material dBufferBlitMaterial, bool drawGUIDecals)
		: base(evt)
	{
		m_DBufferBlitMaterial = dBufferBlitMaterial;
		m_DrawGUIDecals = drawGUIDecals;
		if (drawGUIDecals)
		{
			m_ShaderTags = new ShaderTagId[2]
			{
				new ShaderTagId("DecalGUI"),
				new ShaderTagId("DecalForwardOverlay")
			};
		}
		else
		{
			m_ShaderTags = new ShaderTagId[1]
			{
				new ShaderTagId("DecalDeferred")
			};
		}
		m_OpaqueFilterSettings = new FilteringSettings(RenderQueueRange.opaque);
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		RenderGraph renderGraph = renderingData.RenderGraph;
		RendererListDesc desc = CreateRendererListDesc(ref renderingData);
		rendererList = renderGraph.CreateRendererList(in desc);
	}

	private RendererListDesc CreateRendererListDesc(ref RenderingData renderingData)
	{
		return RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_ShaderTags, renderingData.PerObjectData, m_OpaqueFilterSettings.renderQueueRange, SortingCriteria.RenderQueue);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawDecalsPassData data, ref RenderingData renderingData)
	{
		data.DrawGUIDecals = m_DrawGUIDecals;
		data.CameraDepthRT = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		data.CameraDepthCopyRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.CameraNormalsRT = builder.ReadWriteTexture(in data.Resources.CameraNormalsRT);
		data.CameraSpecularRT = builder.ReadWriteTexture(in data.Resources.CameraSpecularRT);
		if (m_DrawGUIDecals)
		{
			data.CameraColorRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		}
		else
		{
			data.DBufferBlitMaterial = m_DBufferBlitMaterial;
			TextureDesc desc = RenderingUtils.CreateTextureDesc("DBuffer0RT", renderingData.CameraData.CameraTargetDescriptor);
			desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			desc.depthBufferBits = DepthBits.None;
			desc.filterMode = FilterMode.Bilinear;
			desc.wrapMode = TextureWrapMode.Clamp;
			TextureHandle dBuffer0RT = builder.CreateTransientTexture(in desc);
			TextureDesc desc2 = desc;
			desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
			TextureHandle dBuffer1RT = builder.CreateTransientTexture(in desc2);
			data.DBuffer0RT = dBuffer0RT;
			data.DBuffer1RT = dBuffer1RT;
			data.CameraAlbedoRT = builder.WriteTexture(in data.Resources.CameraAlbedoRT);
			data.CameraEmissionRT = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		}
		data.DrawingSettings = CreateDrawingSettings(m_ShaderTags, ref renderingData, SortingCriteria.RenderQueue);
		data.DrawingSettings.enableDynamicBatching = false;
		data.DrawingSettings.enableInstancing = true;
		data.CullingResults = renderingData.CullingResults;
		data.FilteringSettings = m_OpaqueFilterSettings;
	}

	protected override void Render(DrawDecalsPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.CameraDepthCopyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		if (data.DrawGUIDecals)
		{
			context.renderContext.ExecuteCommandBuffer(context.cmd);
			context.cmd.Clear();
			context.renderContext.DrawRenderers(data.CullingResults, ref data.DrawingSettings, ref data.FilteringSettings);
			return;
		}
		RenderTargetIdentifier[] tempArray = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(2);
		tempArray[0] = data.DBuffer0RT;
		tempArray[1] = data.DBuffer1RT;
		context.cmd.SetRenderTarget(tempArray, data.CameraDepthRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, 1, MeshTopology.Triangles, 3);
		RenderTargetIdentifier[] tempArray2 = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(4);
		tempArray2[0] = data.DBuffer0RT;
		tempArray2[1] = data.DBuffer1RT;
		tempArray2[2] = data.CameraAlbedoRT;
		tempArray2[3] = data.CameraEmissionRT;
		context.cmd.SetRenderTarget(tempArray2, data.CameraDepthRT);
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
		context.renderContext.DrawRenderers(data.CullingResults, ref data.DrawingSettings, ref data.FilteringSettings);
		context.cmd.SetGlobalTexture(ShaderPropertyId._DecalsNormalsRT, data.DBuffer1RT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._DecalsMasksRT, data.DBuffer0RT);
		RenderTargetIdentifier[] tempArray3 = context.renderGraphPool.GetTempArray<RenderTargetIdentifier>(2);
		tempArray3[0] = data.CameraNormalsRT;
		tempArray3[1] = data.CameraSpecularRT;
		context.cmd.SetRenderTarget(tempArray3, data.CameraDepthRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DBufferBlitMaterial, 2, MeshTopology.Triangles, 3);
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId[] shaderTags, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		Camera camera = renderingData.CameraData.Camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = sortingCriteria;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings2);
		drawingSettings.perObjectData = renderingData.PerObjectData;
		drawingSettings.enableInstancing = true;
		drawingSettings.enableDynamicBatching = renderingData.SupportsDynamicBatching;
		DrawingSettings result = drawingSettings;
		for (int i = 1; i < shaderTags.Length; i++)
		{
			result.SetShaderPassName(i, shaderTags[i]);
		}
		return result;
	}
}
