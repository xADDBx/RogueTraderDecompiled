using UnityEngine;

namespace Kingmaker.Visual;

public class DynamicDecal : MonoBehaviour
{
	private static int s_Queue;

	private Renderer m_DecalRenderer;

	private void Start()
	{
		m_DecalRenderer = GetComponent<Renderer>();
		if (m_DecalRenderer != null)
		{
			Material material = m_DecalRenderer.material;
			material.renderQueue = GetQueue();
			m_DecalRenderer.material = material;
		}
	}

	private static int GetQueue()
	{
		s_Queue++;
		if (s_Queue >= 50)
		{
			s_Queue = 0;
		}
		return 2450 + s_Queue;
	}
}
