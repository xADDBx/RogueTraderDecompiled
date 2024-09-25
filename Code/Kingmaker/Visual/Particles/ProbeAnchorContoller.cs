using System.Collections.Generic;
using Kingmaker.Visual.Trails;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class ProbeAnchorContoller : MonoBehaviour
{
	private static List<ParticleSystemRenderer> m_TempParticleRenderers = new List<ParticleSystemRenderer>();

	private static List<CompositeTrailRenderer> m_TempTrailRenderers = new List<CompositeTrailRenderer>();

	private GameObject m_Anchor;

	private void OnEnable()
	{
		if (m_Anchor != null)
		{
			Object.Destroy(m_Anchor);
		}
		m_Anchor = new GameObject(base.name + "_ProbeAnchor");
		m_Anchor.transform.parent = FxHelper.FxRoot;
		m_Anchor.transform.position = base.transform.position;
		m_Anchor.transform.rotation = Quaternion.identity;
		m_Anchor.transform.localScale = Vector3.one;
		m_TempParticleRenderers.Clear();
		GetComponentsInChildren(m_TempParticleRenderers);
		foreach (ParticleSystemRenderer tempParticleRenderer in m_TempParticleRenderers)
		{
			tempParticleRenderer.probeAnchor = m_Anchor.transform;
		}
		m_TempTrailRenderers.Clear();
		GetComponentsInChildren(m_TempTrailRenderers);
		foreach (CompositeTrailRenderer tempTrailRenderer in m_TempTrailRenderers)
		{
			tempTrailRenderer.ProbeAnchor = m_Anchor.transform;
		}
	}

	private void OnDisable()
	{
		if (m_Anchor != null)
		{
			Object.Destroy(m_Anchor);
		}
		m_Anchor = null;
	}
}
