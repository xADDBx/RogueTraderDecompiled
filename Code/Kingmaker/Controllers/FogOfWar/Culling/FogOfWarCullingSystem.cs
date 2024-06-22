using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Kingmaker.Controllers.FogOfWar.Culling;

internal sealed class FogOfWarCullingSystem : IDisposable
{
	private sealed class GameScene
	{
		public readonly List<IBlocker> StaticBlockers = new List<IBlocker>();

		public readonly List<IBlocker> DynamicBlockers = new List<IBlocker>();

		public readonly List<ITarget> Targets = new List<ITarget>();

		public bool StaticBlockersChanged;

		public bool TargetsChanged;

		public readonly List<ITarget> TargetsWithChangedProperties = new List<ITarget>();

		public void RegisterStaticBlocker(IBlocker blocker)
		{
			blocker.CullingRegistryIndex = StaticBlockers.Count;
			StaticBlockers.Add(blocker);
			StaticBlockersChanged = true;
		}

		public void UnregisterStaticBlocker(IBlocker blocker)
		{
			int cullingRegistryIndex = blocker.CullingRegistryIndex;
			int num = StaticBlockers.Count - 1;
			blocker.CullingRegistryIndex = -1;
			if (cullingRegistryIndex != num)
			{
				StaticBlockers[cullingRegistryIndex] = StaticBlockers[num];
				StaticBlockers[cullingRegistryIndex].CullingRegistryIndex = cullingRegistryIndex;
			}
			StaticBlockers.RemoveAt(num);
			StaticBlockersChanged = true;
		}

		public void RegisterDynamicBlocker(IBlocker blocker)
		{
			blocker.CullingRegistryIndex = DynamicBlockers.Count;
			DynamicBlockers.Add(blocker);
		}

		public void UnregisterDynamicBlocker(IBlocker blocker)
		{
			int cullingRegistryIndex = blocker.CullingRegistryIndex;
			int num = DynamicBlockers.Count - 1;
			blocker.CullingRegistryIndex = -1;
			if (cullingRegistryIndex != num)
			{
				DynamicBlockers[cullingRegistryIndex] = DynamicBlockers[num];
				DynamicBlockers[cullingRegistryIndex].CullingRegistryIndex = cullingRegistryIndex;
			}
			DynamicBlockers.RemoveAt(num);
		}

		public void RegisterTarget(ITarget target)
		{
			target.RegistryIndex = Targets.Count;
			Targets.Add(target);
			MarkTargetsAsChanged();
		}

		public void UnregisterTarget(ITarget target)
		{
			int registryIndex = target.RegistryIndex;
			int num = Targets.Count - 1;
			target.RegistryIndex = -1;
			if (registryIndex != num)
			{
				Targets[registryIndex] = Targets[num];
				Targets[registryIndex].RegistryIndex = registryIndex;
			}
			Targets.RemoveAt(num);
			MarkTargetsAsChanged();
		}

		public void MarkTargetsAsChanged()
		{
			if (!TargetsChanged)
			{
				TargetsChanged = true;
				TargetsWithChangedProperties.Clear();
			}
		}

		public void MarkTargetAsChangedProperties(ITarget target)
		{
			if (!TargetsChanged)
			{
				TargetsWithChangedProperties.Add(target);
			}
		}

		public void ResetChanges()
		{
			StaticBlockersChanged = false;
			TargetsChanged = false;
			TargetsWithChangedProperties.Clear();
		}
	}

	private sealed class JobScene : IDisposable
	{
		public readonly List<ITarget> Targets = new List<ITarget>();

		public NativeList<TargetProperties> TargetProperties;

		public NativeList<bool> TargetRevealedStates;

		public NativeList<bool> TargetForceRevealStates;

		public NativeList<RevealerProperties> RevealerProperties;

		public NativeList<BlockerSegment> StaticBlockerSegments;

		public NativeList<BlockerSegment> DynamicBlockerSegments;

		public JobScene(Allocator allocator)
		{
			TargetProperties = new NativeList<TargetProperties>(allocator);
			TargetRevealedStates = new NativeList<bool>(allocator);
			TargetForceRevealStates = new NativeList<bool>(allocator);
			RevealerProperties = new NativeList<RevealerProperties>(allocator);
			StaticBlockerSegments = new NativeList<BlockerSegment>(allocator);
			DynamicBlockerSegments = new NativeList<BlockerSegment>(allocator);
		}

