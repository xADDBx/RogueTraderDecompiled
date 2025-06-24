using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class TurnVirtualListController : MonoBehaviour
{
	private enum VirtualListDirection
	{
		None,
		Vertical,
		Horizontal
	}

	[SerializeField]
	private VirtualListDirection m_VirtualDirection;

	[SerializeField]
	private bool m_KeepInvisibleItems;

	[Header("Content paddings")]
	[SerializeField]
	[UsedImplicitly]
	public RectOffset Padding;

	[SerializeField]
	[UsedImplicitly]
	public Vector2 Spacing;

	[Header("Scroll")]
	[SerializeField]
	[UsedImplicitly]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private float m_AutoScrollDelta = 1.25f;

	private ScrollBasePosition m_ForceScrollPosition;

	[Header("Item prefab")]
	public int ElementsInRow = 1;

	private List<ITurnVirtualItemData> DataList;

	private ITurnVirtualItemView m_Prefab;

	private bool m_IsListInit;

	public readonly List<ITurnVirtualItemView> VisibleItems = new List<ITurnVirtualItemView>();

	private int m_FirstItemIndex;

	private int m_LastItemIndex = -1;

	private const int DefaultPoolSize = 1;

	private readonly Queue<ITurnVirtualItemView> m_ItemsPool = new Queue<ITurnVirtualItemView>();

	[SerializeField]
	[UsedImplicitly]
	public float m_AnimationTime = 0.2f;

	public Action OnUpdatedCallback;

	private bool m_ExternalUpdateLock;

	private Sequence m_AnimationSequence;

	private GridConsoleNavigationBehaviour m_ConsoleNavigation;

	private Tweener m_ScrollTweener;

	private ITurnVirtualItemView m_LastSelectedView;

	private IViewModel m_LastSelectedVM;

	private int m_LastSelectedIndex;

	public ScrollRectExtended ScrollRect => m_ScrollRect;

	public RectTransform Content => m_ScrollRect.content;

	private float ContentPositionY => Content.anchoredPosition.y;

	private float ScrollRectHeight => m_ScrollRect.viewport.rect.height;

	private float ContentPositionX => Content.anchoredPosition.x;

	private float ScrollRectWidth => m_ScrollRect.viewport.rect.width;

	public Scrollbar Scrollbar
	{
		get
		{
			if (!(m_ScrollRect.verticalScrollbar != null))
			{
				return m_ScrollRect.horizontalScrollbar;
			}
			return m_ScrollRect.verticalScrollbar;
		}
	}

	private float m_CalculatedAnimationTime => m_AnimationTime / m_AnimationTimeDevider;

	private float m_AnimationTimeDevider => Game.Instance.Player.UISettings.TimeScaleAverage;

	public bool IsAnimating
	{
		get
		{
			if (m_AnimationSequence != null)
			{
				return m_AnimationSequence.IsPlaying();
			}
			return false;
		}
	}

	public void Initialize(ITurnVirtualItemView item)
	{
		if (!m_IsListInit)
		{
			m_Prefab = item;
			Content.pivot = new Vector2(0f, 1f);
			if (m_ScrollRect != null)
			{
				m_ScrollRect.viewport.pivot = new Vector2(0f, 1f);
			}
			m_ScrollRect.onValueChanged.AddListener(delegate(Vector2 data)
			{
				Scroll(data, fromMouse: true);
			});
			CleanList();
			m_IsListInit = true;
		}
	}

	public void CleanList()
	{
		m_FirstItemIndex = 0;
		m_LastItemIndex = -1;
		List<ITurnVirtualItemView> list = new List<ITurnVirtualItemView>();
		list.AddRange(VisibleItems);
		foreach (ITurnVirtualItemView item in list)
		{
			ReleaseItem(item);
		}
		VisibleItems.Clear();
		DataList = null;
	}

	public void UpdateData(List<ITurnVirtualItemData> newDataList, Sequence containerTweener, ScrollBasePosition forceScroll = ScrollBasePosition.None, Action callback = null)
	{
		m_ExternalUpdateLock = true;
		OnUpdatedCallback = callback;
		ScrollBasePosition forceScrollPosition = ((forceScroll == ScrollBasePosition.None) ? m_ForceScrollPosition : forceScroll);
		m_ForceScrollPosition = forceScrollPosition;
		Vector2 newContentSize = GetContentSize(newDataList);
		if (m_AnimationSequence != null)
		{
			m_AnimationSequence.Kill();
			m_AnimationSequence = null;
		}
		m_AnimationSequence = GetAnimationSequence(newDataList, newContentSize, containerTweener);
		DataList = new List<ITurnVirtualItemData>();
		DataList.AddRange(newDataList);
		if (m_AnimationSequence == null)
		{
			UpdateView();
			return;
		}
		m_AnimationSequence.Play().SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			UpdateContentSize(newContentSize);
			UpdateView();
		});
	}

	private void UpdateView()
	{
		m_AnimationSequence.Kill();
		m_AnimationSequence = null;
		Vector2 contentSize = GetContentSize(DataList);
		UpdateContentSize(contentSize);
		UpdateScrollPosition();
		UpdateInternal();
		UpdateNavigation();
		if (OnUpdatedCallback != null)
		{
			OnUpdatedCallback();
			OnUpdatedCallback = null;
		}
		m_ExternalUpdateLock = false;
	}

	public Vector2 GetContentSize(List<ITurnVirtualItemData> dataList)
	{
		if (dataList == null || !dataList.Any())
		{
			return default(Vector2);
		}
		ITurnVirtualItemData turnVirtualItemData = dataList[dataList.Count - 1];
		float y = (turnVirtualItemData.VirtualPosition + turnVirtualItemData.VirtualSize).y + (float)Padding.top + (float)Padding.bottom + Spacing.x * (float)(dataList.Count - 1);
		return new Vector2((turnVirtualItemData.VirtualPosition + turnVirtualItemData.VirtualSize).x + (float)Padding.left + (float)Padding.right + Spacing.y * (float)(dataList.Count - 1), y);
	}

	protected void UpdateContentSize(Vector2 currentContentSize)
	{
		if (m_IsListInit)
		{
			Content.sizeDelta = currentContentSize;
		}
	}

	protected void UpdateScrollPosition()
	{
		switch (m_ForceScrollPosition)
		{
		case ScrollBasePosition.None:
			break;
		case ScrollBasePosition.Top:
			m_ScrollRect?.ScrollToTop();
			break;
		case ScrollBasePosition.Bottom:
			m_ScrollRect?.ScrollToBottom();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected Tweener GetScrollTweenSequence()
	{
		Tweener tweener = null;
		if (!m_ScrollRect.verticalScrollbar.enabled)
		{
			return null;
		}
		return m_ForceScrollPosition switch
		{
			ScrollBasePosition.None => null, 
			ScrollBasePosition.Top => DOTween.To(delegate(float x)
			{
				m_ScrollRect.verticalNormalizedPosition = x;
			}, m_ScrollRect.verticalNormalizedPosition, 1f, m_CalculatedAnimationTime).Pause(), 
			ScrollBasePosition.Bottom => DOTween.To(delegate(float x)
			{
				m_ScrollRect.verticalNormalizedPosition = x;
			}, m_ScrollRect.verticalNormalizedPosition, 0f, m_CalculatedAnimationTime).Pause(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private Sequence GetContentSizeTweenSequence(Vector2 newContentSize)
	{
		Sequence sequence = null;
		if (!newContentSize.Equals(Content.sizeDelta))
		{
			sequence = DOTween.Sequence().Pause();
			Tweener t = Content.DOSizeDelta(newContentSize, m_CalculatedAnimationTime).Pause().OnUpdate(UpdateScrollPosition);
			sequence.Join(t);
		}
		return sequence;
	}

	private Sequence GetAnimationSequence(List<ITurnVirtualItemData> newDataList, Vector2 newContentSize, Sequence containerTweener)
	{
		Sequence sequence = DOTween.Sequence().Pause();
		if (DataList == null)
		{
			sequence.Join(containerTweener);
			return sequence;
		}
		UpdateVisibleRange(newDataList);
		List<ITurnVirtualItemData> itemsToHide = GetItemsToHide(newDataList);
		Dictionary<ITurnVirtualItemData, Vector2> itemsToMove = GetItemsToMove(newDataList);
		List<ITurnVirtualItemData> list = GetItemsToShow(newDataList).ToList();
		if (!itemsToHide.Any() && !itemsToMove.Any() && !list.Any())
		{
			return null;
		}
		if (itemsToHide.Any())
		{
			Sequence sequence2 = DOTween.Sequence().Pause().OnStart(delegate
			{
			});
			foreach (ITurnVirtualItemData item in itemsToHide)
			{
				Sequence hideAnimation = item.BoundView.GetHideAnimation(delegate
				{
					ReleaseItem(item.BoundView);
				});
				sequence2.Join(hideAnimation);
			}
			sequence.Append(sequence2);
		}
		if (itemsToMove.Any())
		{
			Sequence sequence3 = DOTween.Sequence().Pause().OnStart(delegate
			{
			});
			foreach (KeyValuePair<ITurnVirtualItemData, Vector2> item2 in itemsToMove)
			{
				Vector2 targetPosition = new Vector2(item2.Value.x + (float)Padding.left, 0f - (item2.Value.y + (float)Padding.top));
				Sequence moveAnimation = item2.Key.BoundView.GetMoveAnimation(null, targetPosition);
				sequence3.Join(moveAnimation);
			}
			sequence.Append(sequence3);
		}
		Sequence contentSizeTweenSequence = GetContentSizeTweenSequence(newContentSize);
		if (contentSizeTweenSequence != null)
		{
			sequence.Join(contentSizeTweenSequence);
		}
		sequence.Join(containerTweener);
		if (list.Any())
		{
			Sequence sequence4 = DOTween.Sequence().Pause().OnStart(delegate
			{
			});
			foreach (ITurnVirtualItemData item3 in list)
			{
				ITurnVirtualItemView turnVirtualItemView = ClaimItemView();
				SetupItemView(turnVirtualItemView, item3);
				turnVirtualItemView.Selectable?.SetFocus(value: false);
				turnVirtualItemView.CanvasGroup.alpha = 0f;
				Sequence showAnimation = turnVirtualItemView.GetShowAnimation(null, new Vector2(item3.VirtualPosition.x + (float)Padding.left, 0f - (item3.VirtualPosition.y + (float)Padding.top)));
				sequence4.Join(showAnimation);
			}
			sequence.Append(sequence4);
		}
		return sequence;
	}

	private List<ITurnVirtualItemData> GetItemsToHide(List<ITurnVirtualItemData> newDataList)
	{
		return DataList.Where((ITurnVirtualItemData itemData) => itemData.BoundView != null && !newDataList.Any((ITurnVirtualItemData newItemData) => newItemData.ViewModel == itemData.ViewModel)).ToList();
	}

	private Dictionary<ITurnVirtualItemData, Vector2> GetItemsToMove(List<ITurnVirtualItemData> newDataList)
	{
		Dictionary<ITurnVirtualItemData, Vector2> dictionary = new Dictionary<ITurnVirtualItemData, Vector2>();
		foreach (ITurnVirtualItemData itemData in DataList)
		{
			if (itemData.BoundView != null)
			{
				ITurnVirtualItemData turnVirtualItemData = newDataList.FirstOrDefault((ITurnVirtualItemData item) => item.ViewModel == itemData.ViewModel);
				if (turnVirtualItemData != null && !itemData.VirtualPosition.Equals(turnVirtualItemData.VirtualPosition))
				{
					dictionary[itemData] = turnVirtualItemData.VirtualPosition;
				}
			}
		}
		return dictionary;
	}

	private List<ITurnVirtualItemData> GetItemsToShow(List<ITurnVirtualItemData> newDataList)
	{
		return newDataList.Where((ITurnVirtualItemData itemData) => IsVisible(itemData, newDataList) && DataList.FirstOrDefault((ITurnVirtualItemData newItemData) => newItemData.ViewModel == itemData.ViewModel) == null).ToList();
	}

	public bool IsVisible(ITurnVirtualItemData item, List<ITurnVirtualItemData> dataList = null)
	{
		int num = dataList?.IndexOf(item) ?? DataList?.IndexOf(item) ?? (-100);
		if (m_FirstItemIndex <= num)
		{
			return num <= m_LastItemIndex;
		}
		return false;
	}

	private void UpdateVisibleRange(List<ITurnVirtualItemData> dataList)
	{
		m_FirstItemIndex = UpdateFirstItemIndex(dataList);
		m_LastItemIndex = UpdateLastItemIndex(dataList);
	}

	private void UpdateInternal()
	{
		if (DataList == null)
		{
			return;
		}
		UpdateVisibleRange(DataList);
		if (m_FirstItemIndex < 0 || m_LastItemIndex < 0)
		{
			return;
		}
		List<ITurnVirtualItemData> list = (m_KeepInvisibleItems ? DataList.ToList() : DataList.GetRange(m_FirstItemIndex, m_LastItemIndex - m_FirstItemIndex + 1));
		Queue<ITurnVirtualItemView> queue = new Queue<ITurnVirtualItemView>();
		foreach (ITurnVirtualItemView visibleItem in VisibleItems)
		{
			ITurnVirtualItemData turnVirtualItemData = list.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == visibleItem.GetViewModel());
			if (turnVirtualItemData != null)
			{
				if (turnVirtualItemData.BoundView == null)
				{
					SetupItemView(visibleItem, turnVirtualItemData);
				}
				else if (m_AnimationSequence == null)
				{
					visibleItem.SetAnchoredPosition(new Vector2(turnVirtualItemData.VirtualPosition.x + (float)Padding.left, 0f - (turnVirtualItemData.VirtualPosition.y + (float)Padding.top)));
				}
				list.Remove(turnVirtualItemData);
			}
			else
			{
				queue.Enqueue(visibleItem);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (i < queue.Count)
			{
				ITurnVirtualItemView turnVirtualItemView = queue.Dequeue();
				turnVirtualItemView.WillBeReused = true;
				SetupItemView(turnVirtualItemView, list[i]);
				turnVirtualItemView.WillBeReused = false;
			}
			else
			{
				ITurnVirtualItemView view = ClaimItemView();
				SetupItemView(view, list[i]);
			}
		}
		foreach (ITurnVirtualItemView item in queue)
		{
			ReleaseItem(item);
		}
		if (!m_KeepInvisibleItems)
		{
			return;
		}
		for (int j = 0; j < DataList.Count; j++)
		{
			ITurnVirtualItemView boundView = DataList[j].BoundView;
			float alpha = boundView.CanvasGroup.alpha;
			if (alpha == 0f || alpha == 1f)
			{
				boundView.CanvasGroup.alpha = ((j >= m_FirstItemIndex && j <= m_LastItemIndex) ? 1f : 0f);
			}
		}
	}

	private void UpdateSiblingIndexes()
	{
		for (int i = 0; i < DataList.Count; i++)
		{
			ITurnVirtualItemData turnVirtualItemData = DataList[i];
			if (turnVirtualItemData.BoundView != null)
			{
				turnVirtualItemData.BoundView.RectTransform.transform.SetAsLastSibling();
				turnVirtualItemData.BoundView.RectTransform.gameObject.name = $"{m_Prefab.RectTransform.name} {i}";
			}
		}
	}

	private int UpdateLastItemIndex(List<ITurnVirtualItemData> dataList)
	{
		int num = 0;
		if (!m_ScrollRect)
		{
			return dataList.Count - 1;
		}
		if (dataList == null || dataList.Count == 0)
		{
			return -1;
		}
		if (num < 0)
		{
			num = 0;
		}
		if (num >= dataList.Count)
		{
			num = dataList.Count - 1;
		}
		switch (m_VirtualDirection)
		{
		case VirtualListDirection.None:
			num = dataList.Count - 1;
			break;
		case VirtualListDirection.Vertical:
			num = GetLastItemIndexInVertical(dataList, num);
			break;
		case VirtualListDirection.Horizontal:
			num = GetLastItemIndexInHorizontal(dataList, num);
			break;
		}
		return num;
	}

	private int GetLastItemIndexInVertical(List<ITurnVirtualItemData> dataList, int lastItemIndex)
	{
		float num = ContentPositionY + ScrollRectHeight;
		ITurnVirtualItemData turnVirtualItemData = dataList[lastItemIndex];
		while (turnVirtualItemData.VirtualPosition.y < num && lastItemIndex < dataList.Count - 1)
		{
			lastItemIndex++;
			turnVirtualItemData = dataList[lastItemIndex];
		}
		while (turnVirtualItemData.VirtualPosition.y > num && lastItemIndex > 0)
		{
			lastItemIndex--;
			turnVirtualItemData = dataList[lastItemIndex];
		}
		return lastItemIndex;
	}

	private int GetLastItemIndexInHorizontal(List<ITurnVirtualItemData> dataList, int lastItemIndex)
	{
		float num = ContentPositionX + ScrollRectWidth;
		ITurnVirtualItemData turnVirtualItemData = dataList[lastItemIndex];
		while (turnVirtualItemData.VirtualPosition.x < num && lastItemIndex < dataList.Count - 1)
		{
			lastItemIndex++;
			turnVirtualItemData = dataList[lastItemIndex];
		}
		while (turnVirtualItemData.VirtualPosition.x > num && lastItemIndex > 0)
		{
			lastItemIndex--;
			turnVirtualItemData = dataList[lastItemIndex];
		}
		return lastItemIndex;
	}

	private int UpdateFirstItemIndex(List<ITurnVirtualItemData> dataList)
	{
		int num = -1;
		if (!m_ScrollRect)
		{
			return 0;
		}
		if (dataList == null || dataList.Count == 0)
		{
			return 0;
		}
		if (num < 0)
		{
			num = 0;
		}
		if (num >= dataList.Count)
		{
			num = dataList.Count - 1;
		}
		switch (m_VirtualDirection)
		{
		case VirtualListDirection.None:
			num = 0;
			break;
		case VirtualListDirection.Vertical:
			num = GetFirstItemIndexVertical(dataList, num);
			break;
		case VirtualListDirection.Horizontal:
			num = GetFirstItemIndexHorizontal(dataList, num);
			break;
		}
		return num;
	}

	private int GetFirstItemIndexVertical(List<ITurnVirtualItemData> dataList, int firstItemIndex)
	{
		float contentPositionY = ContentPositionY;
		ITurnVirtualItemData turnVirtualItemData = dataList[firstItemIndex];
		while (turnVirtualItemData.VirtualPosition.y + turnVirtualItemData.VirtualSize.y > contentPositionY - turnVirtualItemData.VirtualSize.y - Spacing.y && firstItemIndex > 0)
		{
			firstItemIndex--;
			turnVirtualItemData = dataList[firstItemIndex];
		}
		while (turnVirtualItemData.VirtualPosition.y + turnVirtualItemData.VirtualSize.y < contentPositionY - turnVirtualItemData.VirtualSize.y - Spacing.y && firstItemIndex < dataList.Count - 1)
		{
			firstItemIndex++;
			turnVirtualItemData = dataList[firstItemIndex];
		}
		return firstItemIndex;
	}

	private int GetFirstItemIndexHorizontal(List<ITurnVirtualItemData> dataList, int firstItemIndex)
	{
		float contentPositionY = ContentPositionY;
		ITurnVirtualItemData turnVirtualItemData = dataList[firstItemIndex];
		while (turnVirtualItemData.VirtualPosition.y + turnVirtualItemData.VirtualSize.y > contentPositionY - turnVirtualItemData.VirtualSize.y - Spacing.y && firstItemIndex > 0)
		{
			firstItemIndex--;
			turnVirtualItemData = dataList[firstItemIndex];
		}
		while (turnVirtualItemData.VirtualPosition.y + turnVirtualItemData.VirtualSize.y < contentPositionY - turnVirtualItemData.VirtualSize.y - Spacing.y && firstItemIndex < dataList.Count - 1)
		{
			firstItemIndex++;
			turnVirtualItemData = dataList[firstItemIndex];
		}
		return firstItemIndex;
	}

	private ITurnVirtualItemView ClaimItemView()
	{
		ITurnVirtualItemView turnVirtualItemView = ((m_ItemsPool.Count > 0) ? m_ItemsPool.Dequeue() : CreateItemView());
		turnVirtualItemView.View.gameObject.SetActive(value: true);
		turnVirtualItemView.RectTransform.pivot = new Vector2(0f, 1f);
		turnVirtualItemView.CanvasGroup.alpha = 1f;
		VisibleItems.Add(turnVirtualItemView);
		return turnVirtualItemView;
	}

	private ITurnVirtualItemView CreateItemView()
	{
		return UnityEngine.Object.Instantiate(m_Prefab.View, Content, worldPositionStays: false) as ITurnVirtualItemView;
	}

	private void SetupItemView(ITurnVirtualItemView view, ITurnVirtualItemData data)
	{
		view.ViewBind(data);
		data.BoundView = view;
		view.SetAnchoredPosition(new Vector2(data.VirtualPosition.x + (float)Padding.left, 0f - (data.VirtualPosition.y + (float)Padding.top)));
		view.Selectable?.SetFocus(value: false);
		view.CanvasGroup.alpha = 1f;
	}

	private void ReleaseItem(ITurnVirtualItemView view)
	{
		if (m_ItemsPool.Contains(view))
		{
			PFLog.UI.Warning("Attempt to realese ITurnVirtualItemView twice");
			return;
		}
		view.DestroyViewItem();
		view.RectTransform.anchoredPosition = new Vector2(view.RectTransform.anchoredPosition.x - 100000f, 100000f);
		view.Selectable?.SetFocus(value: false);
		view.View.gameObject.SetActive(value: false);
		VisibleItems.Remove(view);
		m_ItemsPool.Enqueue(view);
	}

	public void FillNavigationBehaviour(GridConsoleNavigationBehaviour consoleNavigationBehavior)
	{
		if (consoleNavigationBehavior == null && m_ConsoleNavigation != null)
		{
			m_ConsoleNavigation.UnFocusCurrentEntity();
		}
		m_ConsoleNavigation = consoleNavigationBehavior;
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_ConsoleNavigation == null)
		{
			return;
		}
		m_ConsoleNavigation.SetEntitiesGrid(GetNavigationEntities(), ElementsInRow);
		if (DataList != null && DataList.Any())
		{
			ITurnVirtualItemData selectedData = DataList.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == m_LastSelectedVM);
			if (selectedData == null)
			{
				selectedData = ((m_LastSelectedIndex >= 0 && m_LastSelectedIndex < DataList.Count) ? DataList[m_LastSelectedIndex] : DataList.Last());
			}
			IConsoleNavigationEntity entity = (IConsoleNavigationEntity)VisibleItems.FirstOrDefault((ITurnVirtualItemView view) => view.GetViewModel() == selectedData.ViewModel);
			m_ConsoleNavigation.FocusOnEntityManual(entity);
			if (!m_ConsoleNavigation.IsFocused)
			{
				m_ConsoleNavigation.UnFocusCurrentEntity();
			}
		}
	}

	private List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return (from view in VisibleItems.ToList()
			orderby view.RectTransform.position.y, -1f * view.RectTransform.position.x
			select view).Reverse().Cast<IConsoleNavigationEntity>().ToList();
	}

	public bool ScrollTo(ITurnVirtualItemData virtualData)
	{
		if (m_ExternalUpdateLock)
		{
			return false;
		}
		if (!DataList.Contains(virtualData))
		{
			return false;
		}
		if (Content == null)
		{
			return false;
		}
		float num = Content.rect.height - m_ScrollRect.viewport.rect.height;
		float num2 = Mathf.Max(Mathf.Min(num, virtualData.VirtualPosition.y - virtualData.VirtualSize.y * m_AutoScrollDelta), 0f);
		m_ForceScrollPosition = ScrollBasePosition.None;
		float endValue = 1f - num2 / num;
		if (m_ScrollTweener != null)
		{
			m_ScrollTweener.Kill();
		}
		m_ScrollTweener = DOTween.To(() => m_ScrollRect.verticalNormalizedPosition, delegate(float x)
		{
			m_ScrollRect.verticalNormalizedPosition = x;
		}, endValue, 0.2f).SetUpdate(isIndependentUpdate: true).OnUpdate(delegate
		{
			UpdateView();
		})
			.OnComplete(delegate
			{
				UpdateView();
			});
		return true;
	}

	public void ScrollToSmoothly(ScrollBasePosition scrollPosition)
	{
		float verticalNormalizedPosition = m_ScrollRect.verticalNormalizedPosition;
		m_ScrollTweener = DOTween.To(endValue: scrollPosition switch
		{
			ScrollBasePosition.Top => 1f, 
			ScrollBasePosition.Bottom => 0f, 
			_ => throw new ArgumentOutOfRangeException("scrollPosition", scrollPosition, null), 
		}, getter: () => m_ScrollRect.verticalNormalizedPosition, setter: delegate(float x)
		{
			m_ScrollRect.verticalNormalizedPosition = x;
		}, duration: 0.2f).SetUpdate(isIndependentUpdate: true).OnUpdate(delegate
		{
			UpdateView();
		})
			.OnComplete(delegate
			{
				UpdateView();
			});
	}

	public void ScrollTo(ScrollBasePosition scrollPosition)
	{
		if (!m_ExternalUpdateLock)
		{
			m_ForceScrollPosition = scrollPosition;
			UpdateView();
		}
	}

	public void Scroll(Vector2 data, bool fromMouse = false)
	{
		if (m_ExternalUpdateLock)
		{
			return;
		}
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		if (m_ScrollRect == null)
		{
			return;
		}
		if (!fromMouse)
		{
			float scrollSensitivity = m_ScrollRect.scrollSensitivity;
			switch (m_VirtualDirection)
			{
			case VirtualListDirection.None:
				pointerEventData.scrollDelta = new Vector2(data.x * scrollSensitivity, data.y * scrollSensitivity);
				break;
			case VirtualListDirection.Vertical:
				pointerEventData.scrollDelta = new Vector2(0f, data.y * scrollSensitivity);
				break;
			case VirtualListDirection.Horizontal:
				pointerEventData.scrollDelta = new Vector2(data.x * scrollSensitivity, 0f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			m_ScrollRect.OnScroll(pointerEventData);
		}
		m_ForceScrollPosition = ScrollBasePosition.None;
		UpdateView();
	}

	public void EntitySelected(IConsoleNavigationEntity entity)
	{
		ITurnVirtualItemView virtualItemView = entity as ITurnVirtualItemView;
		if (virtualItemView != null && !(m_ScrollRect == null) && m_LastSelectedView != entity)
		{
			m_LastSelectedIndex = DataList.IndexOf(DataList.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == virtualItemView.GetViewModel()));
			if (m_LastSelectedIndex >= 0)
			{
				m_LastSelectedView = virtualItemView;
				m_LastSelectedVM = virtualItemView.GetViewModel();
			}
			else
			{
				m_LastSelectedVM = null;
			}
			if (m_ScrollRect.EnsureVisibleVertical(virtualItemView.RectTransform, virtualItemView.RectTransform.rect.height * m_AutoScrollDelta))
			{
				m_ForceScrollPosition = ScrollBasePosition.None;
				UpdateView();
			}
			if (m_LastSelectedIndex < 0)
			{
				m_ConsoleNavigation.FocusOnLastValidEntity();
				m_ConsoleNavigation.UnFocusCurrentEntity();
			}
		}
	}
}
