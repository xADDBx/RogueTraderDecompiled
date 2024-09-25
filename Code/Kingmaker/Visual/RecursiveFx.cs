using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Particles;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual;

public class RecursiveFx : MonoBehaviour, IFxSpawner, IDeactivatableComponent
{
	private bool m_Executed;

	[ValidateHasComponent(typeof(ExcludeFromResources))]
	public GameObject Prefab;

	private ParticlesSnapMap m_Root;

	private ParticlesSnapMap m_Map;

	private GameObject m_Spawned;

	public FxSpawnerPriority Priority => FxSpawnerPriority.Initialize;

	public GameObject Spawn()
	{
		m_Spawned = null;
		if (m_Executed)
		{
			return m_Spawned;
		}
		if ((bool)Prefab)
		{
			return m_Spawned = FxHelper.SpawnFxOnGameObject(Prefab, base.gameObject);
		}
		m_Executed = true;
		return m_Spawned = null;
	}

	public void SpawnFxOnGameObject(GameObject target)
	{
		if (base.enabled)
		{
			Spawn();
			m_Map = GetComponent<ParticlesSnapMap>();
			m_Root = target.GetComponentInChildren<ParticlesSnapMap>();
		}
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		if (base.enabled)
		{
			Spawn();
		}
	}

	private void Update()
	{
		if ((bool)m_Root && (bool)m_Map)
		{
			m_Map.AdditionalScale = m_Root.AdditionalScale * base.transform.lossyScale.x;
		}
	}

	public void Stop()
	{
		if ((bool)m_Spawned)
		{
			FxHelper.Stop(m_Spawned);
			m_Spawned = null;
		}
	}

	public void OnDestroyed(bool immediate = false)
	{
		if ((bool)m_Spawned)
		{
			FxHelper.Destroy(m_Spawned, immediate);
			m_Spawned = null;
		}
	}
}
