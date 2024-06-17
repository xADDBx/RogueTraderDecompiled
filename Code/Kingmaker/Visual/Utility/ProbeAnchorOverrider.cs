using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Visual.Trails;
using UnityEngine;

namespace Kingmaker.Visual.Utility;

public class ProbeAnchorOverrider : MonoBehaviour
{
	[Header("Components to affect")]
	[SerializeField]
	private bool m_OverrideParticleRenderers;

	[SerializeField]
	private bool m_OverrideTrailRenderers;

	[SerializeField]
	private bool m_OverrideMeshRenderers;

	[SerializeField]
	private bool m_OverrideSkinnedMeshRenderers;

	[Space]
	[Header("When to affect")]
	[SerializeField]
	private bool m_UpdateOnStart;

	[SerializeField]
	private bool m_UpdateOnEnable;

	private static List<ParticleSystemRenderer> m_ParticleRenderers = new List<ParticleSystemRenderer>();

	private static List<CompositeTrailRenderer> m_TrailRenderers = new List<CompositeTrailRenderer>();

	private static List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();

	private static List<SkinnedMeshRenderer> m_SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

	private void Start()
	{
		if (m_UpdateOnStart)
		{
			GatherLists();
			UpdateAnchors();
		}
	}

	private void OnEnable()
	{
		if (m_UpdateOnEnable)
		{
			GatherLists();
			UpdateAnchors();
		}
	}

	private void GatherLists()
	{
		if (m_OverrideParticleRenderers)
		{
			GetComponentsInChildren(m_ParticleRenderers);
		}
		if (m_OverrideTrailRenderers)
		{
			GetComponentsInChildren(m_TrailRenderers);
		}
		if (m_OverrideMeshRenderers)
		{
			GetComponentsInChildren(m_MeshRenderers);
		}
		if (m_OverrideSkinnedMeshRenderers)
		{
			GetComponentsInChildren(m_SkinnedMeshRenderers);
		}
	}

	[CanBeNull]
	private Transform LocateAnchor()
	{
		return GetComponentInChildren<ProbeAnchorLocator>().transform;
	}

	private void UpdateAnchors()
	{
		Transform transform = LocateAnchor();
		if (transform == null)
		{
			return;
		}
		if (m_OverrideParticleRenderers)
		{
			foreach (ParticleSystemRenderer particleRenderer in m_ParticleRenderers)
			{
				particleRenderer.probeAnchor = transform;
			}
			m_ParticleRenderers.Clear();
		}
		if (m_OverrideTrailRenderers)
		{
			foreach (CompositeTrailRenderer trailRenderer in m_TrailRenderers)
			{
				trailRenderer.ProbeAnchor = transform;
			}
			m_TrailRenderers.Clear();
		}
		if (m_OverrideMeshRenderers)
		{
			foreach (MeshRenderer meshRenderer in m_MeshRenderers)
			{
				meshRenderer.probeAnchor = transform;
			}
			m_MeshRenderers.Clear();
		}
		if (!m_OverrideSkinnedMeshRenderers)
		{
			return;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in m_SkinnedMeshRenderers)
		{
			skinnedMeshRenderer.probeAnchor = transform;
		}
		m_SkinnedMeshRenderers.Clear();
	}
}
