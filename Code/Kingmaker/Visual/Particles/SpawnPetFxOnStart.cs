using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[RequireComponent(typeof(UnitEntityView))]
public class SpawnPetFxOnStart : MonoBehaviour, IPetInitializationHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	[SerializeField]
	[Tooltip("Префаб эффекта для спавна")]
	public PrefabLink FxPrefab;

	[SerializeField]
	[Tooltip("Задержка перед спавном в секундах")]
	public float DelaySeconds = 0.5f;

	private GameObject m_SpawnedFx;

	private UnitEntityView m_View;

	private bool m_IsSubscribed;

	private void Start()
	{
		m_View = GetComponent<UnitEntityView>();
		if (!(m_View == null))
		{
			EventBus.Subscribe(this);
			m_IsSubscribed = true;
			Invoke("TrySpawnFx", DelaySeconds);
		}
	}

	public void OnPetInitialized()
	{
		_ = m_View?.EntityData;
		if (!m_SpawnedFx)
		{
			Invoke("TrySpawnFx", DelaySeconds);
		}
	}

	private void TrySpawnFx()
	{
		if ((bool)m_SpawnedFx || !FxPrefab.Exists())
		{
			return;
		}
		GameObject gameObject = FxPrefab.Load();
		if (!gameObject || m_View == null)
		{
			return;
		}
		SnapMapBase component = m_View.GetComponent<SnapMapBase>();
		if (component != null && !component.Initialized)
		{
			try
			{
				component.Init();
			}
			catch (Exception)
			{
				Invoke("TrySpawnFx", 1f);
				return;
			}
		}
		try
		{
			m_SpawnedFx = FxHelper.SpawnFxOnEntity(gameObject, m_View);
			if (!m_SpawnedFx)
			{
				Invoke("TrySpawnFx", 1f);
			}
		}
		catch (Exception)
		{
			Invoke("TrySpawnFx", 2f);
		}
	}

	private void OnDestroy()
	{
		if (m_IsSubscribed)
		{
			EventBus.Unsubscribe(this);
			m_IsSubscribed = false;
		}
		if ((bool)m_SpawnedFx)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
	}
}
