using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("UI/Extensions/Scroll Snap")]
public class ScrollSnap : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	public enum ScrollDirection
	{
		Horizontal,
		Vertical
	}

	public delegate void PageSnapChange(int page);

	public ScrollDirection direction;

	protected ScrollRect scrollRect;

	protected RectTransform scrollRectTransform;

	protected Transform listContainerTransform;

	protected RectTransform rectTransform;

	private int pages;

	protected int startingPage;

	protected Vector3[] pageAnchorPositions;

	protected Vector3 lerpTarget;

	protected bool lerp;

	protected float listContainerMinPosition;

	protected float listContainerMaxPosition;

	protected float listContainerSize;

	protected RectTransform listContainerRectTransform;

	protected Vector2 listContainerCachedSize;

	protected float itemSize;

	protected int itemsCount;

	[Tooltip("Button to go to the next page. (optional)")]
	public Button nextButton;

	[Tooltip("Button to go to the previous page. (optional)")]
	public Button prevButton;

	[Tooltip("Number of items visible in one page of scroll frame.")]
	[Range(1f, 100f)]
	public int itemsVisibleAtOnce = 1;

	[Tooltip("Sets minimum width of list items to 1/itemsVisibleAtOnce.")]
	public bool autoLayoutItems = true;

	[Tooltip("If you wish to update scrollbar numberOfSteps to number of active children on list.")]
	public bool linkScrolbarSteps;

	[Tooltip("If you wish to update scrollrect sensitivity to size of list element.")]
	public bool linkScrolrectScrollSensitivity;

	public bool useFastSwipe = true;

	public int fastSwipeThreshold = 100;

	protected bool startDrag = true;

	protected Vector3 positionOnDragStart;

	protected int pageOnDragStart;

	protected bool fastSwipeTimer;

	protected int fastSwipeCounter;

	protected int fastSwipeTarget = 10;

	private bool fastSwipe;

	public event PageSnapChange onPageChange;

	private void Awake()
	{
		lerp = false;
		scrollRect = base.gameObject.GetComponent<ScrollRect>();
		scrollRectTransform = base.gameObject.GetComponent<RectTransform>();
		listContainerTransform = scrollRect.content;
		listContainerRectTransform = listContainerTransform.GetComponent<RectTransform>();
		rectTransform = listContainerTransform.gameObject.GetComponent<RectTransform>();
		UpdateListItemsSize();
		UpdateListItemPositions();
		PageChanged(CurrentPage());
		if ((bool)nextButton)
		{
			nextButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				NextScreen();
			});
		}
		if ((bool)prevButton)
		{
			prevButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				PreviousScreen();
			});
		}
	}

	private void Start()
	{
		Awake();
	}

	public void UpdateListItemsSize()
	{
		float num = 0f;
		float num2 = 0f;
		if (direction == ScrollDirection.Horizontal)
		{
			num = scrollRectTransform.rect.width / (float)itemsVisibleAtOnce;
			num2 = listContainerRectTransform.rect.width / (float)itemsCount;
		}
		else
		{
			num = scrollRectTransform.rect.height / (float)itemsVisibleAtOnce;
			num2 = listContainerRectTransform.rect.height / (float)itemsCount;
		}
		itemSize = num;
		if (linkScrolrectScrollSensitivity)
		{
			scrollRect.scrollSensitivity = itemSize;
		}
		if (!autoLayoutItems || num2 == num || itemsCount <= 0)
		{
			return;
		}
		if (direction == ScrollDirection.Horizontal)
		{
			foreach (Transform item in listContainerTransform)
			{
				GameObject gameObject = item.gameObject;
				if (gameObject.activeInHierarchy)
				{
					LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
					if (layoutElement == null)
					{
						layoutElement = gameObject.AddComponent<LayoutElement>();
					}
					layoutElement.minWidth = itemSize;
				}
			}
			return;
		}
		foreach (Transform item2 in listContainerTransform)
		{
			GameObject gameObject2 = item2.gameObject;
			if (gameObject2.activeInHierarchy)
			{
				LayoutElement layoutElement2 = gameObject2.GetComponent<LayoutElement>();
				if (layoutElement2 == null)
				{
					layoutElement2 = gameObject2.AddComponent<LayoutElement>();
				}
				layoutElement2.minHeight = itemSize;
			}
		}
	}

	public void UpdateListItemPositions()
	{
		if (listContainerRectTransform.rect.size.Equals(listContainerCachedSize))
		{
			return;
		}
		int num = 0;
		foreach (Transform item in listContainerTransform)
		{
			if (item.gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		itemsCount = 0;
		Array.Resize(ref pageAnchorPositions, num);
		if (num > 0)
		{
			pages = Mathf.Max(num - itemsVisibleAtOnce + 1, 1);
			if (direction == ScrollDirection.Horizontal)
			{
				scrollRect.horizontalNormalizedPosition = 0f;
				listContainerMaxPosition = listContainerTransform.localPosition.x;
				scrollRect.horizontalNormalizedPosition = 1f;
				listContainerMinPosition = listContainerTransform.localPosition.x;
				listContainerSize = listContainerMaxPosition - listContainerMinPosition;
				for (int i = 0; i < pages; i++)
				{
					pageAnchorPositions[i] = new Vector3(listContainerMaxPosition - itemSize * (float)i, listContainerTransform.localPosition.y, listContainerTransform.localPosition.z);
				}
			}
			else
			{
				scrollRect.verticalNormalizedPosition = 1f;
				listContainerMinPosition = listContainerTransform.localPosition.y;
				scrollRect.verticalNormalizedPosition = 0f;
				listContainerMaxPosition = listContainerTransform.localPosition.y;
				listContainerSize = listContainerMaxPosition - listContainerMinPosition;
				for (int j = 0; j < pages; j++)
				{
					pageAnchorPositions[j] = new Vector3(listContainerTransform.localPosition.x, listContainerMinPosition + itemSize * (float)j, listContainerTransform.localPosition.z);
				}
			}
			UpdateScrollbar(linkScrolbarSteps);
			startingPage = Mathf.Min(startingPage, pages);
			ResetPage();
		}
		if (itemsCount != num)
		{
			PageChanged(CurrentPage());
		}
		itemsCount = num;
		listContainerCachedSize.Set(listContainerRectTransform.rect.size.x, listContainerRectTransform.rect.size.y);
	}

	public void ResetPage()
	{
		if (direction == ScrollDirection.Horizontal)
		{
			scrollRect.horizontalNormalizedPosition = ((pages > 1) ? ((float)startingPage / (float)(pages - 1)) : 0f);
		}
		else
		{
			scrollRect.verticalNormalizedPosition = ((pages > 1) ? ((float)(pages - startingPage - 1) / (float)(pages - 1)) : 0f);
		}
	}

	protected void UpdateScrollbar(bool linkSteps)
	{
		if (linkSteps)
		{
			if (direction == ScrollDirection.Horizontal)
			{
				if (scrollRect.horizontalScrollbar != null)
				{
					scrollRect.horizontalScrollbar.numberOfSteps = pages;
				}
			}
			else if (scrollRect.verticalScrollbar != null)
			{
				scrollRect.verticalScrollbar.numberOfSteps = pages;
			}
		}
		else if (direction == ScrollDirection.Horizontal)
		{
			if (scrollRect.horizontalScrollbar != null)
			{
				scrollRect.horizontalScrollbar.numberOfSteps = 0;
			}
		}
		else if (scrollRect.verticalScrollbar != null)
		{
			scrollRect.verticalScrollbar.numberOfSteps = 0;
		}
	}

	private void LateUpdate()
	{
		UpdateListItemsSize();
		UpdateListItemPositions();
		if (lerp)
		{
			UpdateScrollbar(linkSteps: false);
			listContainerTransform.localPosition = Vector3.Lerp(listContainerTransform.localPosition, lerpTarget, 7.5f * Time.deltaTime);
			if (Vector3.Distance(listContainerTransform.localPosition, lerpTarget) < 0.001f)
			{
				listContainerTransform.localPosition = lerpTarget;
				lerp = false;
				UpdateScrollbar(linkScrolbarSteps);
			}
			if (Vector3.Distance(listContainerTransform.localPosition, lerpTarget) < 10f)
			{
				PageChanged(CurrentPage());
			}
		}
		if (fastSwipeTimer)
		{
			fastSwipeCounter++;
		}
	}

	public void NextScreen()
	{
		UpdateListItemPositions();
		if (CurrentPage() < pages - 1)
		{
			lerp = true;
			lerpTarget = pageAnchorPositions[CurrentPage() + 1];
			PageChanged(CurrentPage() + 1);
		}
	}

	public void PreviousScreen()
	{
		UpdateListItemPositions();
		if (CurrentPage() > 0)
		{
			lerp = true;
			lerpTarget = pageAnchorPositions[CurrentPage() - 1];
			PageChanged(CurrentPage() - 1);
		}
	}

	private void NextScreenCommand()
	{
		if (pageOnDragStart < pages - 1)
		{
			int num = Mathf.Min(pages - 1, pageOnDragStart + itemsVisibleAtOnce);
			lerp = true;
			lerpTarget = pageAnchorPositions[num];
			PageChanged(num);
		}
	}

	private void PrevScreenCommand()
	{
		if (pageOnDragStart > 0)
		{
			int num = Mathf.Max(0, pageOnDragStart - itemsVisibleAtOnce);
			lerp = true;
			lerpTarget = pageAnchorPositions[num];
			PageChanged(num);
		}
	}

	public int CurrentPage()
	{
		float value;
		if (direction == ScrollDirection.Horizontal)
		{
			value = listContainerMaxPosition - listContainerTransform.localPosition.x;
			value = Mathf.Clamp(value, 0f, listContainerSize);
		}
		else
		{
			value = listContainerTransform.localPosition.y - listContainerMinPosition;
			value = Mathf.Clamp(value, 0f, listContainerSize);
		}
		return Mathf.Clamp(Mathf.RoundToInt(value / itemSize), 0, pages);
	}

	public void ChangePage(int page)
	{
		if (0 <= page && page < pages)
		{
			lerp = true;
			lerpTarget = pageAnchorPositions[page];
			PageChanged(page);
		}
	}

	private void PageChanged(int currentPage)
	{
		startingPage = currentPage;
		if ((bool)nextButton)
		{
			nextButton.interactable = currentPage < pages - 1;
		}
		if ((bool)prevButton)
		{
			prevButton.interactable = currentPage > 0;
		}
		if (this.onPageChange != null)
		{
			this.onPageChange(currentPage);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		UpdateScrollbar(linkSteps: false);
		fastSwipeCounter = 0;
		fastSwipeTimer = true;
		positionOnDragStart = eventData.position;
		pageOnDragStart = CurrentPage();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		startDrag = true;
		float num = 0f;
		num = ((direction != 0) ? (0f - positionOnDragStart.y + eventData.position.y) : (positionOnDragStart.x - eventData.position.x));
		if (useFastSwipe)
		{
			fastSwipe = false;
			fastSwipeTimer = false;
			if (fastSwipeCounter <= fastSwipeTarget && Math.Abs(num) > (float)fastSwipeThreshold)
			{
				fastSwipe = true;
			}
			if (fastSwipe)
			{
				if (num > 0f)
				{
					NextScreenCommand();
				}
				else
				{
					PrevScreenCommand();
				}
			}
			else
			{
				lerp = true;
				lerpTarget = pageAnchorPositions[CurrentPage()];
			}
		}
		else
		{
			lerp = true;
			lerpTarget = pageAnchorPositions[CurrentPage()];
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		lerp = false;
		if (startDrag)
		{
			OnBeginDrag(eventData);
			startDrag = false;
		}
	}
}
