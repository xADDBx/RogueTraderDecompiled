using UnityEngine;

namespace Kingmaker.Visual.Particles;

[RequireComponent(typeof(Light))]
public class DynamicLightImportance : MonoBehaviour
{
	public int Importance = 10;

	private Light m_Light;

	private void OnEnable()
	{
		m_Light = GetComponent<Light>();
		Game.Instance.LightsController.RegisterPartyLight(m_Light, Importance);
	}

	private void OnDisable()
	{
		Game.Instance.LightsController.UnregisterPartyLight(m_Light);
	}
}
