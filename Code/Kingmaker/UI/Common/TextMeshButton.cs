using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class TextMeshButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
	public TextMeshProUGUI Text;

	public Color NormalColor;

	public Color HighlitedColor;

	public Color PressedColor;

	public Color DisableColor;

	private Button m_Button;

	public void OnPointerDown(PointerEventData eventData)
	{
		Text.color = (m_Button.interactable ? PressedColor : DisableColor);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		Text.color = (m_Button.interactable ? NormalColor : DisableColor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Text.color = (m_Button.interactable ? NormalColor : DisableColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Text.color = (m_Button.interactable ? HighlitedColor : DisableColor);
	}

	public void CheckDisable()
	{
		m_Button = base.gameObject.GetComponent<Button>();
		Text.color = (m_Button.interactable ? NormalColor : DisableColor);
	}
}
