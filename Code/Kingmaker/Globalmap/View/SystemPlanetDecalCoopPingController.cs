using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class SystemPlanetDecalCoopPingController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_SystemPlanetDecalCoopPing;

	public void PingEntity(bool state)
	{
		m_SystemPlanetDecalCoopPing.SetActive(state);
	}
}
