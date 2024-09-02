using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
public class UIScrollToSelectionXY : MonoBehaviour
{
	public float scrollSpeed = 10f;

	[SerializeField]
	private RectTransform layoutListGroup;

	private RectTransform targetScrollObject;

	private bool scrollToSelection = true;

	private RectTransform scrollWindow;

	private RectTransform currentCanvas;

	private ScrollRect targetScrollRect;

	private void Start()
	{
		targetScrollRect = GetComponent<ScrollRect>();
		scrollWindow = targetScrollRect.GetComponent<RectTransform>();
	}

	private void Update()
	{
		ScrollRectToLevelSelection();
	}

	private void ScrollRectToLevelSelection()
	{
		EventSystem current = EventSystem.current;
		if (targetScrollRect == null || layoutListGroup == null || scrollWindow == null)
		{
			return;
		}
		RectTransform rectTransform = ((current.currentSelectedGameObject != null) ? current.currentSelectedGameObject.GetComponent<RectTransform>() : null);
		if (rectTransform != targetScrollObject)
		{
			scrollToSelection = true;
		}
		if (!(rectTransform == null) && scrollToSelection && !(rectTransform.transform.parent != layoutListGroup.transform))
		{
			bool flag = false;
			bool flag2 = false;
			if (targetScrollRect.vertical)
			{
				float num = 0f - rectTransform.anchoredPosition.y;
				float y = layoutListGroup.anchoredPosition.y;
				float num2 = 0f;
				num2 = y - num;
				targetScrollRect.verticalNormalizedPosition += num2 / layoutListGroup.sizeDelta.y * Time.deltaTime * scrollSpeed;
				flag2 = Mathf.Abs(num2) < 2f;
			}
			if (targetScrollRect.horizontal)
			{
				float num3 = 0f - rectTransform.anchoredPosition.x;
				float x = layoutListGroup.anchoredPosition.x;
				float num4 = 0f;
				num4 = x - num3;
				targetScrollRect.horizontalNormalizedPosition += num4 / layoutListGroup.sizeDelta.x * Time.deltaTime * scrollSpeed;
				flag = Mathf.Abs(num4) < 2f;
			}
			if (flag && flag2)
			{
				scrollToSelection = false;
			}
			targetScrollObject = rectTransform;
		}
	}
}
