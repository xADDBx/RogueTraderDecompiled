using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("Layout/Extensions/Vertical Scroll Snap")]
public class VerticalScrollSnap : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	private Transform _screensContainer;

	private int _screens = 1;

	private bool _fastSwipeTimer;

	private int _fastSwipeCounter;

	private int _fastSwipeTarget = 30;

	private List<Vector3> _positions;

	private ScrollRect _scroll_rect;

	private Vector3 _lerp_target;

	private bool _lerp;

	[Tooltip("The gameobject that contains toggles which suggest pagination. (optional)")]
	public GameObject Pagination;

	[Tooltip("Button to go to the next page. (optional)")]
	public GameObject NextButton;

	[Tooltip("Button to go to the previous page. (optional)")]
	public GameObject PrevButton;

	[Tooltip("Transition speed between pages. (optional)")]
	public float transitionSpeed = 7.5f;

	public bool UseFastSwipe = true;

	public int FastSwipeThreshold = 100;

	private bool _startDrag = true;

	private Vector3 _startPosition;

	[Tooltip("The currently active page")]
	[SerializeField]
	private int _currentScreen;

	[Tooltip("The screen / page to start the control on")]
	public int StartingScreen = 1;

	[Tooltip("The distance between two pages, by default 3 times the width of the control")]
	public int PageStep;

	private bool fastSwipe;

	public int CurrentPage => _currentScreen;

	private void Start()
	{
		_scroll_rect = base.gameObject.GetComponent<ScrollRect>();
		if ((bool)_scroll_rect.horizontalScrollbar || (bool)_scroll_rect.verticalScrollbar)
		{
			Debug.LogWarning("Warning, using scrollbors with the Scroll Snap controls is not advised as it causes unpredictable results");
		}
		_screensContainer = _scroll_rect.content;
		if (PageStep == 0)
		{
			PageStep = (int)_scroll_rect.GetComponent<RectTransform>().rect.height * 3;
		}
		DistributePages();
		_lerp = false;
		_currentScreen = StartingScreen;
		_scroll_rect.verticalNormalizedPosition = (float)(_currentScreen - 1) / (float)(_screens - 1);
		ChangeBulletsInfo(_currentScreen);
		if ((bool)NextButton)
		{
			NextButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				NextScreen();
			});
		}
		if ((bool)PrevButton)
		{
			PrevButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				PreviousScreen();
			});
		}
	}

	private void Update()
	{
		if (_lerp)
		{
			_screensContainer.localPosition = Vector3.Lerp(_screensContainer.localPosition, _lerp_target, transitionSpeed * Time.deltaTime);
			if (Vector3.Distance(_screensContainer.localPosition, _lerp_target) < 0.005f)
			{
				_lerp = false;
			}
			if (Vector3.Distance(_screensContainer.localPosition, _lerp_target) < 10f)
			{
				ChangeBulletsInfo(CurrentScreen());
			}
		}
		if (_fastSwipeTimer)
		{
			_fastSwipeCounter++;
		}
	}

	public void NextScreen()
	{
		if (_currentScreen < _screens - 1)
		{
			_currentScreen++;
			_lerp = true;
			_lerp_target = _positions[_currentScreen];
			ChangeBulletsInfo(_currentScreen);
		}
	}

	public void PreviousScreen()
	{
		if (_currentScreen > 0)
		{
			_currentScreen--;
			_lerp = true;
			_lerp_target = _positions[_currentScreen];
			ChangeBulletsInfo(_currentScreen);
		}
	}

	public void GoToScreen(int screenIndex)
	{
		if (screenIndex <= _screens && screenIndex >= 0)
		{
			_lerp = true;
			_lerp_target = _positions[screenIndex];
			ChangeBulletsInfo(screenIndex);
		}
	}

	private void NextScreenCommand()
	{
		if (_currentScreen < _screens - 1)
		{
			_lerp = true;
			_lerp_target = _positions[_currentScreen + 1];
			ChangeBulletsInfo(_currentScreen + 1);
		}
	}

	private void PrevScreenCommand()
	{
		if (_currentScreen > 0)
		{
			_lerp = true;
			_lerp_target = _positions[_currentScreen - 1];
			ChangeBulletsInfo(_currentScreen - 1);
		}
	}

	private Vector3 FindClosestFrom(Vector3 start, List<Vector3> positions)
	{
		Vector3 result = Vector3.zero;
		float num = float.PositiveInfinity;
		foreach (Vector3 position in _positions)
		{
			if (Vector3.Distance(start, position) < num)
			{
				num = Vector3.Distance(start, position);
				result = position;
			}
		}
		return result;
	}

	public int CurrentScreen()
	{
		Vector3 pos = FindClosestFrom(_screensContainer.localPosition, _positions);
		return _currentScreen = GetPageforPosition(pos);
	}

	private void ChangeBulletsInfo(int currentScreen)
	{
		if ((bool)Pagination)
		{
			for (int i = 0; i < Pagination.transform.childCount; i++)
			{
				Pagination.transform.GetChild(i).GetComponent<Toggle>().isOn = ((currentScreen == i) ? true : false);
			}
		}
	}

	public void DistributePages()
	{
		float num = 0f;
		float num2 = 0f;
		Vector2 sizeDelta = base.gameObject.GetComponent<RectTransform>().sizeDelta;
		float num3 = 0f;
		for (int i = 0; i < _screensContainer.transform.childCount; i++)
		{
			RectTransform component = _screensContainer.transform.GetChild(i).gameObject.GetComponent<RectTransform>();
			num3 = num + (float)(i * PageStep);
			component.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y);
			component.anchoredPosition = new Vector2(0f - sizeDelta.x / 2f, num3 + sizeDelta.y / 2f);
		}
		num2 = num3 + num * -1f;
		_screensContainer.GetComponent<RectTransform>().offsetMax = new Vector2(0f, num2);
		_screens = _screensContainer.childCount;
		_positions = new List<Vector3>();
		if (_screens > 0)
		{
			for (int j = 0; j < _screens; j++)
			{
				_scroll_rect.verticalNormalizedPosition = (float)j / (float)(_screens - 1);
				_positions.Add(_screensContainer.localPosition);
			}
		}
	}

	private int GetPageforPosition(Vector3 pos)
	{
		for (int i = 0; i < _positions.Count; i++)
		{
			if (_positions[i] == pos)
			{
				return i;
			}
		}
		return 0;
	}

	private void OnValidate()
	{
		int childCount = base.gameObject.GetComponent<ScrollRect>().content.childCount;
		if (StartingScreen > childCount)
		{
			StartingScreen = childCount;
		}
		if (StartingScreen < 1)
		{
			StartingScreen = 1;
		}
	}

	public void AddChild(GameObject GO)
	{
		_scroll_rect.verticalNormalizedPosition = 0f;
		GO.transform.SetParent(_screensContainer);
		DistributePages();
		_scroll_rect.verticalNormalizedPosition = (float)_currentScreen / (float)(_screens - 1);
	}

	public void RemoveChild(int index, out GameObject ChildRemoved)
	{
		ChildRemoved = null;
		if (index < 0 || index > _screensContainer.childCount)
		{
			return;
		}
		_scroll_rect.verticalNormalizedPosition = 0f;
		Transform obj = _screensContainer.transform;
		int num = 0;
		foreach (Transform item in obj)
		{
			if (num == index)
			{
				item.SetParent(null);
				ChildRemoved = item.gameObject;
				break;
			}
			num++;
		}
		DistributePages();
		if (_currentScreen > _screens - 1)
		{
			_currentScreen = _screens - 1;
		}
		_scroll_rect.verticalNormalizedPosition = (float)_currentScreen / (float)(_screens - 1);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_startPosition = _screensContainer.localPosition;
		_fastSwipeCounter = 0;
		_fastSwipeTimer = true;
		_currentScreen = CurrentScreen();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_startDrag = true;
		if (!_scroll_rect.vertical)
		{
			return;
		}
		if (UseFastSwipe)
		{
			fastSwipe = false;
			_fastSwipeTimer = false;
			if (_fastSwipeCounter <= _fastSwipeTarget && Math.Abs(_startPosition.y - _screensContainer.localPosition.y) > (float)FastSwipeThreshold)
			{
				fastSwipe = true;
			}
			if (fastSwipe)
			{
				if (_startPosition.y - _screensContainer.localPosition.y > 0f)
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
				_lerp = true;
				_lerp_target = FindClosestFrom(_screensContainer.localPosition, _positions);
				_currentScreen = GetPageforPosition(_lerp_target);
			}
		}
		else
		{
			_lerp = true;
			_lerp_target = FindClosestFrom(_screensContainer.localPosition, _positions);
			_currentScreen = GetPageforPosition(_lerp_target);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		_lerp = false;
		if (_startDrag)
		{
			OnBeginDrag(eventData);
			_startDrag = false;
		}
	}
}
