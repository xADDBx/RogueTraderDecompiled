using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class SystemPlanetDecalConsoleFocusController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_SystemPlanetDecalFocusDecal;

	public void SetFocusState(bool state)
	{
		m_SystemPlanetDecalFocusDecal.SetActive(state);
		if (state)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
	}
}
