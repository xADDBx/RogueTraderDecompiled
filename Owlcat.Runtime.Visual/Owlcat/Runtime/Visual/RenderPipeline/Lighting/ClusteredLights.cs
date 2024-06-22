using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.RenderPipeline.Shadows;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

public class ClusteredLights
{
	public const int kMaxVisibleLights = 1024;

	public const int kMaxZBinsCount = 4096;

	private ClusteredShadows m_ClusteredShadows = new ClusteredShadows();

	private ComputeBuffer m_LightDataConstantBuffer;

	private ComputeBuffer m_LightVolumeDataConstantBuffer;

	private ComputeBuffer m_ZBinsConstantBuffer;

	private ComputeBuffer m_LightTilesBuffer;

	private NativeArray<float4> m_LightDataRaw;

	private NativeArray<float4> m_LightVolumeDataRaw;

	private NativeArray<LightDescriptor> m_LightDescs;

	private NativeArray<ZBin> m_ZBins;

	private JobHandle m_SetupJobsHandle;

	private Vector4 m_ClusteringParams;

	private Vector4 m_LightDataParams;

	public bool ShadowmaskEnabled { get; private set; }

	public ClusteredShadows ClusteredShadows => m_ClusteredShadows;

	public ComputeBuffer LightDataConstantBuffer => m_LightDataConstantBuffer;

	public ComputeBuffer LightVolumeDataConstantBuffer => m_LightVolumeDataConstantBuffer;

	public ComputeBuffer ZBinsConstantBuffer => m_ZBinsConstantBuffer;

	public ComputeBuffer LightTilesBuffer => m_LightTilesBuffer;

	public Vector4 ClusteringParams => m_ClusteringParams;

	public Vector4 LightDataParams => m_LightDataParams;

