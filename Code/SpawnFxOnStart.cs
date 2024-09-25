using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

[RequireComponent(typeof(UnitEntityView))]
public class SpawnFxOnStart : MonoBehaviour
{
	public PrefabLink FxOnStart;

	public PrefabLink FxOnDeath;

	public PrefabLink FxOnDismemberment;

	public bool RemoveOnDeath;

	private GameObject m_SpawnedFx;

	public void SpawnFx()
	{
		if ((bool)m_SpawnedFx)
		{
			return;
		}
		UnitEntityView componentNonAlloc = this.GetComponentNonAlloc<UnitEntityView>();
		if (!componentNonAlloc.EntityData.LifeState.IsDead || !RemoveOnDeath)
		{
			GameObject gameObject = FxOnStart.Load();
			if ((bool)gameObject)
			{
				m_SpawnedFx = FxHelper.SpawnFxOnEntity(gameObject, componentNonAlloc);
			}
		}
	}

	public void HandleUnitDeath()
	{
		if ((bool)m_SpawnedFx && RemoveOnDeath)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
		GameObject gameObject = FxOnDeath.Load();
		if ((bool)gameObject)
		{
			UnitEntityView componentNonAlloc = this.GetComponentNonAlloc<UnitEntityView>();
			FxHelper.SpawnFxOnEntity(gameObject, componentNonAlloc);
		}
	}

	public void HandleUnitDeathRagdoll()
	{
		if ((bool)m_SpawnedFx && RemoveOnDeath)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
		GameObject gameObject = FxOnDismemberment.Load();
		if ((bool)gameObject)
		{
			UnitEntityView componentNonAlloc = this.GetComponentNonAlloc<UnitEntityView>();
			FxHelper.SpawnFxOnEntity(gameObject, componentNonAlloc);
		}
	}

	public void HandleUnitDismemberment()
	{
		if ((bool)m_SpawnedFx && RemoveOnDeath)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
		GameObject gameObject = FxOnDismemberment.Load();
		if (!gameObject)
		{
			return;
		}
		UnitEntityView componentNonAlloc = this.GetComponentNonAlloc<UnitEntityView>();
		Animator componentInChildren = GetComponentInChildren<Animator>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			componentInChildren.enabled = true;
			componentInChildren.gameObject.SetActive(value: true);
			FxLocator[] componentsInChildren = componentInChildren.GetComponentsInChildren<FxLocator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
			componentInChildren.enabled = false;
			componentInChildren.gameObject.SetActive(value: false);
		}
		SnapMapBase component = GetComponent<SnapMapBase>();
		if ((bool)component)
		{
			component.Init();
		}
		FxHelper.SpawnFxOnEntity(gameObject, componentNonAlloc);
	}

	private void OnDestroy()
	{
		if ((bool)m_SpawnedFx)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
	}
}
