using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxAoeSpawner : MonoBehaviour
{
	[Serializable]
	public class Entry
	{
		public GameObject Root;

		public float DelayFxVisualPrefab;

		public GameObject FxVisualPrefabOverride;

		public GameObject LocatorsObjectOverride;
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("FxAoeSpawner");

	private readonly Dictionary<GameObject, GameObject> m_FxRootMap = new Dictionary<GameObject, GameObject>();

	private float m_DelayTimer;

	private readonly List<Entry> m_DelayedEntries = new List<Entry>();

	public FxAoeSpawnPattern Pattern = FxAoeSpawnPattern.N7;

	public float Radius = 10f;

	public GameObject FxVisualPrefab;

	public GameObject LocatorsObject;

	public LayerMask RaycastMask = -1;

	public float RaycastUpDownShift = 7f;

	public Entry[] Entries = Array.Empty<Entry>();

	private void OnEnable()
	{
		m_DelayTimer = 0f;
		m_FxRootMap.Clear();
		m_DelayedEntries.Clear();
		for (int i = 0; i < Entries.Length; i++)
		{
			Entry entry = Entries[i];
			if (entry == null)
			{
				Logger.Error(this, $"FxAoeSpawner entry at index {i} is null");
			}
			else if (entry.DelayFxVisualPrefab <= 0f)
			{
				SpawnFxOnEntry(entry);
			}
			else
			{
				m_DelayedEntries.Add(entry);
			}
		}
		if (FxVisualPrefab == null)
		{
			Logger.Error(this, "FxAoeSpawner FxVisualPrefab is null");
			return;
		}
		if (LocatorsObject == null)
		{
			Logger.Error(this, "FxAoeSpawner LocatorsObject is null");
			return;
		}
		if (base.transform.root == FxVisualPrefab.transform.root)
		{
			FxVisualPrefab.SetActive(value: false);
		}
		if (base.transform.root == LocatorsObject.transform.root)
		{
			LocatorsObject.SetActive(value: false);
		}
	}

	private void SpawnFxOnEntry(Entry entry)
	{
		GameObject gameObject = (entry.FxVisualPrefabOverride ? entry.FxVisualPrefabOverride : FxVisualPrefab);
		if (!(gameObject == null))
		{
			if (!gameObject.activeSelf && base.transform.root == gameObject.transform.root)
			{
				gameObject.SetActive(value: true);
			}
			Ray ray = default(Ray);
			ray.direction = Vector3.down;
			float y = base.transform.position.y + RaycastUpDownShift;
			Vector3 position = entry.Root.transform.position;
			ray.origin = new Vector3(position.x, y, position.z);
			if (Physics.Raycast(ray, out var hitInfo, RaycastUpDownShift * 2f, RaycastMask))
			{
				GameObject gameObject2 = FxHelper.SpawnFxOnPoint(gameObject, hitInfo.point);
				gameObject2.transform.rotation = entry.Root.transform.rotation;
				gameObject2.transform.localScale = entry.Root.transform.localScale;
				m_FxRootMap.Add(gameObject2, entry.Root);
			}
			if (base.transform.root == gameObject.transform.root)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < m_DelayedEntries.Count; i++)
		{
			Entry entry = m_DelayedEntries[i];
			if (m_DelayTimer >= entry.DelayFxVisualPrefab)
			{
				m_DelayedEntries.RemoveAt(i);
				i--;
				SpawnFxOnEntry(entry);
			}
		}
		m_DelayTimer += Time.deltaTime;
		foreach (KeyValuePair<GameObject, GameObject> item in m_FxRootMap)
		{
			if (!(item.Key == null) && !(item.Value == null))
			{
				Vector3 position = item.Value.transform.position;
				Vector3 position2 = item.Key.transform.position;
				item.Key.transform.position = new Vector3(position.x, position2.y, position.z);
			}
		}
	}
}
