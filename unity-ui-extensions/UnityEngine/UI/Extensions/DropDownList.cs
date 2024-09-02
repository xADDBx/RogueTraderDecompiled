using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("UI/Extensions/Dropdown List")]
public class DropDownList : MonoBehaviour
{
	public Color disabledTextColor;

	public List<DropDownListItem> Items;

	public Action<int> OnSelectionChanged;

	private bool _isPanelActive;

	private bool _hasDrawnOnce;

	private DropDownListButton _mainButton;

	private RectTransform _rectTransform;

	private RectTransform _overlayRT;

	private RectTransform _scrollPanelRT;

	private RectTransform _scrollBarRT;

	private RectTransform _slidingAreaRT;

	private RectTransform _itemsPanelRT;

	private Canvas _canvas;

	private RectTransform _canvasRT;

	private ScrollRect _scrollRect;

	private List<DropDownListButton> _panelItems;

	private GameObject _itemTemplate;

	[SerializeField]
	private float _scrollBarWidth = 20f;

	private int _selectedIndex;

	[SerializeField]
	private int _itemsToDisplay;

	public DropDownListItem SelectedItem { get; private set; }

	public float ScrollBarWidth
	{
		get
		{
			return _scrollBarWidth;
		}
		set
		{
			_scrollBarWidth = value;
			RedrawPanel();
		}
	}

	public int ItemsToDisplay
	{
		get
		{
			return _itemsToDisplay;
		}
		set
		{
			_itemsToDisplay = value;
			RedrawPanel();
		}
	}

	public void Start()
	{
		Initialize();
	}

	private bool Initialize()
	{
		bool result = true;
		try
		{
			_rectTransform = GetComponent<RectTransform>();
			_mainButton = new DropDownListButton(_rectTransform.Find("MainButton").gameObject);
			_overlayRT = _rectTransform.Find("Overlay").GetComponent<RectTransform>();
			_overlayRT.gameObject.SetActive(value: false);
			_scrollPanelRT = _overlayRT.Find("ScrollPanel").GetComponent<RectTransform>();
			_scrollBarRT = _scrollPanelRT.Find("Scrollbar").GetComponent<RectTransform>();
			_slidingAreaRT = _scrollBarRT.Find("SlidingArea").GetComponent<RectTransform>();
			_itemsPanelRT = _scrollPanelRT.Find("Items").GetComponent<RectTransform>();
			_canvas = GetComponentInParent<Canvas>();
			_canvasRT = _canvas.GetComponent<RectTransform>();
			_scrollRect = _scrollPanelRT.GetComponent<ScrollRect>();
			_scrollRect.scrollSensitivity = _rectTransform.sizeDelta.y / 2f;
			_scrollRect.movementType = ScrollRect.MovementType.Clamped;
			_scrollRect.content = _itemsPanelRT;
			_itemTemplate = _rectTransform.Find("ItemTemplate").gameObject;
			_itemTemplate.SetActive(value: false);
		}
		catch (NullReferenceException exception)
		{
			Debug.LogException(exception);
			Debug.LogError("Something is setup incorrectly with the dropdownlist component causing a Null Refernece Exception");
			result = false;
		}
		_panelItems = new List<DropDownListButton>();
		RebuildPanel();
		RedrawPanel();
		return result;
	}