		public void Dispose()
		{
			TargetProperties.Dispose();
			TargetRevealedStates.Dispose();
			TargetForceRevealStates.Dispose();
			RevealerProperties.Dispose();
			StaticBlockerSegments.Dispose();
			DynamicBlockerSegments.Dispose();
		}

		public void Update(GameScene gameScene, in NativeList<RevealerProperties> revealers)
		{
			if (gameScene.StaticBlockersChanged)
			{
				StaticBlockerSegments.Clear();
				foreach (IBlocker staticBlocker in gameScene.StaticBlockers)
				{
					staticBlocker.AppendCullingSegments(StaticBlockerSegments);
				}
			}
			DynamicBlockerSegments.Clear();
			foreach (IBlocker dynamicBlocker in gameScene.DynamicBlockers)
			{
				dynamicBlocker.AppendCullingSegments(DynamicBlockerSegments);
			}
			RevealerProperties.CopyFrom(in revealers);
			if (gameScene.TargetsChanged)
			{
				gameScene.Targets.Sort((ITarget x, ITarget y) => string.CompareOrdinal(x.SortOrder, y.SortOrder));
				for (int i = 0; i < gameScene.Targets.Count; i++)
				{
					gameScene.Targets[i].RegistryIndex = i;
				}
				UpdateAllTargets(gameScene.Targets);
			}
			else
			{
				UpdateTargetProperties(gameScene.TargetsWithChangedProperties);
				UpdateTargetForceRevealStates();
			}
		}

		private unsafe void UpdateAllTargets(List<ITarget> targets)
		{
			Targets.Clear();
			Targets.AddRange(targets);
			TargetProperties.Resize(Targets.Count, NativeArrayOptions.UninitializedMemory);
			TargetRevealedStates.Resize(Targets.Count, NativeArrayOptions.UninitializedMemory);
			TargetForceRevealStates.Resize(Targets.Count, NativeArrayOptions.UninitializedMemory);
			TargetProperties* ptr = TargetProperties.GetUnsafePtr();
			bool* ptr2 = TargetRevealedStates.GetUnsafePtr();
			bool* ptr3 = TargetForceRevealStates.GetUnsafePtr();
			foreach (ITarget target in Targets)
			{
				*ptr = target.Properties;
				*ptr2 = target.Revealed;
				*ptr3 = target.ForceReveal;
				ptr++;
				ptr2++;
				ptr3++;
			}
		}

		private unsafe void UpdateTargetProperties(List<ITarget> targets)
		{
			TargetProperties* unsafePtr = TargetProperties.GetUnsafePtr();
			foreach (ITarget target in targets)
			{
				int registryIndex = target.RegistryIndex;
				unsafePtr[registryIndex] = Targets[registryIndex].Properties;
			}
		}

		private unsafe void UpdateTargetForceRevealStates()
		{
			bool* ptr = TargetForceRevealStates.GetUnsafePtr();
			foreach (ITarget target in Targets)
			{
				*ptr = target.ForceReveal;
				ptr++;
			}
		}
	}

	private sealed class JobResources : IDisposable
	{
		public NativeList<float4> StaticBlockerPlanes;

		public NativeList<float4> DynamicBlockerPlanes;

		public NativeList<int> StaticBlockerPlaneSetCounts;

		public NativeList<BlockerPlaneSet> StaticBlockerPlaneSets;

		public NativeList<int> DynamicBlockerPlaneSetCounts;

		public NativeList<BlockerPlaneSet> DynamicBlockerPlaneSets;

		public NativeList<ushort> NotifyTargetIndices;

		public JobResources(Allocator allocator)
		{
			StaticBlockerPlanes = new NativeList<float4>(allocator);
			DynamicBlockerPlanes = new NativeList<float4>(allocator);
			StaticBlockerPlaneSetCounts = new NativeList<int>(allocator);
			StaticBlockerPlaneSets = new NativeList<BlockerPlaneSet>(allocator);
			DynamicBlockerPlaneSetCounts = new NativeList<int>(allocator);
			DynamicBlockerPlaneSets = new NativeList<BlockerPlaneSet>(allocator);
			NotifyTargetIndices = new NativeList<ushort>(allocator);
		}

