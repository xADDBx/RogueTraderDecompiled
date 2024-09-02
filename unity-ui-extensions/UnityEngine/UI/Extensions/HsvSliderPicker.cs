using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

public class HsvSliderPicker : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerDownHandler
{
	public HSVPicker picker;

	private void PlacePointer(PointerEventData eventData)
	{
		Vector2 vector = new Vector2(eventData.position.x - picker.hsvSlider.rectTransform.position.x, picker.hsvSlider.rectTransform.position.y - eventData.position.y);
		vector.y /= picker.hsvSlider.rectTransform.rect.height * picker.hsvSlider.canvas.transform.lossyScale.y;
		vector.y = Mathf.Clamp(vector.y, 0f, 1f);
		picker.MovePointer(vector.y);
	}

	public void OnDrag(PointerEventData eventData)
	{
		PlacePointer(eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		PlacePointer(eventData);
	}
}
