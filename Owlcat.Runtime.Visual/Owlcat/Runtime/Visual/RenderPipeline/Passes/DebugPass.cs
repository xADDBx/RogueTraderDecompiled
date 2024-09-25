using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.RenderPipeline.Debugging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DebugPass : ScriptableRenderPass
{
	private const string kClusterSlicesTag = "Cluster Slices Debug";

	private const string kClusterHeatmap = "Cluster Heatmap Debug";

	private const string kDebugBuffer = "Debug Buffer";

	private const string kDebugOverdraw = "Debug Overdraw";

	private ProfilingSampler m_ClusterSlicesProfilingSampler = new ProfilingSampler("Cluster Slices Debug");

	private ProfilingSampler m_ClusterHeatmapProfilingSampler = new ProfilingSampler("Cluster Heatmap Debug");

	private ProfilingSampler m_DebugBufferProfilingSampler = new ProfilingSampler("Debug Buffer");

	private ProfilingSampler m_DebugOverdrawProfilingSampler = new ProfilingSampler("Debug Overdraw");

	private Material m_DebugMaterial;

	private Vector2Int m_fullScreenSize;

	private DebugData m_DebugData;

	private RenderTextureDescriptor m_DebugRtDesc;

	private RenderTargetHandle m_DebugRt;

	private RenderTargetHandle m_DepthAttachment;

	private ShaderTagId m_DebugAdditionalPassShaderTag = new ShaderTagId("DebugAdditional");

	private FilteringSettings m_FilteringSettingsAll;

	private FilteringSettings m_FilteringSettingsTransparent;

	private FilteringSettings m_FilteringSettingsOpaque;

	public DebugPass(RenderPassEvent evt, DebugData debugData)
	{
		base.RenderPassEvent = evt;
		m_DebugData = debugData;
		m_DebugMaterial = CoreUtils.CreateEngineMaterial(m_DebugData.DebugShader);
		m_DebugRt.Init("_DebugRT");
		m_FilteringSettingsAll = new FilteringSettings(RenderQueueRange.all);
		m_FilteringSettingsOpaque = new FilteringSettings(RenderQueueRange.opaque);
		m_FilteringSettingsTransparent = new FilteringSettings(RenderQueueRange.transparent);
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachment)
	{
		m_DebugRtDesc = baseDescriptor;
		m_DebugRtDesc.colorFormat = RenderTextureFormat.ARGB32;
		m_DebugRtDesc.depthBufferBits = 0;
		m_DepthAttachment = depthAttachment;
		m_fullScreenSize = new Vector2Int(baseDescriptor.width, baseDescriptor.height);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		switch (m_DebugData.RenderingDebug.DebugBuffers)
		{
		case DebugBuffers.Depth:
		{
			CommandBuffer commandBuffer4 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer4, m_DebugBufferProfilingSampler))
			{
				commandBuffer4.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("DEPTH"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer4);
			CommandBufferPool.Release(commandBuffer4);
			break;
		}
		case DebugBuffers.GBuffer0_RGB:
		case DebugBuffers.GBuffer0_A:
		case DebugBuffers.GBuffer1_RGB:
		case DebugBuffers.GBuffer1_A:
		{
			CommandBuffer commandBuffer2 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer2, m_DebugBufferProfilingSampler))
			{
				commandBuffer2.SetGlobalInt("_Debug_GBuffer_Channel", (int)m_DebugData.RenderingDebug.DebugBuffers);
				commandBuffer2.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("GBUFFER DEBUG"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer2);
			CommandBufferPool.Release(commandBuffer2);
			break;
		}
		case DebugBuffers.Shadowmap:
		{
			CommandBuffer commandBuffer3 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer3, m_DebugBufferProfilingSampler))
			{
				commandBuffer3.SetViewport(new Rect(0f, 0f, Mathf.Min(m_fullScreenSize.x, m_fullScreenSize.y), Mathf.Min(m_fullScreenSize.x, m_fullScreenSize.y)));
				commandBuffer3.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("SHADOWMAP"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer3);
			CommandBufferPool.Release(commandBuffer3);
			break;
		}
		case DebugBuffers.ScreenSpaceShadows:
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_DebugBufferProfilingSampler))
			{
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("SCREENSPACE SHADOWMAP"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
			break;
		}
		}
		switch (m_DebugData.LightingDebug.DebugClustersMode)
		{
		case DebugClustersMode.Heatmap:
		{
			CommandBuffer commandBuffer6 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer6, m_ClusterHeatmapProfilingSampler))
			{
				commandBuffer6.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("TILES HEATMAP"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer6);
			CommandBufferPool.Release(commandBuffer6);
			break;
		}
		case DebugClustersMode.HeatmapShadowedLights:
		{
			CommandBuffer commandBuffer5 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer5, m_ClusterHeatmapProfilingSampler))
			{
				commandBuffer5.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("CLUSTERS HEATMAP SHADOWS"), MeshTopology.Triangles, 3);
			}
			context.ExecuteCommandBuffer(commandBuffer5);
			CommandBufferPool.Release(commandBuffer5);
			break;
		}
		}
		switch (m_DebugData.OverdrawDebug.OverdrawMode)
		{
		case OverdrawMode.Transparent:
		{
			CommandBuffer commandBuffer8 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer8, m_DebugOverdrawProfilingSampler))
			{
				context.SetupCameraProperties(renderingData.CameraData.Camera);
				commandBuffer8.GetTemporaryRT(m_DebugRt.Id, m_DebugRtDesc, FilterMode.Bilinear);
				commandBuffer8.SetRenderTarget(m_DebugRt.Identifier());
				commandBuffer8.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
				context.ExecuteCommandBuffer(commandBuffer8);
				commandBuffer8.Clear();
				commandBuffer8.SetGlobalVector(DebugBuffer._DebugOverdrawChannelMask, new Vector4(1f, 0f, 0f, 0f));
				context.ExecuteCommandBuffer(commandBuffer8);
				commandBuffer8.Clear();
				DrawingSettings drawingSettings2 = CreateDrawingSettings(m_DebugAdditionalPassShaderTag, ref renderingData, renderingData.CameraData.DefaultOpaqueSortFlags);
				context.DrawRenderers(renderingData.CullResults, ref drawingSettings2, ref m_FilteringSettingsTransparent);
				IndirectRenderingSystem.Instance.DrawPass(ref context, renderingData.CameraData.Camera.cameraType, renderingData.CameraData.IsIndirectRenderingEnabled, renderingData.CameraData.IsSceneViewInPrefabEditMode, "DEBUG ADDITIONAL", m_FilteringSettingsTransparent.renderQueueRange);
				commandBuffer8.SetGlobalVector(DebugBuffer._DebugOverdrawChannelMask, new Vector4(0f, 1f, 0f, 0f));
				context.ExecuteCommandBuffer(commandBuffer8);
				commandBuffer8.Clear();
				context.DrawRenderers(renderingData.CullResults, ref drawingSettings2, ref m_FilteringSettingsOpaque);
				IndirectRenderingSystem.Instance.DrawPass(ref context, renderingData.CameraData.Camera.cameraType, renderingData.CameraData.IsIndirectRenderingEnabled, renderingData.CameraData.IsSceneViewInPrefabEditMode, "DEBUG ADDITIONAL", m_FilteringSettingsOpaque.renderQueueRange);
				commandBuffer8.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				commandBuffer8.SetGlobalTexture(m_DebugRt.Id, m_DebugRt.Identifier());
				commandBuffer8.SetGlobalInt(DebugBuffer._DebugOverdrawLevel, m_DebugData.OverdrawDebug.ShowOnlyPixelsWithOverdraw);
				commandBuffer8.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("OVERDRAW"), MeshTopology.Triangles, 3);
				commandBuffer8.ReleaseTemporaryRT(m_DebugRt.Id);
			}
			context.ExecuteCommandBuffer(commandBuffer8);
			CommandBufferPool.Release(commandBuffer8);
			break;
		}
		case OverdrawMode.All:
		{
			CommandBuffer commandBuffer7 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer7, m_DebugOverdrawProfilingSampler))
			{
				context.SetupCameraProperties(renderingData.CameraData.Camera);
				commandBuffer7.GetTemporaryRT(m_DebugRt.Id, m_DebugRtDesc, FilterMode.Bilinear);
				commandBuffer7.SetRenderTarget(m_DebugRt.Identifier());
				commandBuffer7.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
				context.ExecuteCommandBuffer(commandBuffer7);
				commandBuffer7.Clear();
				commandBuffer7.SetGlobalVector(DebugBuffer._DebugOverdrawChannelMask, new Vector4(1f, 1f, 1f, 1f));
				context.ExecuteCommandBuffer(commandBuffer7);
				commandBuffer7.Clear();
				DrawingSettings drawingSettings = CreateDrawingSettings(m_DebugAdditionalPassShaderTag, ref renderingData, renderingData.CameraData.DefaultOpaqueSortFlags);
				context.DrawRenderers(renderingData.CullResults, ref drawingSettings, ref m_FilteringSettingsAll);
				IndirectRenderingSystem.Instance.DrawPass(ref context, renderingData.CameraData.Camera.cameraType, renderingData.CameraData.IsIndirectRenderingEnabled, renderingData.CameraData.IsSceneViewInPrefabEditMode, "DEBUG ADDITIONAL", m_FilteringSettingsAll.renderQueueRange);
				commandBuffer7.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				commandBuffer7.SetGlobalTexture(m_DebugRt.Id, m_DebugRt.Identifier());
				commandBuffer7.SetGlobalInt(DebugBuffer._DebugOverdrawLevel, m_DebugData.OverdrawDebug.ShowOnlyPixelsWithOverdraw);
				commandBuffer7.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("OVERDRAW"), MeshTopology.Triangles, 3);
				commandBuffer7.ReleaseTemporaryRT(m_DebugRt.Id);
			}
			context.ExecuteCommandBuffer(commandBuffer7);
			CommandBufferPool.Release(commandBuffer7);
			break;
		}
		}
		switch (m_DebugData.StencilDebug.StencilDebugType)
		{
		case StencilDebugType.Flags:
		{
			CommandBuffer commandBuffer10 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer10, m_ClusterHeatmapProfilingSampler))
			{
				commandBuffer10.GetTemporaryRT(m_DebugRt.Id, m_DebugRtDesc, FilterMode.Bilinear);
				commandBuffer10.SetRenderTarget(m_DebugRt.Identifier(), m_DepthAttachment.Identifier());
				commandBuffer10.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
				commandBuffer10.SetGlobalFloat("_Debug_StecilRef", (float)m_DebugData.StencilDebug.Flags);
				commandBuffer10.SetGlobalFloat("_Debug_StecilReadMask", (float)m_DebugData.StencilDebug.Flags);
				commandBuffer10.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("STENCIL DEBUG"), MeshTopology.Triangles, 3);
				commandBuffer10.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				commandBuffer10.SetGlobalTexture("_Debug_BlitInput", m_DebugRt.Identifier());
				commandBuffer10.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("DEBUG BLIT"), MeshTopology.Triangles, 3);
				commandBuffer10.ReleaseTemporaryRT(m_DebugRt.Id);
			}
			context.ExecuteCommandBuffer(commandBuffer10);
			CommandBufferPool.Release(commandBuffer10);
			break;
		}
		case StencilDebugType.Ref:
		{
			CommandBuffer commandBuffer9 = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer9, m_ClusterHeatmapProfilingSampler))
			{
				commandBuffer9.GetTemporaryRT(m_DebugRt.Id, m_DebugRtDesc, FilterMode.Bilinear);
				commandBuffer9.SetRenderTarget(m_DebugRt.Identifier(), m_DepthAttachment.Identifier());
				commandBuffer9.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
				commandBuffer9.SetGlobalFloat("_Debug_StecilRef", m_DebugData.StencilDebug.Ref);
				commandBuffer9.SetGlobalFloat("_Debug_StecilReadMask", 255f);
				commandBuffer9.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("STENCIL DEBUG"), MeshTopology.Triangles, 3);
				commandBuffer9.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				commandBuffer9.SetGlobalTexture("_Debug_BlitInput", m_DebugRt.Identifier());
				commandBuffer9.DrawProcedural(Matrix4x4.identity, m_DebugMaterial, m_DebugMaterial.FindPass("DEBUG BLIT"), MeshTopology.Triangles, 3);
				commandBuffer9.ReleaseTemporaryRT(m_DebugRt.Id);
			}
			context.ExecuteCommandBuffer(commandBuffer9);
			CommandBufferPool.Release(commandBuffer9);
			break;
		}
		case StencilDebugType.None:
			break;
		}
	}
}
