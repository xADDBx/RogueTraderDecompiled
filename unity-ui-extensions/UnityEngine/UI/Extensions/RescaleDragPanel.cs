using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/RescalePanels/RescaleDragPanel")]
public class RescaleDragPanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	private Vector2 pointerOffset;

	private RectTransform canvasRectTransform;

	private RectTransform panelRectTransform;

	private Transform goTransform;

	private void Awake()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (componentInParent != null)
		{
			canvasRectTransform = componentInParent.transform as RectTransform;
			panelRectTransform = base.transform.parent as RectTransform;
			goTransform = base.transform.parent;
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		panelRectTransform.SetAsLastSibling();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out pointerOffset);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(panelRectTransform == null))
		{
			Vector2 position = data.position;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, data.pressEventCamera, out var localPoint))
			{
				panelRectTransform.localPosition = localPoint - new Vector2(pointerOffset.x * goTransform.localScale.x, pointerOffset.y * goTransform.localScale.y);
			}
		}
	}

	private Vector2 ClampToWindow(PointerEventData data)
	{
		Vector2 position = data.position;
		Vector3[] array = new Vector3[4];
		canvasRectTransform.GetWorldCorners(array);
		float x = Mathf.Clamp(position.x, array[0].x, array[2].x);
		float y = Mathf.Clamp(position.y, array[0].y, array[2].y);
		return new Vector2(x, y);
	}
}
