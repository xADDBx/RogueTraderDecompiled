using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class LightCullingPass : ScriptableRenderPass<LightCullingPassData>
{
	private ComputeShader m_LightCullingShader;

	private ComputeShaderKernelDescriptor m_BuilLightTilesKernel;

	private WaaaghLights m_WaaaghLights;

	private int3 m_DispatchSize;

	public override string Name => "LightCullingPass";

	public LightCullingPass(RenderPassEvent evt, ComputeShader lightCullingShader, WaaaghLights waaaghLights)
		: base(evt)
	{
		m_LightCullingShader = lightCullingShader;
		m_BuilLightTilesKernel = lightCullingShader.GetKernelDescriptor("BuildLightTiles");
		m_WaaaghLights = waaaghLights;
	}

	protected override void Setup(RenderGraphBuilder builder, LightCullingPassData data, ref RenderingData renderingData)
	{
		data.LightCullingShader = m_LightCullingShader;
		data.BuildLightTilesKernel = m_BuilLightTilesKernel.Index;
		data.LightTilesBuffer = builder.WriteComputeBuffer(in data.Resources.LightTilesBuffer);
		Vector4 clusteringParams = m_WaaaghLights.ClusteringParams;
		Vector4 lightDataParams = m_WaaaghLights.LightDataParams;
		int x = (int)(clusteringParams.x * clusteringParams.y);
		int x2 = (int)lightDataParams.z;
		data.DispatchSize = new int3(RenderingUtils.DivRoundUp(x, (int)m_BuilLightTilesKernel.ThreadGroupSize.x), 1, math.max(1, RenderingUtils.DivRoundUp(x2, 32)));
		data.ScreenProjMatrix = GetScreenProjMatrix(ref renderingData.CameraData);
		data.LightTilesBufferSize = m_WaaaghLights.LightTilesBuffer.count;
		data.TilesMinMaxZTexture = builder.ReadTexture(in data.Resources.TilesMinMaxZTexture);
	}

	private Matrix4x4 GetScreenProjMatrix(ref CameraData cameraData)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		float num = cameraData.CameraTargetDescriptor.width;
		float num2 = cameraData.CameraTargetDescriptor.height;
		matrix4x.SetRow(0, new Vector4(0.5f * num, 0f, 0f, 0.5f * num));
		matrix4x.SetRow(1, new Vector4(0f, 0.5f * num2, 0f, 0.5f * num2));
		matrix4x.SetRow(2, new Vector4(0f, 0f, 0.5f, 0.5f));
		matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		return matrix4x * cameraData.GetProjectionMatrix();
	}

	protected override void Render(LightCullingPassData data, RenderGraphContext context)
	{
		context.cmd.SetComputeMatrixParam(data.LightCullingShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
		context.cmd.SetComputeBufferParam(data.LightCullingShader, data.BuildLightTilesKernel, ShaderPropertyId._LightTilesBufferUAV, data.LightTilesBuffer);
		context.cmd.SetComputeIntParam(data.LightCullingShader, ShaderPropertyId._LightTilesBufferUAVSize, data.LightTilesBufferSize);
		context.cmd.SetComputeTextureParam(data.LightCullingShader, data.BuildLightTilesKernel, ShaderPropertyId._TilesMinMaxZTexture, data.TilesMinMaxZTexture);
		context.cmd.DispatchCompute(data.LightCullingShader, data.BuildLightTilesKernel, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._LightTilesBuffer, data.LightTilesBuffer);
	}
}
