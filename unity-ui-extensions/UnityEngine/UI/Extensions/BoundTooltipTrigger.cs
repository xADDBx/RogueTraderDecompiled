using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Bound Tooltip/Tooltip Trigger")]
public class BoundTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[TextArea]
	public string text;

	public bool useMousePosition;

	public Vector3 offset;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (useMousePosition)
		{
			StartHover(new Vector3(eventData.position.x, eventData.position.y, 0f));
		}
		else
		{
			StartHover(base.transform.position + offset);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		StartHover(base.transform.position);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopHover();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		StopHover();
	}

	private void StartHover(Vector3 position)
	{
		BoundTooltipItem.Instance.ShowTooltip(text, position);
	}

	private void StopHover()
	{
		BoundTooltipItem.Instance.HideTooltip();
	}
}
