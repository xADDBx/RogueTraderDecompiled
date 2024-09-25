using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUIControls;

public class DropDownListControl : ListBoxControl
{
	private const float CONTROL_BORDER = 30f;

	private const int MINIMUM_DISPLAYED_CELLS = 3;

	[SerializeField]
	private string _strStartingValue = "";

	[SerializeField]
	private string _strPlaceholder = "Select Item...";

	[SerializeField]
	private float _fHeight = 36f;

	private bool _blnDroppedDown;

	private Event _evntClick;

	private RectTransform _trnLB;

	private float _fOffset = -1f;

	public GameObject ParentContainer;

	public Text SelectedTextObject;

	public GameObject DropDownButton;

	public ListBoxControl DdlListBox;

	public string StartingValue
	{
		get
		{
			return _strStartingValue;
		}
		set
		{
			_strStartingValue = value.Trim();
		}
	}

	public string PlaceholderText
	{
		get
		{
			return _strPlaceholder;
		}
		set
		{
			_strPlaceholder = value.Trim();
			SelectedTextObject.text = _strPlaceholder;
		}
	}

	public float LineItemHeight
	{
		get
		{
			return _fHeight;
		}
		set
		{
			_fHeight = value;
			if (DdlListBox != null)
			{
				DdlListBox.Height = _fHeight;
			}
		}
	}

