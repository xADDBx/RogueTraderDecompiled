using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

public class HsvBoxSelector : MonoBehaviour, IDragHandler, IEventSystemHandler, IPointerDownHandler
{
	public HSVPicker picker;

	private void PlaceCursor(PointerEventData eventData)
	{
		Vector2 vector = new Vector2(eventData.position.x - picker.hsvImage.rectTransform.position.x, picker.hsvImage.rectTransform.rect.height * picker.hsvImage.transform.lossyScale.y - (picker.hsvImage.rectTransform.position.y - eventData.position.y));
		vector.x /= picker.hsvImage.rectTransform.rect.width * picker.hsvImage.transform.lossyScale.x;
		vector.y /= picker.hsvImage.rectTransform.rect.height * picker.hsvImage.transform.lossyScale.y;
		vector.x = Mathf.Clamp(vector.x, 0f, 0.9999f);
		vector.y = Mathf.Clamp(vector.y, 0f, 0.9999f);
		picker.MoveCursor(vector.x, vector.y);
	}

	public void OnDrag(PointerEventData eventData)
	{
		PlaceCursor(eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		PlaceCursor(eventData);
	}
}
