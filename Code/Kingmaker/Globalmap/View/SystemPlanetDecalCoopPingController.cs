using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class SystemPlanetDecalCoopPingController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_SystemPlanetDecalCoopPing;

	[SerializeField]
	private MeshRenderer DecalMeshRenderer;

	public void PingEntity(bool state, Material material = null)
	{
		if (DecalMeshRenderer != null && state && material != null)
		{
			DecalMeshRenderer.material = material;
		}
		m_SystemPlanetDecalCoopPing.SetActive(state);
	}
}
