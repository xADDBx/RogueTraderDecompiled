using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Trails;

internal static class InstancedCompositeTrailsManager
{
	private static Transform s_TrailsRoot;

	[NotNull]
	private static readonly Dictionary<int, List<CompositeTrailRenderer>> s_Instances = new Dictionary<int, List<CompositeTrailRenderer>>();

	private static Transform TrailsRoot
	{
		get
		{
			if (s_TrailsRoot == null)
			{
				GameObject gameObject = new GameObject("[Composite Trail Instances]");
				Game.Instance.DynamicRoot?.Add(gameObject.transform);
				s_TrailsRoot = gameObject.transform;
			}
			return s_TrailsRoot;
		}
	}

	[CanBeNull]
	public static CompositeTrailRenderer RequestInstance([NotNull] InstancedCompositeTrail instancedTrail)
	{
		if (instancedTrail.Prefab == null)
		{
			PFLog.Default.Error(instancedTrail, "InstancedCompositeTrail without prefab");
			return null;
		}
		int instanceID = instancedTrail.Prefab.GetInstanceID();
		List<CompositeTrailRenderer> list = s_Instances.Get(instanceID);
		if (list == null)
		{
			list = new List<CompositeTrailRenderer>(instancedTrail.InstancesCount);
			s_Instances[instanceID] = list;
		}
		int num = instancedTrail.InstanceId;
		if (num < 0)
		{
			num = PFStatefulRandom.Trails.Range(0, instancedTrail.InstancesCount);
		}
		else if (num >= instancedTrail.InstancesCount)
		{
			num = instancedTrail.InstancesCount - 1;
		}
		for (int i = list.Count; i <= num; i++)
		{
			list.Add(CreateInstance(instancedTrail.Prefab));
		}
		if (list[num] == null)
		{
			list[num] = CreateInstance(instancedTrail.Prefab);
		}
		return list[num];
	}

	[NotNull]
	private static CompositeTrailRenderer CreateInstance([NotNull] CompositeTrailRenderer prefab)
	{
		CompositeTrailRenderer compositeTrailRenderer = Object.Instantiate(prefab);
		compositeTrailRenderer.gameObject.transform.parent = TrailsRoot;
		compositeTrailRenderer.gameObject.transform.position = Vector3.zero;
		compositeTrailRenderer.gameObject.transform.rotation = Quaternion.identity;
		compositeTrailRenderer.Alignment = CompositeTrailRenderer.TrailAlignment.World;
		compositeTrailRenderer.gameObject.SetActive(value: false);
		return compositeTrailRenderer;
	}
}