		public void Dispose()
		{
			StaticBlockerPlanes.Dispose();
			DynamicBlockerPlanes.Dispose();
			StaticBlockerPlaneSetCounts.Dispose();
			StaticBlockerPlaneSets.Dispose();
			DynamicBlockerPlaneSetCounts.Dispose();
			DynamicBlockerPlaneSets.Dispose();
			NotifyTargetIndices.Dispose();
		}
	}

	private readonly GameScene m_GameScene;

	private readonly JobScene m_JobScene;

	private readonly JobResources m_JobResources;

	private JobHandle? m_ScheduledJobHandle;

	public FogOfWarCullingSystem(Allocator allocator = Allocator.Persistent)
	{
		m_GameScene = new GameScene();
		m_JobScene = new JobScene(allocator);
		m_JobResources = new JobResources(allocator);
	}

	public void Dispose()
	{
		if (m_ScheduledJobHandle.HasValue)
		{
			m_ScheduledJobHandle.Value.Complete();
			m_ScheduledJobHandle = null;
		}
		m_JobResources.Dispose();
		m_JobScene.Dispose();
	}

	public void RegisterStaticBlocker(IBlocker blocker)
	{
		m_GameScene.RegisterStaticBlocker(blocker);
	}

	public void UnregisterStaticBlocker(IBlocker blocker)
	{
		m_GameScene.UnregisterStaticBlocker(blocker);
	}

	public void RegisterDynamicBlocker(IBlocker blocker)
	{
		m_GameScene.RegisterDynamicBlocker(blocker);
	}

	public void UnregisterDynamicBlocker(IBlocker blocker)
	{
		m_GameScene.UnregisterDynamicBlocker(blocker);
	}

	public void RegisterTarget(ITarget target)
	{
		m_GameScene.RegisterTarget(target);
		TryEarlyClearJobSceneTargets();
	}

	public void UnregisterTarget(ITarget target)
	{
		m_GameScene.UnregisterTarget(target);
		TryEarlyClearJobSceneTargets();
	}

	public void UpdateTarget(ITarget target)
	{
		m_GameScene.MarkTargetAsChangedProperties(target);
	}

	public void ScheduleUpdate(in NativeList<RevealerProperties> revealers)
	{
		CompleteUpdate(applyCullingResults: false);
		try
		{
			m_JobScene.Update(m_GameScene, in revealers);
			ScheduleJobs();
		}
		finally
		{
			m_GameScene.ResetChanges();
		}
	}

	public void CompleteUpdate(bool applyCullingResults)
	{
		if (!m_ScheduledJobHandle.HasValue)
		{
			return;
		}
		m_ScheduledJobHandle.Value.Complete();
		try
		{
			if (!applyCullingResults)
			{
				return;
			}
			foreach (ushort notifyTargetIndex in m_JobResources.NotifyTargetIndices)
			{
				m_JobScene.Targets[notifyTargetIndex].Revealed = m_JobScene.TargetRevealedStates[notifyTargetIndex];
			}
		}
		finally
		{
			m_ScheduledJobHandle = null;
			TryEarlyClearJobSceneTargets();
		}
	}

	private void TryEarlyClearJobSceneTargets()
	{
		if (!m_ScheduledJobHandle.HasValue && m_GameScene.TargetsChanged)
		{
			m_JobScene.Targets.Clear();
		}
	}

