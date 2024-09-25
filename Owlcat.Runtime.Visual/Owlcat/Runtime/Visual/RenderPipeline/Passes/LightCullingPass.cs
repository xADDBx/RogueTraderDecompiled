using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class LightCullingPass : ScriptableRenderPass
{
	private const string kLightCullingTag = "Light Culling";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Light Culling");

	private ComputeShader m_LightCullingShader;

	private int m_BuildLightTilesKernel;

	private ClusteredLights m_ClusteredLights;

	public LightCullingPass(RenderPassEvent evt, ComputeShader lightCullingShader)
	{
		base.RenderPassEvent = evt;
		m_LightCullingShader = lightCullingShader;
		m_BuildLightTilesKernel = m_LightCullingShader.FindKernel("BuildLightTiles");
	}

	public void Setup(ClusteredLights clusteredLights)
	{
		m_ClusteredLights = clusteredLights;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetGlobalConstantBuffer(m_ClusteredLights.LightDataConstantBuffer, ComputeBufferId.LightDataCB, 0, m_ClusteredLights.LightDataConstantBuffer.count * 4 * 4);
			commandBuffer.SetGlobalConstantBuffer(m_ClusteredLights.LightVolumeDataConstantBuffer, ComputeBufferId.LightVolumeDataCB, 0, m_ClusteredLights.LightVolumeDataConstantBuffer.count * 4 * 4);
			commandBuffer.SetGlobalConstantBuffer(m_ClusteredLights.ZBinsConstantBuffer, ComputeBufferId.ZBinsCB, 0, m_ClusteredLights.ZBinsConstantBuffer.count * 4 * 4);
			commandBuffer.SetGlobalVector(CameraBuffer._ClusteringParams, m_ClusteredLights.ClusteringParams);
			commandBuffer.SetGlobalVector(CameraBuffer._LightDataParams, m_ClusteredLights.LightDataParams);
			commandBuffer.SetComputeMatrixParam(m_LightCullingShader, CameraBuffer._ScreenProjMatrix, GetScreenProjMatrix(renderingData.CameraData.Camera));
			commandBuffer.SetComputeBufferParam(m_LightCullingShader, m_BuildLightTilesKernel, ComputeBufferId._LightTilesBufferUAV, m_ClusteredLights.LightTilesBuffer);
			commandBuffer.SetComputeIntParam(m_LightCullingShader, ComputeBufferId._LightTilesBufferUAVSize, m_ClusteredLights.LightTilesBuffer.count);
			int x = (int)m_ClusteredLights.LightDataParams.z;
			int threadGroupsX = RenderingUtils.DivRoundUp((int)(m_ClusteredLights.ClusteringParams.x * m_ClusteredLights.ClusteringParams.y), 64);
			int threadGroupsY = 1;
			int threadGroupsZ = math.max(1, RenderingUtils.DivRoundUp(x, 32));
			commandBuffer.DispatchCompute(m_LightCullingShader, m_BuildLightTilesKernel, threadGroupsX, threadGroupsY, threadGroupsZ);
			commandBuffer.SetGlobalBuffer(ComputeBufferId._LightTilesBuffer, m_ClusteredLights.LightTilesBuffer);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private Matrix4x4 GetScreenProjMatrix(Camera camera)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		float num = camera.pixelWidth;
		float num2 = camera.pixelHeight;
		matrix4x.SetRow(0, new Vector4(0.5f * num, 0f, 0f, 0.5f * num));
		matrix4x.SetRow(1, new Vector4(0f, 0.5f * num2, 0f, 0.5f * num2));
		matrix4x.SetRow(2, new Vector4(0f, 0f, 0.5f, 0.5f));
		matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		return matrix4x * camera.projectionMatrix;
	}
}
