using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("UI/Extensions/Scrollrect Conflict Manager")]
public class ScrollConflictManager : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	public ScrollRect ParentScrollRect;

	private ScrollRect _myScrollRect;

	private bool scrollOther;

	private bool scrollOtherHorizontally;

	private void Awake()
	{
		_myScrollRect = GetComponent<ScrollRect>();
		scrollOtherHorizontally = _myScrollRect.vertical;
		if (scrollOtherHorizontally)
		{
			if (_myScrollRect.horizontal)
			{
				Debug.Log("You have added the SecondScrollRect to a scroll view that already has both directions selected");
			}
			if (!ParentScrollRect.horizontal)
			{
				Debug.Log("The other scroll rect doesnt support scrolling horizontally");
			}
		}
		else if (!ParentScrollRect.vertical)
		{
			Debug.Log("The other scroll rect doesnt support scrolling vertically");
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		float num = Mathf.Abs(eventData.position.x - eventData.pressPosition.x);
		float num2 = Mathf.Abs(eventData.position.y - eventData.pressPosition.y);
		if (scrollOtherHorizontally)
		{
			if (num > num2)
			{
				scrollOther = true;
				_myScrollRect.enabled = false;
				ParentScrollRect.OnBeginDrag(eventData);
			}
		}
		else if (num2 > num)
		{
			scrollOther = true;
			_myScrollRect.enabled = false;
			ParentScrollRect.OnBeginDrag(eventData);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (scrollOther)
		{
			scrollOther = false;
			_myScrollRect.enabled = true;
			ParentScrollRect.OnEndDrag(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (scrollOther)
		{
			ParentScrollRect.OnDrag(eventData);
		}
	}
}
