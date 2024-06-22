using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;
using Owlcat.Runtime.Visual.Waaagh;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

internal sealed class Service : IDisposable
{
	public class DebugData
	{
		public List<Frustum> frustums = new List<Frustum>();

		public List<FrustumCastGeometry> castGeometries = new List<FrustumCastGeometry>();

		public Hierarchy<OccluderGeometry> hierarchy;
	}

	private enum ScheduledJobType
	{
		None,
		Build,
		Cast
	}

	[BurstCompile]
	private struct BuildCastGeometryJob : IJob
	{
		public Settings.DynamicTargetMode dynamicTargetMode;

		public float2 defaultTargetSize;

		public float2 defaultDynamicTargetSize;

		public float dynamicDistanceMin;

		public float dynamicDistanceMax;

		public bool targetInsideBoxOccluded;

		[ReadOnly]
		public NativeArray<TargetInfo> targetArray;

		[ReadOnly]
		public NativeArray<CameraInfo> cameraArray;

		[WriteOnly]
		public NativeArray<FrustumCastGeometry> castGeometryArray;

		[WriteOnly]
		public NativeArray<int> castGeometryCountArray;

		public void Execute()
		{
			int value = 0;
			int i = 0;
			for (int length = cameraArray.Length; i < length; i++)
			{
				CameraInfo cameraInfo = cameraArray[i];
				int j = 0;
				for (int length2 = targetArray.Length; j < length2; j++)
				{
					TargetInfo targetInfo = targetArray[j];
					if (IsPointInsideViewFrustum(in cameraInfo.viewProjectionMatrix, in targetInfo.position))
					{
						bool flag = targetInfo.targetInsideBoxOcclusionMode switch
						{
							TargetInsideBoxOcclusionMode.UseSettings => targetInsideBoxOccluded, 
							TargetInsideBoxOcclusionMode.Occluded => true, 
							TargetInsideBoxOcclusionMode.NotOccluded => false, 
							_ => targetInsideBoxOccluded, 
						};
						castGeometryArray[value++] = new FrustumCastGeometry(cameraInfo.viewMatrix, cameraInfo.viewMatrixInverse, targetInfo.position, math.any(targetInfo.size > 0f) ? targetInfo.size : defaultTargetSize, flag, dynamicTargetMode, math.any(targetInfo.dynamicSize > 0f) ? targetInfo.dynamicSize : defaultDynamicTargetSize, dynamicDistanceMin, dynamicDistanceMax);
					}
				}
			}
			castGeometryCountArray[0] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsPointInsideViewFrustum(in float4x4 viewProjectionMatrix, in float3 p)
		{
			float4 @float = math.mul(viewProjectionMatrix, new float4(p.x, p.y, p.z, 1f));
			return math.all(math.abs((@float / @float.w).xyz) <= 1f);
		}
	}

	[BurstCompile]
	private struct InitializeArrayJob<T> : IJob where T : unmanaged
	{
		public T value;

		[WriteOnly]
		public NativeArray<T> array;

		public unsafe void Execute()
		{
			T* unsafeBufferPointerWithoutChecks = (T*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array);
			int i = 0;
			for (int length = array.Length; i < length; i++)
			{
				*(unsafeBufferPointerWithoutChecks++) = value;
			}
		}
	}

	[BurstCompile]
	private struct AnimationJob : IJobParallelFor
	{
		public float currentTimeMinusFadeInDelay;

		public float maxOpacityDelta;

		public NativeArray<float> rendererOpacityArray;

		public NativeArray<uint> rendererDirtyFlagsArray;

		[ReadOnly]
		public NativeArray<float> rendererOccludeTimestampArray;

		public void Execute(int index)
		{
			float num = rendererOpacityArray[index];
			float num2 = rendererOccludeTimestampArray[index];
			float num3 = math.select(0f - maxOpacityDelta, maxOpacityDelta, currentTimeMinusFadeInDelay > num2);
			float num4 = math.saturate(num + num3);
			bool num5 = num != num4;
			rendererOpacityArray[index] = num4;
			if (num5)
			{
				rendererDirtyFlagsArray[index] = 1u;
			}
			else
			{
				rendererDirtyFlagsArray[index] = 0u;
			}
		}
	}

