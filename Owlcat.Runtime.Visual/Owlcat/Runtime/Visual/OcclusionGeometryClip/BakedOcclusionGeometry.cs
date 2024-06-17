using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public sealed class BakedOcclusionGeometry : MonoBehaviour, IOcclusionGeometryProvider
{
	[SerializeField]
	public OcclusionGeometry m_OcclusionGeometry;

	OcclusionGeometry IOcclusionGeometryProvider.OcclusionGeometry => m_OcclusionGeometry;

	[UsedImplicitly]
	private void OnEnable()
	{
		System.RegisterOcclusionGeometry(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		System.UnregisterOcclusionGeometry(this);
	}
}
