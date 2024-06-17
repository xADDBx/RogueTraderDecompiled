using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.Visual.Particles.GameObjectsPooling;

public static class GameObjectsPool
{
	public static bool Disabled;

	[CanBeNull]
	private static GameObject s_PoolRoot;

	[CanBeNull]
	private static GameObject s_PoolDisabledRoot;

	[NotNull]
	private static readonly Dictionary<int, Queue<PooledGameObject>> s_Instances = new Dictionary<int, Queue<PooledGameObject>>();

	[NotNull]
	private static readonly HashSet<PooledGameObject> s_AllPooledPrefabs = new HashSet<PooledGameObject>();

	[NotNull]
	private static GameObject PoolRoot
	{
		get
		{
			if (s_PoolRoot == null)
			{
				s_PoolRoot = new GameObject("[Fx Pool]");
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(s_PoolRoot);
				}
			}
			return s_PoolRoot;
		}
	}

	[NotNull]
	private static GameObject DisabledRoot
	{
		get
		{
			if (s_PoolDisabledRoot == null)
			{
				s_PoolDisabledRoot = new GameObject("[Fx Disabled Pool]");
				s_PoolDisabledRoot.SetActive(value: false);
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(s_PoolDisabledRoot);
				}
			}
			return s_PoolDisabledRoot;
		}
	}

	[NotNull]
	public static GameObject Claim(GameObject prefab, Vector3 position, Quaternion rotation, bool enableObject = true)
	{
		try
		{
			PooledGameObject component = prefab.GetComponent<PooledGameObject>();
			if (!component || !component.enabled || Disabled)
			{
				try
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
					gameObject.SetActive(enableObject);
					return gameObject;
				}
				finally
				{
				}
			}
			if (BuildModeUtility.IsDevelopment)
			{
				s_AllPooledPrefabs.Add(component);
			}
			PooledGameObject pooledGameObject = null;
			Queue<PooledGameObject> instances = GetInstances(component);
			if (instances.Count > 0)
			{
				pooledGameObject = instances.Dequeue();
				if (!pooledGameObject)
				{
					PFLog.Default.Error($"Unable to dequeue {component} from FX pool: null in pool.");
				}
			}
			if (pooledGameObject == null)
			{
				pooledGameObject = CreateInstance(component, position, rotation);
			}
			pooledGameObject.gameObject.transform.position = position;
			pooledGameObject.gameObject.transform.rotation = rotation;
			pooledGameObject.gameObject.SetActive(enableObject);
			pooledGameObject.OnClaim();
			return pooledGameObject.gameObject;
		}
		finally
		{
		}
	}

	public static void Release(GameObject instance)
	{
		if (instance == null)
		{
			return;
		}
		if (instance.transform.parent == PoolRoot.transform)
		{
			PFLog.Default.Warning($"Fx object {instance} double-released");
			return;
		}
		try
		{
			PooledGameObject component = instance.GetComponent<PooledGameObject>();
			if (component == null || component.Prefab == null)
			{
				UnityEngine.Object.Destroy(instance);
				return;
			}
			if (BuildModeUtility.IsDevelopment && component.Version < component.Prefab.Version)
			{
				UnityEngine.Object.Destroy(instance);
				return;
			}
			instance.SetActive(value: false);
			component.OnRelease();
			instance.transform.parent = PoolRoot.transform;
			GetInstances(component.Prefab).Enqueue(component);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		finally
		{
		}
	}

	public static bool HasPoolInstances(PooledGameObject prefab)
	{
		int instanceID = prefab.GetInstanceID();
		return s_Instances.ContainsKey(instanceID);
	}

	public static bool IsInPool(GameObject instance)
	{
		return instance.transform.parent == PoolRoot.transform;
	}

	public static void Warmup(PooledGameObject prefab, int count)
	{
		if ((bool)prefab)
		{
			if (BuildModeUtility.IsDevelopment)
			{
				s_AllPooledPrefabs.Add(prefab);
			}
			Queue<PooledGameObject> instances = GetInstances(prefab);
			while (instances.Count < count)
			{
				instances.Enqueue(CreateInstance(prefab, Vector3.zero, Quaternion.identity));
			}
		}
	}

	public static void ResetAllPools()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return;
		}
		foreach (PooledGameObject s_AllPooledPrefab in s_AllPooledPrefabs)
		{
			s_AllPooledPrefab.ResetPool();
		}
	}

	public static void ClearPool(PooledGameObject prefab)
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return;
		}
		Queue<PooledGameObject> instances = GetInstances(prefab);
		foreach (PooledGameObject item in instances)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		instances.Clear();
	}

	public static void ClearPool()
	{
		s_Instances.Clear();
		s_AllPooledPrefabs.Clear();
		UnityEngine.Object.Destroy(s_PoolRoot);
		_ = PoolRoot;
	}

	private static PooledGameObject CreateInstance(PooledGameObject prefab, Vector3 position, Quaternion rotation)
	{
		PooledGameObject pooledGameObject = prefab.CreateInstance(position, rotation, DisabledRoot.transform);
		pooledGameObject.transform.parent = PoolRoot.transform;
		return pooledGameObject;
	}

	[NotNull]
	private static Queue<PooledGameObject> GetInstances(PooledGameObject prefab)
	{
		int instanceID = prefab.GetInstanceID();
		if (!s_Instances.TryGetValue(instanceID, out var value))
		{
			value = new Queue<PooledGameObject>();
			s_Instances[instanceID] = value;
		}
		return value;
	}
}