	public ClusteredLights()
	{
		m_LightDataConstantBuffer = new ComputeBuffer(4096, Marshal.SizeOf<float4>(), ComputeBufferType.Constant);
		m_LightDataConstantBuffer.name = "LightDataCB";
		m_LightVolumeDataConstantBuffer = new ComputeBuffer(3072, Marshal.SizeOf<float4>(), ComputeBufferType.Constant);
		m_LightVolumeDataConstantBuffer.name = "LightVolumeDataCB";
		m_ZBinsConstantBuffer = new ComputeBuffer(1024, Marshal.SizeOf<float4>(), ComputeBufferType.Constant);
		m_ZBinsConstantBuffer.name = "ZBinsCB";
		m_LightDataRaw = new NativeArray<float4>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LightVolumeDataRaw = new NativeArray<float4>(3072, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_ZBins = new NativeArray<ZBin>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public void StartSetupJobs(ref RenderingData renderingData, TileSize tileSize)
	{
		Camera camera = renderingData.CameraData.Camera;
		m_ClusteredShadows.Setup(ref renderingData);
		ShadowmaskEnabled = InitializeSingleThreadedParameters(ref renderingData);
		InitLightTilesBuffer(ref renderingData, tileSize);
		if (ShadowmaskEnabled)
		{
			renderingData.PerObjectData |= PerObjectData.ShadowMask;
		}
		ref NativeArray<VisibleLight> visibleLights = ref renderingData.LightData.VisibleLights;
		int length = visibleLights.Length;
		int num = math.min(length, 1024);
		int num2 = 0;
		for (int i = 0; i < length && visibleLights[i].lightType == LightType.Directional; i++)
		{
			num2++;
		}
		m_LightDataParams = new Vector4(num2, num - num2, num);
		Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * camera.worldToCameraMatrix;
		float w = m_ClusteringParams.w;
		NativeArray<float> meanZ = new NativeArray<float>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> indices = new NativeArray<int>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<LightDescriptor> nativeArray = new NativeArray<LightDescriptor>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		MinMaxZJob jobData = default(MinMaxZJob);
		jobData.WorldToViewMatrix = matrix4x;
		jobData.LightDescriptors = m_LightDescs;
		jobData.MeanZ = meanZ;
		JobHandle dependsOn = IJobForExtensions.ScheduleParallel(jobData, length, 32, default(JobHandle));
		RadixSortJob jobData2 = default(RadixSortJob);
		jobData2.Keys = meanZ.Reinterpret<int>();
		jobData2.Indices = indices;
		JobHandle dependency = jobData2.Schedule(dependsOn);
		ReorderJob<LightDescriptor> jobData3 = default(ReorderJob<LightDescriptor>);
		jobData3.Indices = indices;
		jobData3.Input = m_LightDescs;
		jobData3.Output = nativeArray;
		JobHandle dependsOn2 = IJobForExtensions.ScheduleParallel(jobData3, length, 32, dependency);
		CopyArrayJob<LightDescriptor> jobData4 = default(CopyArrayJob<LightDescriptor>);
		jobData4.Count = length;
		jobData4.Input = nativeArray;
		jobData4.Output = m_LightDescs;
		JobHandle dependency2 = jobData4.Schedule(dependsOn2);
		ExtractLightDataJob jobData5 = default(ExtractLightDataJob);
		jobData5.WorldToViewMatrix = matrix4x;
		jobData5.LightDescriptors = m_LightDescs;
		jobData5.LightData = m_LightDataRaw;
		jobData5.LightVolumeData = m_LightVolumeDataRaw;
		JobHandle dependency3 = IJobForExtensions.ScheduleParallel(jobData5, num, 32, dependency2);
		ZBinningJob zBinningJob = default(ZBinningJob);
		zBinningJob.ZBinFactor = w;
		zBinningJob.CameraNearClip = camera.nearClipPlane;
		zBinningJob.LightCount = length;
		zBinningJob.DirectionalLightCount = num2;
		zBinningJob.Lights = m_LightDescs;
		zBinningJob.ZBins = m_ZBins;
		ZBinningJob jobData6 = zBinningJob;
		m_SetupJobsHandle = IJobForExtensions.ScheduleParallel(jobData6, 64, 1, dependency3);
		meanZ.Dispose(m_SetupJobsHandle);
		indices.Dispose(m_SetupJobsHandle);
		nativeArray.Dispose(m_SetupJobsHandle);
	}

	private void InitLightTilesBuffer(ref RenderingData renderingData, TileSize tileSize)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref renderingData.CameraData.CameraTargetDescriptor;
		int2 @int = 1;
		@int.x = RenderingUtils.DivRoundUp(cameraTargetDescriptor.width, (int)tileSize);
		@int.y = RenderingUtils.DivRoundUp(cameraTargetDescriptor.height, (int)tileSize);
		int num = 32;
		int num2 = @int.x * @int.y * num;
		if (m_LightTilesBuffer == null || !m_LightTilesBuffer.IsValid() || m_LightTilesBuffer.count != num2)
		{
			if (m_LightTilesBuffer != null)
			{
				m_LightTilesBuffer.Release();
			}
			if (num2 > 0)
			{
				m_LightTilesBuffer = new ComputeBuffer(num2, Marshal.SizeOf<uint>());
				m_LightTilesBuffer.name = "_LightTilesBuffer";
			}
		}
		Camera camera = renderingData.CameraData.Camera;
		float w = 4096f / (camera.farClipPlane - camera.nearClipPlane);
		m_ClusteringParams = new Vector4(@int.x, @int.y, (float)tileSize, w);
	}

	public void CompleteSetupJobs()
	{
		m_SetupJobsHandle.Complete();
		m_LightDataConstantBuffer.SetData(m_LightDataRaw);
		m_LightVolumeDataConstantBuffer.SetData(m_LightVolumeDataRaw);
		m_ZBinsConstantBuffer.SetData(m_ZBins.Reinterpret<float4>(Marshal.SizeOf<ZBin>()), 0, 0, 1024);
	}

	private bool InitializeSingleThreadedParameters(ref RenderingData renderingData)
	{
		Span<VisibleLight> span = renderingData.LightData.VisibleLights.AsSpan();
		int length = span.Length;
		if (!m_LightDescs.IsCreated || m_LightDescs.Length < length)
		{
			if (m_LightDescs.IsCreated)
			{
				m_LightDescs.Dispose();
			}
			m_LightDescs = new NativeArray<LightDescriptor>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		bool flag = false;
		for (int i = 0; i < length; i++)
		{
			ref VisibleLight reference = ref span[i];
			VisibleLight visibleLight = reference;
			Light light = visibleLight.light;
			LightDescriptor value = default(LightDescriptor);
			value.VisibleLight = reference;
			if (light != null)
			{
				LightBakingOutput bakingOutput = light.bakingOutput;
				if (light.TryGetComponent<OwlcatAdditionalLightData>(out var component))
				{
					value.SnapSpecularToInnerRadius = component.SnapSperularToInnerRadius;
					value.LightFalloffType = component.FalloffType;
					value.InnerRadius = component.InnerRadius;
				}
				value.InnerSpotAngle = light.innerSpotAngle;
				value.IsBaked = bakingOutput.lightmapBakeType == LightmapBakeType.Baked;
				if (m_ClusteredShadows.ShadowIndicesMap.TryGetValue(i, out var value2))
				{
					value.ShadowDataIndex = value2;
				}
				else
				{
					value.ShadowDataIndex = -1;
				}
				bool flag2 = RenderingUtils.IsBakedShadowMaskLight(in bakingOutput);
				if (flag2)
				{
					value.ShadowmaskChannel = bakingOutput.occlusionMaskChannel;
				}
				else
				{
					value.ShadowmaskChannel = -1;
				}
				flag = flag || flag2;
				value.ShadowStrength = light.shadowStrength;
			}
			else
			{
				value.ShadowStrength = 1f;
				value.InnerSpotAngle = -1f;
				value.ShadowDataIndex = -1;
			}
			m_LightDescs[i] = value;
		}
		return flag;
	}

	public void Dispose()
	{
		if (m_LightDataRaw.IsCreated)
		{
			m_LightDataRaw.Dispose();
		}
		if (m_LightVolumeDataRaw.IsCreated)
		{
			m_LightVolumeDataRaw.Dispose();
		}
		if (m_LightDescs.IsCreated)
		{
			m_LightDescs.Dispose();
		}
		if (m_ZBins.IsCreated)
		{
			m_ZBins.Dispose();
		}
		ReleaseBuffers();
		m_ClusteredShadows.Dispose();
	}

	private void ReleaseBuffers()
	{
		if (m_LightDataConstantBuffer != null)
		{
			m_LightDataConstantBuffer.Release();
		}
		if (m_LightVolumeDataConstantBuffer != null)
		{
			m_LightVolumeDataConstantBuffer.Release();
		}
		if (m_ZBinsConstantBuffer != null)
		{
			m_ZBinsConstantBuffer.Release();
		}
		if (m_LightTilesBuffer != null)
		{
			m_LightTilesBuffer.Release();
		}
	}
}
