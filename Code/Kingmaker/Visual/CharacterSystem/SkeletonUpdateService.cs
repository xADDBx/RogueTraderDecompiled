using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility.Locator;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Pool;

namespace Kingmaker.Visual.CharacterSystem;

public class SkeletonUpdateService : RegisteredObjectBase, IService, ILateUpdatable, IDisposable
{
	private static ServiceProxy<SkeletonUpdateService> s_Proxy;

	private NativeList<JobHandle> m_JobHandles;

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	private SkeletonUpdateService()
	{
		Enable();
	}

	public static void Ensure()
	{
		if (s_Proxy == null)
		{
			Services.RegisterServiceInstance(new SkeletonUpdateService());
		}
		s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<SkeletonUpdateService>());
	}

	public void Dispose()
	{
		if (m_JobHandles.IsCreated)
		{
			m_JobHandles.Dispose();
		}
	}

	protected override void OnDisabled()
	{
		if (m_JobHandles.IsCreated)
		{
			m_JobHandles.Dispose();
		}
	}

	public void DoLateUpdate()
	{
		List<Character> value;
		using (CollectionPool<List<Character>, Character>.Get(out value))
		{
			foreach (Character item in ObjectRegistry<Character>.Instance)
			{
				if ((bool)item && (bool)item.Skeleton)
				{
					value.Add(item);
				}
			}
			if (value.Count <= 0)
			{
				return;
			}
			if (m_JobHandles.IsCreated)
			{
				m_JobHandles.Clear();
			}
			else
			{
				m_JobHandles = new NativeList<JobHandle>(value.Count, Allocator.Persistent);
			}
			foreach (Character item2 in value)
			{
				if (item2.HasBonesList)
				{
					ref NativeList<JobHandle> jobHandles = ref m_JobHandles;
					JobHandle value2 = item2.ScheduleBoneUpdateJob();
					jobHandles.Add(in value2);
				}
			}
			JobHandle.CompleteAll(m_JobHandles);
			foreach (Character item3 in value)
			{
				item3.UpdateSkeleton(runJob: false);
			}
		}
	}
}
