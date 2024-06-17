using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SurfaceCombat;

public class CombatStartCoopProgressBaseItemView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public void SetActive(bool active)
	{
		m_CanvasGroup.alpha = (active ? 1f : 0.25f);
	}
}
