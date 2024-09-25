using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Decals;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DrawDecalsPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Draw Decals";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Decals");

	private List<ShaderTagId> m_ShaderTags = new List<ShaderTagId>();

	private FilteringSettings m_OpaqueFilterSettings;

	private RenderTargetHandle m_ColorAttachment;

	private GBuffer m_GBuffer;

	private RenderTargetIdentifier[] m_DBuffer = new RenderTargetIdentifier[3];

	private bool m_DrawGUIDecals;

	private Material m_DBufferBlitMat;

	public DrawDecalsPass(RenderPassEvent evt, Material dBufferBlitMaterial, bool drawGUIDecals = false)
	{
		base.RenderPassEvent = evt;
		m_OpaqueFilterSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DrawGUIDecals = drawGUIDecals;
		if (m_DrawGUIDecals)
		{
			m_ShaderTags.Add(new ShaderTagId("DecalGUI"));
		}
		else
		{
			m_ShaderTags.Add(new ShaderTagId("Decal"));
		}
		m_DBufferBlitMat = dBufferBlitMaterial;
	}

	public void Setup(RenderTargetHandle colorAttachment, GBuffer gBuffer)
	{
		m_ColorAttachment = colorAttachment;
		m_GBuffer = gBuffer;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			context.SetupCameraProperties(renderingData.CameraData.Camera);
			if (!m_DrawGUIDecals && m_GBuffer.RenderPath == RenderPath.Deferred && renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraColorPyramidRt.Identifier());
				commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
				RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
				cameraTargetDescriptor.colorFormat = RenderTextureFormat.RG16;
				m_DBuffer[0] = m_ColorAttachment.Identifier();
				m_DBuffer[1] = m_GBuffer.CameraColorPyramidRt.Identifier();
				m_DBuffer[2] = m_GBuffer.CameraTranslucencyRT.Identifier();
				commandBuffer.SetRenderTarget(m_DBuffer, m_GBuffer.CameraDepthRt.Identifier());
			}
			else
			{
				commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier(), m_GBuffer.CameraDepthRt.Identifier());
			}
			if (!m_DrawGUIDecals && FullScreenDecal.All.Count > 0)
			{
				CameraData cameraData = renderingData.CameraData;
				foreach (FullScreenDecal item in FullScreenDecal.All)
				{
					if ((cameraData.Camera.gameObject.scene == item.gameObject.scene || cameraData.IsSceneViewCamera) && item.Material != null)
					{
						int num = item.Material.FindPass("FULL SCREEN DECAL");
						if (num >= 0)
						{
							commandBuffer.DrawProcedural(item.transform.localToWorldMatrix, item.Material, num, MeshTopology.Triangles, 3);
						}
					}
				}
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTags, ref renderingData, SortingCriteria.RenderQueue);
			drawingSettings.enableDynamicBatching = false;
			drawingSettings.enableInstancing = true;
			context.DrawRenderers(renderingData.CullResults, ref drawingSettings, ref m_OpaqueFilterSettings);
			if (!m_DrawGUIDecals && m_GBuffer.RenderPath == RenderPath.Deferred && renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				commandBuffer.SetGlobalTexture("_DecalsNormalsRT", m_GBuffer.CameraColorPyramidRt.Identifier());
				commandBuffer.SetGlobalTexture("_DecalsMasksRT", m_GBuffer.CameraTranslucencyRT.Identifier());
				commandBuffer.SetRenderTarget(m_GBuffer.CameraDepthRt.Identifier());
				commandBuffer.SetRandomWriteTarget(1, m_GBuffer.CameraSpecularRt.Identifier());
				commandBuffer.SetRandomWriteTarget(2, m_GBuffer.CameraNormalsRt.Identifier());
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_DBufferBlitMat, 0, MeshTopology.Triangles, 3);
				commandBuffer.ClearRandomWriteTargets();
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
