using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DepthPyramidPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Build Depth Pyramid";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Build Depth Pyramid");

	private ComputeShader m_DepthPyramidShader;

	private int m_DepthPyramidKernel;

	private GBuffer m_GBuffer;

	private int[] m_SrcOffset = new int[4];

	private int[] m_DstOffset = new int[4];

	public DepthPyramidPass(RenderPassEvent evt, ComputeShader depthPyramidShader)
	{
		base.RenderPassEvent = evt;
		m_DepthPyramidShader = depthPyramidShader;
		m_DepthPyramidKernel = m_DepthPyramidShader.FindKernel("DepthPyramid");
	}

	public void Setup(GBuffer gBuffer)
	{
		m_GBuffer = gBuffer;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			for (int i = 1; i < m_GBuffer.DepthPyramidLodCount; i++)
			{
				int4 @int = (int4)m_GBuffer.DepthPyramidMipRects[i];
				int4 int2 = (int4)m_GBuffer.DepthPyramidMipRects[i - 1];
				m_SrcOffset[0] = int2.x;
				m_SrcOffset[1] = int2.y;
				m_SrcOffset[2] = int2.x + int2.z - 1;
				m_SrcOffset[3] = int2.y + int2.w - 1;
				m_DstOffset[0] = @int.x;
				m_DstOffset[1] = @int.y;
				m_DstOffset[2] = 0;
				m_DstOffset[3] = 0;
				commandBuffer.SetComputeIntParams(m_DepthPyramidShader, DepthPyramidBuffer._SrcOffsetAndLimit, m_SrcOffset);
				commandBuffer.SetComputeIntParams(m_DepthPyramidShader, DepthPyramidBuffer._DstOffset, m_DstOffset);
				int2 depthPyramidTextureSize = m_GBuffer.DepthPyramidTextureSize;
				commandBuffer.SetComputeVectorParam(m_DepthPyramidShader, DepthPyramidBuffer._CameraDepthUAVSize, new Vector4(depthPyramidTextureSize.x, depthPyramidTextureSize.y));
				commandBuffer.SetComputeTextureParam(m_DepthPyramidShader, m_DepthPyramidKernel, DepthPyramidBuffer._CameraDepthUAV, m_GBuffer.CameraDepthCopyRt.Identifier());
				commandBuffer.DispatchCompute(m_DepthPyramidShader, m_DepthPyramidKernel, RenderingUtils.DivRoundUp(@int.z, 8), RenderingUtils.DivRoundUp(@int.w, 8), 1);
			}
			commandBuffer.SetGlobalVector(DepthPyramidBuffer._DepthPyramidSamplingRatio, m_GBuffer.DepthPyramidSamplingRatio);
			commandBuffer.SetGlobalVectorArray(DepthPyramidBuffer._DepthPyramidMipRects, m_GBuffer.DepthPyramidMipRects);
			commandBuffer.SetGlobalInt(DepthPyramidBuffer._DepthPyramidLodCount, m_GBuffer.DepthPyramidLodCount);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