	private const int MaxDepth = 100;

	private const int MaxGeometryPerLeaf = 1;

	private List<ITargetInfoProvider> m_Targets;

	private List<ICameraInfoProvider> m_Cameras;

	private MaterialPropertyBlock m_PropertyBlock;

	private int m_HierarchyDepth;

	private NativeList<Node> m_HierarchyNodeList;

	private NativeArray<BihTreeInfo> m_HierarchyTreeInfoArray;

	private NativeArray<BuildJobDebugInfo> m_HierarchyBuildDebugInfoArray;

	private NativeArray<TargetInfo> m_TargetsArray;

	private NativeArray<CameraInfo> m_CamerasArray;

	private NativeArray<FrustumCastGeometry> m_HierarchyCastGeometryArray;

	private NativeArray<int> m_HierarchyCastGeometryCountArray;

	private NativeArray<IntersectJobDebugData> m_HierarchyCastDebugDataArray;

	private NativeArray<CastJobStackFrame> m_HierarchyCastStackArray;

	private RendererDataRegistry m_GeometryRegistry;

	private ScheduledJobType m_ScheduledJobType;

	private JobHandle m_JobHandle;

	private DebugData m_DebugData = new DebugData();

	private Settings? m_Settings;

	public Service()
	{
		m_GeometryRegistry = new RendererDataRegistry();
		m_TargetsArray = new NativeArray<TargetInfo>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_CamerasArray = new NativeArray<CameraInfo>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_HierarchyTreeInfoArray = new NativeArray<BihTreeInfo>(1, Allocator.Persistent);
		m_HierarchyNodeList = new NativeList<Node>(Allocator.Persistent);
		m_HierarchyBuildDebugInfoArray = new NativeArray<BuildJobDebugInfo>(1, Allocator.Persistent);
		m_HierarchyCastGeometryArray = new NativeArray<FrustumCastGeometry>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_HierarchyCastGeometryCountArray = new NativeArray<int>(1, Allocator.Persistent);
		m_HierarchyCastDebugDataArray = new NativeArray<IntersectJobDebugData>(1, Allocator.Persistent);
		m_HierarchyCastStackArray = new NativeArray<CastJobStackFrame>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_PropertyBlock = new MaterialPropertyBlock();
		m_Targets = new List<ITargetInfoProvider>(16);
		m_Cameras = new List<ICameraInfoProvider>(16);
	}

	public void Dispose()
	{
		if (m_ScheduledJobType != 0)
		{
			m_JobHandle.Complete();
		}
		m_TargetsArray.Dispose();
		m_CamerasArray.Dispose();
		m_HierarchyTreeInfoArray.Dispose();
		m_HierarchyNodeList.Dispose();
		m_HierarchyBuildDebugInfoArray.Dispose();
		m_HierarchyCastGeometryArray.Dispose();
		m_HierarchyCastGeometryCountArray.Dispose();
		m_HierarchyCastDebugDataArray.Dispose();
		m_HierarchyCastStackArray.Dispose();
		m_GeometryRegistry.Dispose();
	}

	public void SetSettings(Settings? settings)
	{
		m_Settings = settings;
	}

	internal DebugData GetDebugData()
	{
		return m_DebugData;
	}

	public void AddOcclusionGeometry(IOcclusionGeometryProvider provider)
	{
		m_GeometryRegistry.AddProvider(provider);
	}

	public void RemoveOcclusionGeometry(IOcclusionGeometryProvider provider)
	{
		m_GeometryRegistry.RemoveProvider(provider);
	}

	public void AddCamera(ICameraInfoProvider camera)
	{
		m_Cameras.Add(camera);
	}

	public void UnregisterCamera(ICameraInfoProvider camera)
	{
		m_Cameras.Remove(camera);
	}

	public void AddTarget(ITargetInfoProvider target)
	{
		m_Targets.Add(target);
	}

	public void RemoveTarget(ITargetInfoProvider target)
	{
		m_Targets.Remove(target);
	}

	public void OnUpdate()
	{
		CompleteJobs();
		ScheduleJobs();
	}