	private void ScheduleJobs()
	{
		int length = m_JobScene.RevealerProperties.Length;
		int length2 = m_JobScene.TargetProperties.Length;
		int length3 = m_JobScene.StaticBlockerSegments.Length;
		int length4 = m_JobScene.DynamicBlockerSegments.Length;
		if (m_GameScene.StaticBlockersChanged)
		{
			m_JobResources.StaticBlockerPlanes.Resize(length3, NativeArrayOptions.UninitializedMemory);
		}
		m_JobResources.StaticBlockerPlaneSetCounts.Resize(length, NativeArrayOptions.UninitializedMemory);
		m_JobResources.StaticBlockerPlaneSets.Resize(length * length3, NativeArrayOptions.UninitializedMemory);
		m_JobResources.DynamicBlockerPlanes.Resize(length4, NativeArrayOptions.UninitializedMemory);
		m_JobResources.DynamicBlockerPlaneSetCounts.Resize(length, NativeArrayOptions.UninitializedMemory);
		m_JobResources.DynamicBlockerPlaneSets.Resize(length * length4, NativeArrayOptions.UninitializedMemory);
		m_JobResources.NotifyTargetIndices.Clear();
		if (m_JobResources.NotifyTargetIndices.Capacity < length2)
		{
			m_JobResources.NotifyTargetIndices.SetCapacity(length2);
		}
		JobHandle dependsOn;
		BuildBlockerPlanesJob jobData;
		if (m_GameScene.StaticBlockersChanged)
		{
			jobData = default(BuildBlockerPlanesJob);
			jobData.BlockerSegments = m_JobScene.StaticBlockerSegments.AsArray();
			jobData.BlockerPlanes = m_JobResources.StaticBlockerPlanes.AsArray();
			dependsOn = IJobParallelForExtensions.Schedule(jobData, length3, 128);
		}
		else
		{
			dependsOn = default(JobHandle);
		}
		BuildBlockerPlaneSetsJob jobData2 = default(BuildBlockerPlaneSetsJob);
		jobData2.Revealers = m_JobScene.RevealerProperties.AsArray();
		jobData2.BlockerSegments = m_JobScene.StaticBlockerSegments.AsArray();
		jobData2.BlockerPlanes = m_JobResources.StaticBlockerPlanes.AsArray();
		jobData2.BlockerPlaneSetCounts = m_JobResources.StaticBlockerPlaneSetCounts.AsArray();
		jobData2.BlockerPlaneSets = m_JobResources.StaticBlockerPlaneSets.AsArray();
		JobHandle job = IJobParallelForExtensions.Schedule(jobData2, length, 2, dependsOn);
		jobData = default(BuildBlockerPlanesJob);
		jobData.BlockerSegments = m_JobScene.DynamicBlockerSegments.AsArray();
		jobData.BlockerPlanes = m_JobResources.DynamicBlockerPlanes.AsArray();
		JobHandle dependsOn2 = IJobParallelForExtensions.Schedule(jobData, length4, 128);
		jobData2 = default(BuildBlockerPlaneSetsJob);
		jobData2.Revealers = m_JobScene.RevealerProperties.AsArray();
		jobData2.BlockerSegments = m_JobScene.DynamicBlockerSegments.AsArray();
		jobData2.BlockerPlanes = m_JobResources.DynamicBlockerPlanes.AsArray();
		jobData2.BlockerPlaneSetCounts = m_JobResources.DynamicBlockerPlaneSetCounts.AsArray();
		jobData2.BlockerPlaneSets = m_JobResources.DynamicBlockerPlaneSets.AsArray();
		JobHandle job2 = IJobParallelForExtensions.Schedule(jobData2, length, 2, dependsOn2);
		CullJob jobData3 = default(CullJob);
		jobData3.StaticBlockerSegmentCount = length3;
		jobData3.DynamicBlockerSegmentCount = length4;
		jobData3.Revealers = m_JobScene.RevealerProperties.AsArray();
		jobData3.StaticPlaneSetCounts = m_JobResources.StaticBlockerPlaneSetCounts.AsArray();
		jobData3.StaticPlaneSets = m_JobResources.StaticBlockerPlaneSets.AsArray();
		jobData3.DynamicPlaneSetCounts = m_JobResources.DynamicBlockerPlaneSetCounts.AsArray();
		jobData3.DynamicPlaneSets = m_JobResources.DynamicBlockerPlaneSets.AsArray();
		jobData3.Targets = m_JobScene.TargetProperties.AsArray();
		jobData3.TargetRevealedStates = m_JobScene.TargetRevealedStates.AsArray();
		jobData3.TargetForceRevealStates = m_JobScene.TargetForceRevealStates.AsArray();
		jobData3.NotifyTargetIndices = m_JobResources.NotifyTargetIndices.AsParallelWriter();
		JobHandle value = IJobParallelForExtensions.Schedule(jobData3, m_JobScene.TargetProperties.Length, 32, JobHandle.CombineDependencies(job, job2));
		m_ScheduledJobHandle = value;
	}
}
