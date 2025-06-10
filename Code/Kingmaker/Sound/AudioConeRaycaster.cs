using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Sound;

public class AudioConeRaycaster : MonoBehaviour
{
	[SerializeField]
	private LayerMask m_LayerMask;

	[SerializeField]
	private float m_ConeDistance = 10f;

	[SerializeField]
	private float m_ConeAngle = 45f;

	[SerializeField]
	private string m_RtpcName = "Raycast";

	[SerializeField]
	private float m_RtpcHitValue = 1f;

	[SerializeField]
	private float m_RtpcDefaultValue;

	private const int MaxColliders = 20;

	private readonly Collider[] m_Colliders = new Collider[20];

	private HashSet<GameObject> m_HitObjects = new HashSet<GameObject>();

	private Vector3? m_LastForward;

	private Vector3? m_LastPosition;

	private void Update()
	{
		DrawDebugCone();
		UpdateHitObjects();
	}

	private void DrawDebugCone()
	{
	}

	private void UpdateHitObjects()
	{
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		if (m_LastForward.HasValue && m_LastForward.Value == forward && m_LastPosition.HasValue && m_LastPosition.Value == position)
		{
			return;
		}
		m_LastForward = forward;
		m_LastPosition = position;
		int num = Physics.OverlapSphereNonAlloc(m_LastPosition.Value, m_ConeDistance, m_Colliders, m_LayerMask);
		HashSet<GameObject> hashSet = CollectionPool<HashSet<GameObject>, GameObject>.Get();
		hashSet.Clear();
		for (int i = 0; i < num; i++)
		{
			Vector3 to = m_Colliders[i].transform.position - m_LastPosition.Value;
			to.Normalize();
			if (Vector3.Angle(m_LastForward.Value, to) <= m_ConeAngle)
			{
				Debug.DrawLine(m_LastPosition.Value, m_Colliders[i].transform.position, Color.green, 0.1f);
				AudioObject component = m_Colliders[i].GetComponent<AudioObject>();
				if (component != null)
				{
					hashSet.Add(component.gameObject);
					if (!m_HitObjects.Contains(component.gameObject))
					{
						AkSoundEngine.SetRTPCValue(m_RtpcName, m_RtpcHitValue, component.gameObject);
					}
				}
			}
			else
			{
				Debug.DrawLine(m_LastPosition.Value, m_Colliders[i].transform.position, Color.yellow, 0.1f);
			}
		}
		m_HitObjects.ExceptWith(hashSet);
		foreach (GameObject hitObject in m_HitObjects)
		{
			if (hitObject != null)
			{
				AkSoundEngine.SetRTPCValue(m_RtpcName, m_RtpcDefaultValue, hitObject);
			}
		}
		m_HitObjects.Clear();
		m_HitObjects.UnionWith(hashSet);
		hashSet.Clear();
		CollectionPool<HashSet<GameObject>, GameObject>.Release(hashSet);
	}
}