	private void UpdateDebugData()
	{
		if (m_ScheduledJobType == ScheduledJobType.None)
		{
			m_DebugData.hierarchy = new Hierarchy<OccluderGeometry>
			{
				sceneBounds = m_GeometryRegistry.SceneBounds,
				geometryArray = m_GeometryRegistry.OccluderGeometryArray,
				nodeList = m_HierarchyNodeList,
				depth = 0
			};
			m_DebugData.frustums.Clear();
			m_DebugData.castGeometries.Clear();
			int i = 0;
			for (int num = m_HierarchyCastGeometryCountArray[0]; i < num; i++)
			{
				FrustumCastGeometry item = m_HierarchyCastGeometryArray[i];
				m_DebugData.frustums.Add(item.frustumSatGeometry.frustum);
				m_DebugData.castGeometries.Add(item);
			}
		}
	}

	private void TransferOpacity()
	{
		int num = m_GeometryRegistry.RendererArray.Length;
		RendererDataRegistry.RendererData[] rendererArray = m_GeometryRegistry.RendererArray;
		NativeArray<float> rendererOpacityArray = m_GeometryRegistry.RendererOpacityArray;
		NativeArray<uint> rendererDirtyFlagsArray = m_GeometryRegistry.RendererDirtyFlagsArray;
		try
		{
			int i = 0;
			for (int num2 = num; i < num2; i++)
			{
				if (rendererDirtyFlagsArray[i] != 0)
				{
					RendererDataRegistry.RendererData rendererData = rendererArray[i];
					float num3 = rendererOpacityArray[i];
					if ((bool)rendererData.renderer)
					{
						rendererData.renderer.GetPropertyBlock(m_PropertyBlock);
						m_PropertyBlock.SetFloat(ShaderPropertyId._OccluderObjectOpacity, num3);
						rendererData.renderer.SetPropertyBlock(m_PropertyBlock);
					}
					rendererData.proxy?.SetOpacity(num3);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void CompleteJobs()
	{
		switch (m_ScheduledJobType)
		{
		case ScheduledJobType.Build:
			CompleteBuildJobs();
			m_ScheduledJobType = ScheduledJobType.None;
			break;
		case ScheduledJobType.Cast:
			CompleteCastJobs();
			m_ScheduledJobType = ScheduledJobType.None;
			break;
		default:
			m_ScheduledJobType = ScheduledJobType.None;
			break;
		case ScheduledJobType.None:
			break;
		}
	}

	private void CompleteBuildJobs()
	{
		m_JobHandle.Complete();
		UpdateDebugData();
	}

	private void CompleteCastJobs()
	{
		m_JobHandle.Complete();
		if (!m_GeometryRegistry.Changed)
		{
			TransferOpacity();
		}
		UpdateDebugData();
	}

	private void ScheduleJobs()
	{
		if (m_GeometryRegistry.Changed)
		{
			m_JobHandle = ScheduleBuildJob();
			m_JobHandle = ScheduleAnimationJob(m_JobHandle);
			m_ScheduledJobType = ScheduledJobType.Build;
		}
		else
		{
			m_JobHandle = ScheduleCastJob();
			m_JobHandle = ScheduleAnimationJob(m_JobHandle);
			m_ScheduledJobType = ScheduledJobType.Cast;
		}
	}

	private JobHandle ScheduleBuildJob(JobHandle dependsOn = default(JobHandle))
	{
		m_GeometryRegistry.Update();
		return new BuildJob<OccluderGeometry>(100, 1, m_GeometryRegistry.SceneBounds, m_HierarchyNodeList, m_GeometryRegistry.OccluderGeometryArray, m_HierarchyTreeInfoArray, m_HierarchyBuildDebugInfoArray).Schedule(dependsOn);
	}

	private JobHandle ScheduleCastJob(JobHandle dependsOn = default(JobHandle))
	{
		PopulateHierarchyCastData();
		if (m_HierarchyCastGeometryArray.Length == 0)
		{
			return dependsOn;
		}
		Settings settings = m_Settings ?? Settings.Default;
		BuildCastGeometryJob buildCastGeometryJob = default(BuildCastGeometryJob);
		buildCastGeometryJob.cameraArray = m_CamerasArray;
		buildCastGeometryJob.targetArray = m_TargetsArray;
		buildCastGeometryJob.castGeometryArray = m_HierarchyCastGeometryArray;
		buildCastGeometryJob.castGeometryCountArray = m_HierarchyCastGeometryCountArray;
		buildCastGeometryJob.dynamicTargetMode = settings.dynamicTargetSettings.mode;
		buildCastGeometryJob.defaultTargetSize = settings.defaultTargetSize;
		buildCastGeometryJob.defaultDynamicTargetSize = settings.dynamicTargetSettings.targetSize;
		buildCastGeometryJob.dynamicDistanceMin = settings.dynamicTargetSettings.distanceMin;
		buildCastGeometryJob.dynamicDistanceMax = settings.dynamicTargetSettings.distanceMax;
		buildCastGeometryJob.targetInsideBoxOccluded = settings.targetInsideBoxOccluded;
		BuildCastGeometryJob jobData = buildCastGeometryJob;
		CastJob<OccluderGeometry, FrustumCastGeometry, float> castJob = new CastJob<OccluderGeometry, FrustumCastGeometry, float>(m_GeometryRegistry.SceneBounds, m_HierarchyTreeInfoArray[0].depth, m_HierarchyNodeList, m_GeometryRegistry.OccluderGeometryArray, m_HierarchyCastGeometryArray, m_HierarchyCastGeometryCountArray, m_GeometryRegistry.RendererOccludeTimestampArray, m_GeometryRegistry.RendererIndicesArray, Time.unscaledTime, m_HierarchyCastStackArray, default(NativeArray<IntersectJobDebugData>));
		JobHandle dependsOn2 = dependsOn;
		dependsOn2 = jobData.Schedule(dependsOn2);
		return castJob.Schedule(dependsOn2);
	}

	private JobHandle ScheduleAnimationJob(JobHandle dependsOn = default(JobHandle))
	{
		Settings obj = m_Settings ?? Settings.Default;
		float fadeDuration = obj.fadeDuration;
		Settings obj2 = m_Settings ?? Settings.Default;
		float fadeInDelay = obj2.fadeInDelay;
		AnimationJob jobData = default(AnimationJob);
		jobData.maxOpacityDelta = Time.unscaledDeltaTime / fadeDuration;
		jobData.currentTimeMinusFadeInDelay = Time.unscaledTime - fadeInDelay;
		jobData.rendererOpacityArray = m_GeometryRegistry.RendererOpacityArray;
		jobData.rendererDirtyFlagsArray = m_GeometryRegistry.RendererDirtyFlagsArray;
		jobData.rendererOccludeTimestampArray = m_GeometryRegistry.RendererOccludeTimestampArray;
		return IJobParallelForExtensions.Schedule(jobData, m_GeometryRegistry.RendererOpacityArray.Length, 128, dependsOn);
	}

	private void PopulateHierarchyCastData()
	{
		int num = m_Targets.Count * m_Cameras.Count;
		int depth = m_HierarchyTreeInfoArray[0].depth;
		EnsureSize(ref m_HierarchyCastGeometryArray, num);
		EnsureSize(ref m_TargetsArray, m_Targets.Count);
		EnsureSize(ref m_CamerasArray, m_Cameras.Count);
		EnsureSize(ref m_HierarchyCastStackArray, num * CastJobUtility.EvaluateFrameStackSize(depth));
		int i = 0;
		for (int count = m_Targets.Count; i < count; i++)
		{
			m_TargetsArray[i] = m_Targets[i].TargetInfo;
		}
		int j = 0;
		for (int count2 = m_Cameras.Count; j < count2; j++)
		{
			m_CamerasArray[j] = m_Cameras[j].CameraInfo;
		}
	}

	private static void EnsureSize<T>(ref NativeArray<T> array, int size, Allocator allocator = Allocator.Persistent, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory) where T : unmanaged
	{
		if (array.Length != size)
		{
			array.Dispose();
			array = new NativeArray<T>(size, allocator, options);
		}
	}

	private static void EnsureSize<T>(ref T[] array, int size)
	{
		if (array.Length != size)
		{
			array = new T[size];
		}
	}
}
