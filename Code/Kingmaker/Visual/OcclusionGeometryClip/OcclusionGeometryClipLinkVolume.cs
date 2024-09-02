using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

public sealed class OcclusionGeometryClipLinkVolume : MonoBehaviour
{
	[SerializeField]
	private Renderer m_LinkedRenderer;

	public Renderer LinkedRenderer => m_LinkedRenderer;

	public PlaneBox GetBounds()
	{
		Transform transform = base.transform;
		Vector3 center = transform.position;
		Quaternion rotation = transform.rotation;
		Vector3 size = transform.localScale;
		return new PlaneBox(in center, in rotation, in size);
	}
}
