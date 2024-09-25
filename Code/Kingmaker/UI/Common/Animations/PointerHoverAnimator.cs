using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common.Animations;

public class PointerHoverAnimator : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_MoveAnimator.AppearAnimation();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_MoveAnimator.DisappearAnimation();
	}
}
