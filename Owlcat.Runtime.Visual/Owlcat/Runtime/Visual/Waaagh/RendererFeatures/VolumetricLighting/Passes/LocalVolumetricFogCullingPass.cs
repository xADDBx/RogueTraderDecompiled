using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class LocalVolumetricFogCullingPass : ScriptableRenderPass<LocalVolumetricFogCullingPassData>
{
	private VolumetricLightingFeature m_Feature;

	private ComputeShaderKernelDescriptor m_BuilFogTilesKernel;

	public override string Name => "LocalVolumetricFogCullingPass";

	public LocalVolumetricFogCullingPass(RenderPassEvent evt, VolumetricLightingFeature feature)
		: base(evt)
	{
		m_Feature = feature;
		m_BuilFogTilesKernel = m_Feature.Shaders.LocalVolumetricFogCullingCS.GetKernelDescriptor("BuildFogTiles");
	}

	protected override void Setup(RenderGraphBuilder builder, LocalVolumetricFogCullingPassData data, ref RenderingData renderingData)
	{
		data.ZBins = m_Feature.ZBins;
		data.VisibleVolumeDataList = m_Feature.VisibleVolumeDataList;
		data.VisibleVolumeBoundsList = m_Feature.VisibleVolumeBoundsList;
		data.VisibleVolumesCount = m_Feature.VisibleVolumesCount;
		m_Feature.VisibleVolumesBoundsBufferHandle = renderingData.RenderGraph.ImportComputeBuffer(m_Feature.VisibleVolumeBoundsBuffer);
		m_Feature.VisibleVolumesDataBufferHandle = renderingData.RenderGraph.ImportComputeBuffer(m_Feature.VisibleVolumeDataBuffer);
		m_Feature.FogTilesBufferHandle = renderingData.RenderGraph.ImportComputeBuffer(m_Feature.FogTilesBuffer);
		m_Feature.ZBinsBufferHandle = renderingData.RenderGraph.ImportComputeBuffer(m_Feature.ZBinsBuffer);
		data.VisibleVolumeDataBuffer = builder.ReadComputeBuffer(in m_Feature.VisibleVolumesDataBufferHandle);
		data.VisibleVolumeBoundsBuffer = builder.ReadComputeBuffer(in m_Feature.VisibleVolumesBoundsBufferHandle);
		data.ZBinsBuffer = builder.ReadComputeBuffer(in m_Feature.ZBinsBufferHandle);
		data.FogTilesBuffer = builder.WriteComputeBuffer(in m_Feature.FogTilesBufferHandle);
		data.FogTilesBufferSize = m_Feature.FogTilesBuffer.count;
		data.Atlas = LocalVolumetricFogManager.Instance.VolumeAtlas;
		data.CullingShader = m_Feature.Shaders.LocalVolumetricFogCullingCS;
		data.BuildFogTilesKernelDesc = m_BuilFogTilesKernel;
		Vector4 fogClusteringParams = m_Feature.FogClusteringParams;
		int x = (int)(fogClusteringParams.x * fogClusteringParams.y);
		data.TileMinMaxZTexture = builder.ReadTexture(in data.Resources.TilesMinMaxZTexture);
		data.DispatchSize = new int3(RenderingUtils.DivRoundUp(x, (int)m_BuilFogTilesKernel.ThreadGroupSize.x), 1, math.max(1, RenderingUtils.DivRoundUp(data.VisibleVolumesCount, 32)));
		data.ClusteringParams = fogClusteringParams;
		data.ScreenProjMatrix = GetScreenProjMatrix(ref renderingData.CameraData);
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

	protected override void Render(LocalVolumetricFogCullingPassData data, RenderGraphContext context)
	{
		data.Atlas.Update(context.cmd);
		context.cmd.SetBufferData(data.VisibleVolumeDataBuffer, data.VisibleVolumeDataList, 0, 0, data.VisibleVolumesCount);
		context.cmd.SetBufferData(data.VisibleVolumeBoundsBuffer, data.VisibleVolumeBoundsList, 0, 0, data.VisibleVolumesCount);
		context.cmd.SetBufferData(data.ZBinsBuffer, data.ZBins.Reinterpret<float4>(Marshal.SizeOf<ZBin>()), 0, 0, 1024);
		context.cmd.SetComputeTextureParam(data.CullingShader, data.BuildFogTilesKernelDesc.Index, ShaderPropertyId._TilesMinMaxZTexture, data.TileMinMaxZTexture);
		context.cmd.SetComputeBufferParam(data.CullingShader, data.BuildFogTilesKernelDesc.Index, ShaderPropertyId._FogTilesBufferUAV, data.FogTilesBuffer);
		context.cmd.SetComputeIntParam(data.CullingShader, ShaderPropertyId._FogTilesBufferUAVSize, data.FogTilesBufferSize);
		context.cmd.SetComputeBufferParam(data.CullingShader, data.BuildFogTilesKernelDesc.Index, ShaderPropertyId._VisibleVolumeBoundsBuffer, data.VisibleVolumeBoundsBuffer);
		context.cmd.SetComputeIntParam(data.CullingShader, ShaderPropertyId._LocalFogVolumesCount, data.VisibleVolumesCount);
		context.cmd.SetComputeVectorParam(data.CullingShader, ShaderPropertyId._LocalVolumetricFogClusteringParams, data.ClusteringParams);
		context.cmd.SetComputeMatrixParam(data.CullingShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
		context.cmd.DispatchCompute(data.CullingShader, data.BuildFogTilesKernelDesc.Index, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
	}
}
