using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.Particles.GameObjectsPooling;

public abstract class PooledGameObject : MonoBehaviour
{
	[NonSerialized]
	[CanBeNull]
	public PooledGameObject Prefab;

	[NonSerialized]
	public int Version;

	[ItemCanBeNull]
	private List<IPooledComponent> m_PooledComponents = new List<IPooledComponent>();

	public bool IsClaimed { get; private set; }

	public void ResetPool()
	{
		if (Prefab != null)
		{
			Prefab.ResetPool();
			return;
		}
		GameObjectsPool.ClearPool(this);
		Version++;
	}

	[NotNull]
	public virtual PooledGameObject CreateInstance(Vector3 position, Quaternion rotation, Transform disabledRoot)
	{
		PooledGameObject component = UnityEngine.Object.Instantiate(base.gameObject, position, rotation, disabledRoot).GetComponent<PooledGameObject>();
		component.gameObject.SetActive(value: false);
		component.Prefab = this;
		component.Version = Version;
		component.CollectSubObjects();
		return component;
	}

	private void CollectSubObjects()
	{
		GetComponents(m_PooledComponents);
	}

	public virtual void OnClaim()
	{
		if (m_PooledComponents != null)
		{
			for (int i = 0; i < m_PooledComponents.Count; i++)
			{
				m_PooledComponents[i]?.OnClaim();
			}
			IsClaimed = true;
		}
	}

	public virtual void OnRelease()
	{
		if (m_PooledComponents != null)
		{
			for (int i = 0; i < m_PooledComponents.Count; i++)
			{
				m_PooledComponents[i]?.OnRelease();
			}
			IsClaimed = false;
		}
	}

	[UsedImplicitly]
	private void Start()
	{
	}
}