	public override List<ListBoxLineItem> Items
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.Items;
			}
			return new List<ListBoxLineItem>();
		}
	}

	public override List<int> SelectedIndexes
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.SelectedIndexes;
			}
			return new List<int>();
		}
	}

	public override List<string> SelectedValues
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.SelectedValues;
			}
			return new List<string>();
		}
	}

	public override string SelectedValue
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.SelectedValue;
			}
			return "";
		}
	}

	public override int SelectedValueInt
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.SelectedValueInt;
			}
			return -1;
		}
	}

	public override int SelectedIndex
	{
		get
		{
			if (DdlListBox != null)
			{
				return DdlListBox.SelectedIndex;
			}
			return -1;
		}
		set
		{
			if (DdlListBox != null)
			{
				DdlListBox.SelectedIndex = value;
			}
		}
	}

	public event OnDropDownSelectChanged OnSelectionChange;

	public override string SelectedArrayValue(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.SelectedArrayValue(intIndex);
		}
		return "";
	}

	public override int SelectedArrayValueInt(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.SelectedArrayValueInt(intIndex);
		}
		return -1;
	}

	private void Awake()
	{
		base.ListBoxMode = ListBoxModes.DropDownList;
		if (DdlListBox != null)
		{
			DdlListBox.gameObject.SetActive(value: true);
			DdlListBox.PartOfDDL = true;
			DdlListBox.Height = _fHeight;
			DdlListBox.InitStartItems(_startArray);
		}
		if (ParentContainer == null)
		{
			ParentContainer = base.transform.parent.gameObject;
		}
	}

	private void Start()
	{
		if (DdlListBox != null)
		{
			DdlListBox.gameObject.SetActive(value: true);
			DdlListBox.Show();
			DdlListBox.OnChange += OnChange;
		}
		DetermineDropDownPosition();
	}

	private void Update()
	{
		if (!_blnInitialized && DdlListBox.IsInitialized)
		{
			_blnInitialized = true;
			_blnDroppedDown = false;
			if (StartingValue == "")
			{
				DdlListBox.SelectedIndex = -1;
			}
			else
			{
				DdlListBox.SelectByValue(StartingValue);
			}
			if (DdlListBox.SelectedIndex >= 0)
			{
				SelectedTextObject.text = DdlListBox.GetTextByIndex(DdlListBox.SelectedIndex);
			}
			else
			{
				SelectedTextObject.text = _strPlaceholder;
			}
			DdlListBox.Hide();
		}
	}

	private void OnGUI()
	{
		Event current = Event.current;
		if (_blnInitialized && _blnDroppedDown && current.type == EventType.MouseUp)
		{
			_evntClick = current;
			StartCoroutine(CheckHide());
		}
	}

	private bool IsOverflowing(GameObject go)
	{
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] array = new Vector3[4];
		go.GetComponent<RectTransform>().GetWorldCorners(array);
		Vector3[] array2 = array;
		foreach (Vector3 point in array2)
		{
			if (!rect.Contains(point))
			{
				return true;
			}
		}
		return false;
	}

	private void DetermineDropDownPosition()
	{
		if (!(DdlListBox == null) && IsOverflowing(DdlListBox.gameObject))
		{
			RectTransform component = DdlListBox.GetComponent<RectTransform>();
			Vector3 position = component.position;
			Vector2 sizeDelta = component.sizeDelta;
			float y = GetComponent<RectTransform>().sizeDelta.y;
			float y2 = position.y + sizeDelta.y + y + DdlListBox.Spacing * 2f;
			position.y = y2;
			DdlListBox.GetComponent<RectTransform>().position = position;
		}
	}

	private IEnumerator CheckHide()
	{
		yield return new WaitForSeconds(0.1f);
		HideIfClickedOutside(DdlListBox.gameObject, _evntClick);
	}

	private bool HideIfClickedOutside(GameObject panel, Event e)
	{
		if (panel != null && panel.activeSelf && _blnDroppedDown)
		{
			if (_trnLB == null)
			{
				_trnLB = DdlListBox.GetComponent<RectTransform>();
			}
			if (_fOffset < 0f)
			{
				_fOffset = SelectedTextObject.GetComponent<RectTransform>().rect.size.y;
			}
			float y = (float)Screen.height - (_trnLB.position.y + _fOffset + 30f);
			Vector2 point = new Vector2(e.mousePosition.x, e.mousePosition.y);
			Vector2 position = new Vector2(_trnLB.position.x - 30f, y);
			Vector2 size = new Vector2(_trnLB.rect.size.x + 60f, _trnLB.rect.size.y + _fOffset + 60f);
			if (!new Rect(position, size).Contains(point))
			{
				_evntClick = null;
				DdlListBox.Hide();
				_blnDroppedDown = false;
				return true;
			}
		}
		_evntClick = null;
		return false;
	}

	public override void Clear()
	{
		if (DdlListBox != null)
		{
			DdlListBox.Clear();
		}
		if (SelectedTextObject != null)
		{
			SelectedTextObject.text = PlaceholderText;
		}
	}

	public override void AddItem(string strValue, string strText, string strIcon = "", string strSub = "")
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, strSub);
		}
	}

	public override void AddItem(string strValue, string strText, Sprite sprIcon, string strSub = "")
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, strSub);
		}
	}

	public override void AddItem(string strValue, string strText, string strIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, intSub.ToString());
		}
	}

	public override void AddItem(string strValue, string strText, string strIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, fSub.ToString());
		}
	}

	public override void AddItem(string strValue, string strText, Sprite sprIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, intSub.ToString());
		}
	}

	public override void AddItem(string strValue, string strText, Sprite sprIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, fSub.ToString());
		}
	}

	public override void AddItem(string[] strValue, string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText);
		}
	}

	public override void AddItem(string[] strValue, string strText, string strIcon)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon);
		}
	}

	public override void AddItem(string[] strValue, string strText, string strIcon, string strSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, strSub);
		}
	}

	public override void AddItem(string[] strValue, string strText, string strIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, intSub);
		}
	}

	public override void AddItem(string[] strValue, string strText, string strIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, strIcon, fSub);
		}
	}

	public override void AddItem(string[] strValue, string strText, Sprite sprIcon)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon);
		}
	}

	public override void AddItem(string[] strValue, string strText, Sprite sprIcon, string strSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, strSub);
		}
	}

	public override void AddItem(string[] strValue, string strText, Sprite sprIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, intSub);
		}
	}

	public override void AddItem(string[] strValue, string strText, Sprite sprIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(strValue, strText, sprIcon, fSub);
		}
	}

	public override void AddItem(int intValue, string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText);
		}
	}

	public override void AddItem(int intValue, string strText, string strIcon)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, strIcon);
		}
	}

	public override void AddItem(int intValue, string strText, string strIcon, string strSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, strIcon, strSub);
		}
	}

	public override void AddItem(int intValue, string strText, string strIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, strIcon, intSub);
		}
	}

	public override void AddItem(int intValue, string strText, string strIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, strIcon, fSub);
		}
	}

	public override void AddItem(int intValue, string strText, Sprite sprIcon)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, sprIcon);
		}
	}

	public override void AddItem(int intValue, string strText, Sprite sprIcon, string strSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, sprIcon, strSub);
		}
	}

	public override void AddItem(int intValue, string strText, Sprite sprIcon, int intSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, sprIcon, intSub);
		}
	}

	public override void AddItem(int intValue, string strText, Sprite sprIcon, float fSub)
	{
		if (DdlListBox != null)
		{
			DdlListBox.AddItem(intValue, strText, sprIcon, fSub);
		}
	}

	public override void RemoveItemByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			DdlListBox.RemoveItemByIndex(intIndex);
		}
	}

	public override void RemoveItemByValue(string strValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.RemoveItemByValue(strValue);
		}
	}

	public override void RemoveItemByText(string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.RemoveItemByText(strText);
		}
	}

	public override void Sort()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SortByText();
		}
	}

	public override void SortByText()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SortByText();
		}
	}

	public override void SortByValue()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SortByValue();
		}
	}

	public override void SortBySubText()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SortBySubText();
		}
	}

	public override void SetToTop()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetToTop();
		}
	}

	public override void SetToBottom()
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetToBottom();
		}
	}

	public override bool HasItemWithValue(string strValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.HasItemWithValue(strValue);
		}
		return false;
	}

	public override bool HasItemWithValue(int intValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.HasItemWithValue(intValue);
		}
		return false;
	}

	public override bool HasItemWithValue(float fValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.HasItemWithValue(fValue);
		}
		return false;
	}

	public override void EnableByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			DdlListBox.EnableByIndex(intIndex);
		}
	}

	public override void EnableByValue(string strValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.EnableByValue(strValue);
		}
	}

	public override void EnableByValue(int intValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.EnableByValue(intValue);
		}
	}

	public override void EnableByText(string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.EnableByText(strText);
		}
	}

	public override void DisableByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			DdlListBox.DisableByIndex(intIndex);
		}
	}

	public override void DisableByValue(string strValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.DisableByValue(strValue);
		}
	}

	public override void DisableByValue(int intValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.DisableByValue(intValue);
		}
	}

	public override void DisableByText(string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.DisableByText(strText);
		}
	}

	public override void SetItemTextByIndex(int intIndex, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetItemTextByIndex(intIndex, strNewText);
		}
	}

	public override void SetItemTextByValue(string strValue, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetItemTextByValue(strValue, strNewText);
		}
	}

	public override void SetItemTextByValue(int intValue, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetItemTextByValue(intValue.ToString(), strNewText);
		}
	}

	public override void SetItemSubTextByIndex(int intIndex, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.Items[intIndex].SubText = strNewText;
		}
	}

	public override void SetItemSubTextByValue(string strValue, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetItemSubTextByValue(strValue, strNewText);
		}
	}

	public override void SetItemSubTextByValue(int intValue, string strNewText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SetItemSubTextByValue(intValue.ToString(), strNewText);
		}
	}

	public override bool MoveItemUp(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.MoveItemUp(intIndex);
		}
		return false;
	}

	public override bool MoveItemDown(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.MoveItemDown(intIndex);
		}
		return false;
	}

	public override string GetValueByText(string strText)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetValueByText(strText);
		}
		return "";
	}

	public override string GetValueByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetValueByIndex(intIndex);
		}
		return "";
	}

	public override int GetIntValueByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetIntValueByIndex(intIndex);
		}
		return -1;
	}

	public override string GetTextByValue(string strvalue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetTextByValue(strvalue);
		}
		return "";
	}

	public override string GetTextByValue(int intValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetTextByValue(intValue);
		}
		return "";
	}

	public override string GetTextByValue(float fValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetTextByValue(fValue);
		}
		return "";
	}

	public override string GetTextByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetTextByIndex(intIndex);
		}
		return "";
	}

	public override string GetSubTextByValue(string strvalue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetSubTextByValue(strvalue);
		}
		return "";
	}

	public override string GetSubTextByValue(int intValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetSubTextByValue(intValue);
		}
		return "";
	}

	public override string GetSubTextByValue(float fValue)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetSubTextByValue(fValue);
		}
		return "";
	}

	public override string GetSubTextByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.GetSubTextByIndex(intIndex);
		}
		return "";
	}

	public override void SelectByIndex(int intIndex, bool blnShifted = false, bool blnCtrled = false)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SelectByIndex(intIndex, blnShifted, blnCtrled);
			if (DdlListBox.SelectedIndex < 0)
			{
				SelectedTextObject.text = _strPlaceholder;
			}
			else
			{
				SelectedTextObject.text = DdlListBox.SelectedText;
			}
		}
	}

	public override void SelectByValue(string strValue)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SelectByValue(strValue);
			if (DdlListBox.SelectedIndex < 0)
			{
				SelectedTextObject.text = _strPlaceholder;
			}
			else
			{
				SelectedTextObject.text = DdlListBox.SelectedText;
			}
		}
	}

	public override void SelectByText(string strText)
	{
		if (DdlListBox != null)
		{
			DdlListBox.SelectByText(strText);
			if (DdlListBox.SelectedIndex < 0)
			{
				SelectedTextObject.text = _strPlaceholder;
			}
			else
			{
				SelectedTextObject.text = DdlListBox.SelectedText;
			}
		}
	}

	public override void Unselect()
	{
		if (DdlListBox != null)
		{
			DdlListBox.Unselect();
		}
		SelectedTextObject.text = _strPlaceholder;
	}

	public override bool IsSelectedByIndex(int intIndex)
	{
		if (DdlListBox != null)
		{
			return DdlListBox.IsSelectedByIndex(intIndex);
		}
		return false;
	}

	public override void UpdateListBoxContainerSize()
	{
		if (DdlListBox != null)
		{
			DdlListBox.UpdateListBoxContainerSize();
		}
	}

	public override void Hide()
	{
		_blnDroppedDown = false;
		if (DdlListBox != null)
		{
			DdlListBox.Hide();
		}
		GetComponent<Image>().enabled = false;
		if (SelectedTextObject != null)
		{
			SelectedTextObject.gameObject.SetActive(value: false);
		}
		if (DropDownButton != null)
		{
			DropDownButton.SetActive(value: false);
		}
	}

	public override void Show()
	{
		_blnDroppedDown = false;
		GetComponent<Image>().enabled = true;
		if (SelectedTextObject != null)
		{
			SelectedTextObject.gameObject.SetActive(value: true);
		}
		if (DropDownButton != null)
		{
			DropDownButton.SetActive(value: true);
		}
	}

	private void DoShow()
	{
		DdlListBox.transform.parent.SetSiblingIndex(base.transform.parent.childCount - 1);
		DdlListBox.Show();
	}

	public void OnDownButtonClick()
	{
		if (DdlListBox != null)
		{
			_blnDroppedDown = !_blnDroppedDown;
			if (_blnDroppedDown)
			{
				DoShow();
			}
			else
			{
				DdlListBox.Hide();
			}
		}
	}

	public new void OnChange(GameObject go, int intIndex)
	{
		if (go != DdlListBox.gameObject)
		{
			return;
		}
		if (SelectedTextObject != null)
		{
			if (intIndex < 0)
			{
				SelectedTextObject.text = PlaceholderText;
			}
			else
			{
				SelectedTextObject.text = DdlListBox.GetTextByIndex(intIndex);
			}
		}
		_blnDroppedDown = false;
		DdlListBox.Hide();
		if (SelectedIndex >= 0 && this.OnSelectionChange != null)
		{
			this.OnSelectionChange(base.gameObject, SelectedIndex);
		}
	}
}