	private void RebuildPanel()
	{
		if (Items.Count == 0)
		{
			return;
		}
		int num = _panelItems.Count;
		while (_panelItems.Count < Items.Count)
		{
			GameObject gameObject = Object.Instantiate(_itemTemplate);
			gameObject.name = "Item " + num;
			gameObject.transform.SetParent(_itemsPanelRT, worldPositionStays: false);
			_panelItems.Add(new DropDownListButton(gameObject));
			num++;
		}
		for (int i = 0; i < _panelItems.Count; i++)
		{
			if (i < Items.Count)
			{
				DropDownListItem item = Items[i];
				_panelItems[i].txt.text = item.Caption;
				if (item.IsDisabled)
				{
					_panelItems[i].txt.color = disabledTextColor;
				}
				if (_panelItems[i].btnImg != null)
				{
					_panelItems[i].btnImg.sprite = null;
				}
				_panelItems[i].img.sprite = item.Image;
				_panelItems[i].img.color = ((item.Image == null) ? new Color(1f, 1f, 1f, 0f) : (item.IsDisabled ? new Color(1f, 1f, 1f, 0.5f) : Color.white));
				int ii = i;
				_panelItems[i].btn.onClick.RemoveAllListeners();
				_panelItems[i].btn.onClick.AddListener(delegate
				{
					OnItemClicked(ii);
					if (item.OnSelect != null)
					{
						item.OnSelect();
					}
				});
			}
			_panelItems[i].gameobject.SetActive(i < Items.Count);
		}
	}

	private void OnItemClicked(int indx)
	{
		Debug.Log("item " + indx + " clicked");
		if (indx != _selectedIndex && OnSelectionChanged != null)
		{
			OnSelectionChanged(indx);
		}
		_selectedIndex = indx;
		ToggleDropdownPanel(directClick: true);
		UpdateSelected();
	}

	private void UpdateSelected()
	{
		SelectedItem = ((_selectedIndex > -1 && _selectedIndex < Items.Count) ? Items[_selectedIndex] : null);
		if (SelectedItem != null)
		{
			if (SelectedItem.Image != null)
			{
				_mainButton.img.sprite = SelectedItem.Image;
				_mainButton.img.color = Color.white;
			}
			else
			{
				_mainButton.img.sprite = null;
			}
			_mainButton.txt.text = SelectedItem.Caption;
			for (int i = 0; i < _itemsPanelRT.childCount; i++)
			{
				_panelItems[i].btnImg.color = ((_selectedIndex == i) ? _mainButton.btn.colors.highlightedColor : new Color(0f, 0f, 0f, 0f));
			}
		}
	}

	private void RedrawPanel()
	{
		float num = ((Items.Count > ItemsToDisplay) ? _scrollBarWidth : 0f);
		if (!_hasDrawnOnce || _rectTransform.sizeDelta != _mainButton.rectTransform.sizeDelta)
		{
			_hasDrawnOnce = true;
			_mainButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rectTransform.sizeDelta.x);
			_mainButton.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rectTransform.sizeDelta.y);
			_mainButton.txt.rectTransform.offsetMax = new Vector2(4f, 0f);
			_scrollPanelRT.SetParent(base.transform, worldPositionStays: true);
			_scrollPanelRT.anchoredPosition = new Vector2(0f, 0f - _rectTransform.sizeDelta.y);
			_overlayRT.SetParent(_canvas.transform, worldPositionStays: false);
			_overlayRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _canvasRT.sizeDelta.x);
			_overlayRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _canvasRT.sizeDelta.y);
			_overlayRT.SetParent(base.transform, worldPositionStays: true);
			_scrollPanelRT.SetParent(_overlayRT, worldPositionStays: true);
		}
		if (Items.Count >= 1)
		{
			float num2 = _rectTransform.sizeDelta.y * (float)Mathf.Min(_itemsToDisplay, Items.Count);
			_scrollPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
			_scrollPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rectTransform.sizeDelta.x);
			_itemsPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _scrollPanelRT.sizeDelta.x - num - 5f);
			_itemsPanelRT.anchoredPosition = new Vector2(5f, 0f);
			_scrollBarRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
			_scrollBarRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
			_slidingAreaRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
			_slidingAreaRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2 - _scrollBarRT.sizeDelta.x);
		}
	}

	public void ToggleDropdownPanel(bool directClick)
	{
		_overlayRT.transform.localScale = new Vector3(1f, 1f, 1f);
		_scrollBarRT.transform.localScale = new Vector3(1f, 1f, 1f);
		_isPanelActive = !_isPanelActive;
		_overlayRT.gameObject.SetActive(_isPanelActive);
		if (_isPanelActive)
		{
			base.transform.SetAsLastSibling();
		}
	}
}
