using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class CopyDepthPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Copy Depth Buffer";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Copy Depth Buffer");

	private RenderTargetHandle m_Source;

	private RenderTargetHandle m_Dest;

	private RenderTargetIdentifier m_DestIdentifier;

	private bool m_CopyHandle;

	private RenderTextureDescriptor m_Desc;

	private Material m_CopyDepthMat;

	private Vector4 m_DepthPyramidSamplingRatio;

	public CopyDepthPass(RenderPassEvent evt, Material copyDepthMat)
	{
		base.RenderPassEvent = evt;
		m_CopyDepthMat = copyDepthMat;
	}

	public void Setup(RenderTargetHandle source, RenderTargetHandle dest, RenderTextureDescriptor desc, Vector4 depthPyramidSamplingRatio)
	{
		m_Source = source;
		m_Dest = dest;
		m_DestIdentifier = m_Dest.Identifier();
		m_CopyHandle = true;
		m_Desc = desc;
		m_DepthPyramidSamplingRatio = depthPyramidSamplingRatio;
	}

	public void Setup(RenderTargetHandle source, RenderTargetIdentifier dest, RenderTextureDescriptor desc, Vector4 depthPyramidSamplingRatio)
	{
		m_Source = source;
		m_DestIdentifier = dest;
		m_CopyHandle = false;
		m_Desc = desc;
		m_DepthPyramidSamplingRatio = depthPyramidSamplingRatio;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetGlobalTexture(m_Source.Id, m_Source.Identifier());
			commandBuffer.SetRenderTarget(m_DestIdentifier);
			commandBuffer.DrawProcedural(Matrix4x4.identity, m_CopyDepthMat, 0, MeshTopology.Triangles, 3);
			commandBuffer.SetGlobalVector(DepthPyramidBuffer._DepthPyramidSamplingRatio, m_DepthPyramidSamplingRatio);
		}
		if (m_CopyHandle)
		{
			commandBuffer.SetGlobalTexture(m_Source.Id, m_DestIdentifier);
			commandBuffer.SetGlobalTexture(CommonTextureId.Unity_CameraDepthTexture, m_DestIdentifier);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
