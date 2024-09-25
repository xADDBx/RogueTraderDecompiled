using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

[RequireComponent(typeof(FogOfWarBlocker))]
public class FogOfWarBlockerUpdater : MonoBehaviour
{
	private class FogOfWarBlockerUpdateManager : MonoBehaviour
	{
		private static FogOfWarBlockerUpdateManager s_Instance;

		public bool m_UpdateManually;

		public static void Ensure()
		{
			if (s_Instance == null)
			{
				GameObject obj = new GameObject("FogOfWarBlockerUpdateManager");
				Object.DontDestroyOnLoad(obj);
				s_Instance = obj.AddComponent<FogOfWarBlockerUpdateManager>();
			}
		}

		public static void ManualUpdate()
		{
			if (!(s_Instance == null))
			{
				s_Instance.m_UpdateManually = true;
				s_Instance.Tick();
			}
		}

		private void Update()
		{
			if (!m_UpdateManually)
			{
				Tick();
			}
		}

		private void Tick()
		{
			foreach (FogOfWarBlocker updatableBlocker in UpdatableBlockers)
			{
				updatableBlocker.UpdateIfNecessary();
			}
		}
	}

	private static readonly List<FogOfWarBlocker> UpdatableBlockers = new List<FogOfWarBlocker>();

	private FogOfWarBlocker m_Blocker;

	public static void ManualUpdate()
	{
		FogOfWarBlockerUpdateManager.ManualUpdate();
	}

	private void Awake()
	{
		m_Blocker = (TryGetComponent<FogOfWarBlocker>(out var component) ? component : null);
		if (m_Blocker == null)
		{
			Debug.Log("FogOfWarBlockerUpdater.Awake: FogOfWarBlocker component is missing", this);
		}
	}

	private void OnEnable()
	{
		if (m_Blocker != null)
		{
			FogOfWarBlockerUpdateManager.Ensure();
			UpdatableBlockers.Add(m_Blocker);
		}
	}

	private void OnDisable()
	{
		UpdatableBlockers.Remove(m_Blocker);
	}
}
