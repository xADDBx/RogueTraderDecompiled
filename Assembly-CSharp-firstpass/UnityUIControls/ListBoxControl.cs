using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUIControls;

[Serializable]
public class ListBoxControl : MonoBehaviour
{
	[Serializable]
	public class StartingListItem
	{
		public string Value = "";

		public string Text = "";

		public string SubText = "";

		public Sprite Icon;

		public int Index = -1;

		public StartingListItem(string strValue, string strText, Sprite imgSprite = null, string strSub = "")
		{
			Value = strValue;
			Text = strText;
			SubText = strSub;
			Icon = imgSprite;
		}
	}

	public enum ListBoxModes
	{
		ListBox,
		DropDownList
	}

	[SerializeField]
	protected List<StartingListItem> _startArray = new List<StartingListItem>();

	[SerializeField]
	private string _strTitle = "";

	[SerializeField]
	private bool _blnBestFit;

	[SerializeField]
	private bool _blnAllowDblClick;

	[SerializeField]
	private bool _blnPartOfDDL;

	private ListBoxModes _lbMode;

	private List<ListBoxLineItem> _items = new List<ListBoxLineItem>();

	private RectTransform _rtContainer;

	private RectTransform _rtScrollRect;

	private int _intSelectedItem = -1;

	private List<int> _intSelectedList = new List<int>();

	protected bool _blnInitialized;

	protected bool _blnInitializing;

	public GameObject ScrollBarObject;

	public GameObject ScrollRectObject;

	public GameObject ScrollContainerObject;

	public Text ListBoxTitle;

	public GameObject ListBoxLineItemPrefabObject;

	[SerializeField]
	public Color ItemNormalColor;

	[SerializeField]
	public Color ItemHighlightColor;

	[SerializeField]
	public Color ItemSelectedColor;

	[SerializeField]
	public Color ItemDisabledColor;

	[SerializeField]
	public bool CanMultiSelect;

	[SerializeField]
	public float Height = 36f;

	[SerializeField]
	public float Spacing = 4f;

	[SerializeField]
	public char SeparatorChar = '|';

	private RectTransform ContainerRect
	{
		get
		{
			if (_rtContainer == null && ScrollContainerObject != null)
			{
				_rtContainer = ScrollContainerObject.GetComponent<RectTransform>();
			}
			return _rtContainer;
		}
	}

	private RectTransform ScrollRect
	{
		get
		{
			if (_rtScrollRect == null && ScrollRectObject != null)
			{
				_rtScrollRect = ScrollRectObject.GetComponent<RectTransform>();
			}
			return _rtScrollRect;
		}
	}

	public ListBoxModes ListBoxMode
	{
		get
		{
			return _lbMode;
		}
		set
		{
			_lbMode = value;
		}
	}

	public string Title
	{
		get
		{
			return _strTitle;
		}
		set
		{
			_strTitle = value.Trim();
			if (ListBoxMode == ListBoxModes.ListBox && ListBoxTitle != null)
			{
				ListBoxTitle.text = _strTitle;
			}
		}
	}

	public bool TitleBestFit
	{
		get
		{
			return _blnBestFit;
		}
		set
		{
			_blnBestFit = value;
			if (ListBoxMode == ListBoxModes.ListBox && ListBoxTitle != null)
			{
				ListBoxTitle.resizeTextForBestFit = _blnBestFit;
			}
		}
	}

	public bool AllowDoubleClick
	{
		get
		{
			if (_blnAllowDblClick && !_blnPartOfDDL)
			{
				return ListBoxMode == ListBoxModes.ListBox;
			}
			return false;
		}
		set
		{
			_blnAllowDblClick = value;
		}
	}

	public bool PartOfDDL
	{
		get
		{
			return _blnPartOfDDL;
		}
		set
		{
			_blnPartOfDDL = value;
		}
	}

	public List<StartingListItem> StartArray => _startArray;

	public virtual List<ListBoxLineItem> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<ListBoxLineItem>();
			}
			return _items;
		}
	}

	public virtual List<int> SelectedIndexes
	{
		get
		{
			if (_intSelectedList == null)
			{
				_intSelectedList = new List<int>();
			}
			return _intSelectedList;
		}
	}

	public virtual List<string> SelectedValues
	{
		get
		{
			if (_intSelectedItem < 0 || _intSelectedList == null || _intSelectedList.Count < 0)
			{
				return null;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < _intSelectedList.Count; i++)
			{
				list.Add(Items[_intSelectedList[i]].Value);
			}
			return list;
		}
	}

	public virtual string SelectedValuesString
	{
		get
		{
			List<string> selectedValues = SelectedValues;
			if (selectedValues == null || selectedValues.Count < 1)
			{
				return "";
			}
			string text = "";
			for (int i = 0; i < selectedValues.Count; i++)
			{
				if (selectedValues[i].Trim() != "")
				{
					text = text + SeparatorChar + selectedValues[i];
				}
			}
			if (text.Length > 1)
			{
				text = text.Substring(1);
			}
			return text;
		}
	}

	public virtual string SelectedValue
	{
		get
		{
			if (_intSelectedItem < 0 || _intSelectedList == null || _intSelectedList.Count < 0)
			{
				return null;
			}
			return Items[_intSelectedList[0]].Value;
		}
	}

	public virtual int SelectedValueInt
	{
		get
		{
			if (_intSelectedItem < 0 || _intSelectedList == null || _intSelectedList.Count < 0)
			{
				return -1;
			}
			return Util.ConvertToInt(Items[_intSelectedList[0]].Value);
		}
	}

	public virtual int SelectedIndex
	{
		get
		{
			return _intSelectedItem;
		}
		set
		{
			_intSelectedItem = value;
		}
	}

	public virtual string SelectedText
	{
		get
		{
			if (_intSelectedItem < 0 || _intSelectedList == null || _intSelectedList.Count < 0)
			{
				return null;
			}
			return Items[_intSelectedList[0]].Text;
		}
	}

	public bool IsInitialized => _blnInitialized;

	public virtual bool IsShown
	{
		get
		{
			if (ListBoxMode == ListBoxModes.ListBox)
			{
				if (GetComponent<Image>().enabled && ScrollBarObject.activeSelf)
				{
					return ScrollRectObject.activeSelf;
				}
				return false;
			}
			return false;
		}
	}

	public event OnListBoxSelectChanged OnChange;

	public event OnListBoxDoubleClick OnDoubleClick;

	public virtual string SelectedArrayValue(int intIndex)
	{
		if (intIndex > Items[_intSelectedList[0]].Value.Split(SeparatorChar).Length - 1)
		{
			return "";
		}
		return Items[_intSelectedList[0]].Value.Split(SeparatorChar)[intIndex];
	}

	public virtual int SelectedArrayValueInt(int intIndex)
	{
		return Util.ConvertToInt(SelectedArrayValue(intIndex));
	}

	private void Awake()
	{
		_intSelectedItem = -1;
		_items = new List<ListBoxLineItem>();
		_intSelectedList = new List<int>();
		if (ListBoxMode != ListBoxModes.DropDownList && ScrollContainerObject != null && ScrollContainerObject.transform.childCount > 0)
		{
			for (int num = ScrollContainerObject.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(ScrollContainerObject.transform.GetChild(num).gameObject);
			}
		}
	}

	private void Start()
	{
		if (_blnInitialized || _blnInitializing || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		_blnInitializing = true;
		if (ContainerRect != null)
		{
			ContainerRect.sizeDelta = new Vector2(ScrollRect.rect.width, ScrollRect.rect.height);
		}
		if (ScrollRectObject != null)
		{
			ScrollRectObject.GetComponent<ScrollRect>().scrollSensitivity = Height - Spacing;
		}
		if (ScrollBarObject != null)
		{
			ScrollBarObject.GetComponent<Scrollbar>().numberOfSteps = 1;
		}
		if (ListBoxMode == ListBoxModes.DropDownList)
		{
			return;
		}
		if (ListBoxTitle != null)
		{
			Title = _strTitle;
		}
		if (ListBoxLineItemPrefabObject == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
		}
		else if (ListBoxLineItemPrefabObject.GetComponent<ListBoxLineItem>() == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
		}
		if (StartArray.Count > 0)
		{
			for (int i = 0; i < StartArray.Count; i++)
			{
				AddItem(StartArray[i].Value, StartArray[i].Text, StartArray[i].Icon);
			}
		}
		_blnInitializing = false;
		_blnInitialized = true;
	}

	private void OnEnable()
	{
		if (ListBoxMode == ListBoxModes.ListBox)
		{
			UpdateListBoxContainerSize();
		}
	}

	private void ResizeContainer()
	{
		if (!Application.isPlaying || ListBoxMode == ListBoxModes.DropDownList)
		{
			return;
		}
		float scroll = 1f;
		if (ScrollBarObject != null)
		{
			scroll = ScrollBarObject.GetComponent<Scrollbar>().value;
		}
		Vector2 sizeDelta = ContainerRect.sizeDelta;
		sizeDelta.y = (Height + Spacing) * (float)Items.Count + Spacing;
		ContainerRect.sizeDelta = sizeDelta;
		try
		{
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(SetScroll(scroll));
			}
		}
		catch
		{
		}
	}

	private void SelectByRange(int intEnd)
	{
		int num = (int)Mathf.Sign(intEnd - _intSelectedItem);
		int num2 = _intSelectedItem;
		while (intEnd >= 0 && num2 >= 0 && num2 < Items.Count && ((num > 0 && num2 <= intEnd) || (num < 0 && num2 >= intEnd)))
		{
			if (Items[num2].Enabled && Items[num2].Shown)
			{
				Items[num2].Select();
				_intSelectedList.Add(num2);
			}
			num2 += num;
		}
	}

	private void UnSelectItem(int intIndex)
	{
		if (ListBoxMode == ListBoxModes.DropDownList)
		{
			return;
		}
		if (intIndex >= 0 && intIndex == _intSelectedItem && Items[intIndex] != null)
		{
			Items[_intSelectedItem].UnSelect();
			_intSelectedItem = -1;
		}
		if (_intSelectedList.Count > 0)
		{
			int num = _intSelectedList.FindIndex((int x) => x == intIndex);
			if (num >= 0)
			{
				Items[_intSelectedList[num]].UnSelect();
				_intSelectedList.RemoveAt(num);
			}
		}
	}

	private void UnSelectByRange(int intEnd)
	{
		if (ListBoxMode != ListBoxModes.DropDownList)
		{
			int num = (int)Mathf.Sign(intEnd - _intSelectedItem);
			int num2 = _intSelectedItem;
			while (intEnd >= 0 && num2 >= 0 && num2 < Items.Count && ((num > 0 && num2 <= intEnd) || (num < 0 && num2 >= intEnd)))
			{
				Items[_intSelectedList[num2]].UnSelect();
				_intSelectedList.RemoveAt(num2);
				num2 += num;
			}
		}
	}

	private void UnSelectAllItems()
	{
		if (ListBoxMode == ListBoxModes.DropDownList)
		{
			return;
		}
		if (_intSelectedItem >= 0 && Items[_intSelectedItem] != null)
		{
			Items[_intSelectedItem].UnSelect();
		}
		if (_intSelectedList.Count > 0)
		{
			for (int num = _intSelectedList.Count - 1; num >= 0; num--)
			{
				Items[_intSelectedList[num]].UnSelect();
				_intSelectedList.RemoveAt(num);
			}
		}
	}

	private IEnumerator SetScroll(float fValue)
	{
		yield return new WaitForSeconds(0.01f);
		if (base.gameObject.activeInHierarchy && ScrollBarObject != null && ScrollBarObject.activeSelf && ListBoxMode == ListBoxModes.ListBox)
		{
			yield return new WaitForSeconds(0.12f);
			ScrollBarObject.GetComponent<Scrollbar>().value = 0f;
			yield return new WaitForSeconds(0.001f);
			ScrollBarObject.GetComponent<Scrollbar>().value = 1f;
			yield return new WaitForSeconds(0.001f);
			ScrollBarObject.GetComponent<Scrollbar>().value = fValue;
		}
	}

	public void ClearStartItems()
	{
		_startArray = new List<StartingListItem>();
	}

	public void InitStartItems(List<StartingListItem> sli)
	{
		ClearStartItems();
		foreach (StartingListItem item in sli)
		{
			_startArray.Add(item);
		}
	}

	public virtual void AddStartItem(string strValue, string strText, Sprite sprIcon = null, string strSub = "")
	{
		int num = StartArray.FindIndex((StartingListItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant() || x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		if (num >= 0)
		{
			StartArray[num].Value = strValue;
			StartArray[num].Text = strText;
			StartArray[num].Icon = sprIcon;
			StartArray[num].SubText = strSub;
			StartArray[num].Index = num;
		}
		else
		{
			StartArray.Add(new StartingListItem(strValue, strText, sprIcon, strSub));
			StartArray[StartArray.Count - 1].Index = StartArray.Count - 1;
		}
	}

	public virtual void RemoveStartItemByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= StartArray.Count)
		{
			return;
		}
		for (int num = StartArray.Count - 1; num >= intIndex; num--)
		{
			if (num > intIndex)
			{
				StartArray[num].Index = num - 1;
			}
			else
			{
				StartArray.RemoveAt(num);
			}
		}
	}

	public virtual void RemoveStartItemByValue(string strValue)
	{
		int num = StartArray.FindIndex((StartingListItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant());
		if (num >= 0)
		{
			RemoveStartItemByIndex(num);
		}
	}

	public virtual void RemoveStartItemByText(string strText)
	{
		int num = StartArray.FindIndex((StartingListItem x) => x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		if (num >= 0)
		{
			RemoveStartItemByIndex(num);
		}
	}

	public virtual void SortStartByValue()
	{
		StartArray.Sort((StartingListItem p1, StartingListItem p2) => p1.Text.CompareTo(p2.Value));
		for (int i = 0; i < StartArray.Count; i++)
		{
			StartArray[i].Index = i;
		}
	}

	public virtual void SortStartByText()
	{
		StartArray.Sort((StartingListItem p1, StartingListItem p2) => p1.Text.CompareTo(p2.Text));
		for (int i = 0; i < StartArray.Count; i++)
		{
			StartArray[i].Index = i;
		}
	}

	public virtual void SortStartBySub()
	{
		StartArray.Sort((StartingListItem p1, StartingListItem p2) => p1.SubText.CompareTo(p2.Text));
		for (int i = 0; i < StartArray.Count; i++)
		{
			StartArray[i].Index = i;
		}
	}

	public virtual void Clear()
	{
		_intSelectedItem = -1;
		_items = new List<ListBoxLineItem>();
		_intSelectedList = new List<int>();
		if (ScrollContainerObject.transform.childCount > 0)
		{
			for (int num = ScrollContainerObject.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(ScrollContainerObject.transform.GetChild(num).gameObject);
			}
		}
	}

	public virtual void AddItem(string strValue, string strText, string strIcon = "", string strSub = "")
	{
		if (ListBoxLineItemPrefabObject == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
			return;
		}
		if (ListBoxLineItemPrefabObject.GetComponent<ListBoxLineItem>() == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
			return;
		}
		Sprite icon = null;
		if (strIcon != "")
		{
			icon = Resources.Load<Sprite>(strIcon);
		}
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant() || x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		if (num >= 0)
		{
			Items[num].Value = strValue;
			Items[num].Text = strText;
			Items[num].SubText = strSub;
			Items[num].SetIcon(icon);
			return;
		}
		num = Items.Count;
		GameObject gameObject = UnityEngine.Object.Instantiate(ListBoxLineItemPrefabObject);
		gameObject.transform.SetParent(ScrollContainerObject.transform);
		gameObject.GetComponent<ListBoxLineItem>().ListBoxControlObject = base.gameObject;
		gameObject.GetComponent<ListBoxLineItem>().Index = num;
		gameObject.GetComponent<ListBoxLineItem>().Spacing = Spacing;
		gameObject.GetComponent<ListBoxLineItem>().Width = ContainerRect.sizeDelta.x - Spacing * 2f;
		if (Height > 0f)
		{
			gameObject.GetComponent<ListBoxLineItem>().Height = Height;
		}
		else
		{
			Height = gameObject.GetComponent<ListBoxLineItem>().Height;
		}
		gameObject.GetComponent<ListBoxLineItem>().ItemNormalColor = ItemNormalColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemHighlightColor = ItemHighlightColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemSelectedColor = ItemSelectedColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemDisabledColor = ItemDisabledColor;
		gameObject.GetComponent<ListBoxLineItem>().Value = strValue;
		gameObject.GetComponent<ListBoxLineItem>().Text = strText;
		gameObject.GetComponent<ListBoxLineItem>().SubText = strSub;
		gameObject.GetComponent<ListBoxLineItem>().SetIcon(icon);
		gameObject.GetComponent<ListBoxLineItem>().AutoSize();
		Items.Add(gameObject.GetComponent<ListBoxLineItem>());
		ResizeContainer();
	}

	public virtual void AddItem(string strValue, string strText, Sprite sprIcon, string strSub = "")
	{
		if (ListBoxLineItemPrefabObject == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
			return;
		}
		if (ListBoxLineItemPrefabObject.GetComponent<ListBoxLineItem>() == null)
		{
			Debug.LogError(base.gameObject.name + " is Missing the Line Item Prefab. Please add the Prefab.");
			return;
		}
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant() || x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		if (num >= 0)
		{
			Items[num].Value = strValue;
			Items[num].Text = strText;
			Items[num].SubText = strSub;
			Items[num].SetIcon(sprIcon);
			return;
		}
		num = Items.Count;
		GameObject gameObject = UnityEngine.Object.Instantiate(ListBoxLineItemPrefabObject);
		gameObject.transform.SetParent(ScrollContainerObject.transform);
		gameObject.GetComponent<ListBoxLineItem>().ListBoxControlObject = base.gameObject;
		gameObject.GetComponent<ListBoxLineItem>().Index = num;
		gameObject.GetComponent<ListBoxLineItem>().Spacing = Spacing;
		gameObject.GetComponent<ListBoxLineItem>().Width = ContainerRect.sizeDelta.x - Spacing * 2f;
		if (Height > 0f)
		{
			gameObject.GetComponent<ListBoxLineItem>().Height = Height;
		}
		else
		{
			Height = gameObject.GetComponent<ListBoxLineItem>().Height;
		}
		gameObject.GetComponent<ListBoxLineItem>().ItemNormalColor = ItemNormalColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemHighlightColor = ItemHighlightColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemSelectedColor = ItemSelectedColor;
		gameObject.GetComponent<ListBoxLineItem>().ItemDisabledColor = ItemDisabledColor;
		gameObject.GetComponent<ListBoxLineItem>().Value = strValue;
		gameObject.GetComponent<ListBoxLineItem>().Text = strText;
		gameObject.GetComponent<ListBoxLineItem>().SubText = strSub;
		gameObject.GetComponent<ListBoxLineItem>().SetIcon(sprIcon);
		gameObject.GetComponent<ListBoxLineItem>().AutoSize();
		Items.Add(gameObject.GetComponent<ListBoxLineItem>());
		ResizeContainer();
	}

	public virtual void AddItem(string strValue, string strText, string strIcon, int intSub)
	{
		AddItem(strValue, strText, strIcon, intSub.ToString());
	}

	public virtual void AddItem(string strValue, string strText, string strIcon, float fSub)
	{
		AddItem(strValue, strText, strIcon, fSub.ToString());
	}

	public virtual void AddItem(string strValue, string strText, Sprite sprIcon, int intSub)
	{
		AddItem(strValue, strText, sprIcon, intSub.ToString());
	}

	public virtual void AddItem(string strValue, string strText, Sprite sprIcon, float fSub)
	{
		AddItem(strValue, strText, sprIcon, fSub.ToString());
	}

	public virtual void AddItem(string[] strValue, string strText)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText);
		}
	}

	public virtual void AddItem(string[] strValue, string strText, string strIcon)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, strIcon);
		}
	}

	public virtual void AddItem(string[] strValue, string strText, string strIcon, string strSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, strIcon, strSub);
		}
	}

	public virtual void AddItem(string[] strValue, string strText, string strIcon, int intSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, strIcon, intSub.ToString());
		}
	}

	public virtual void AddItem(string[] strValue, string strText, string strIcon, float fSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, strIcon, fSub.ToString());
		}
	}

	public virtual void AddItem(string[] strValue, string strText, Sprite sprIcon)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, sprIcon);
		}
	}

	public virtual void AddItem(string[] strValue, string strText, Sprite sprIcon, string strSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, sprIcon, strSub);
		}
	}

	public virtual void AddItem(string[] strValue, string strText, Sprite sprIcon, int intSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, sprIcon, intSub.ToString());
		}
	}

	public virtual void AddItem(string[] strValue, string strText, Sprite sprIcon, float fSub)
	{
		if (strValue != null && strValue.Length != 0 && strText.Trim() != "")
		{
			string text = "";
			for (int i = 0; i < strValue.Length; i++)
			{
				text = text + SeparatorChar + strValue[i];
			}
			text = text.Substring(1);
			AddItem(text, strText, sprIcon, fSub.ToString());
		}
	}

	public virtual void AddItem(int intValue, string strText)
	{
		AddItem(intValue.ToString(), strText);
	}

	public virtual void AddItem(int intValue, string strText, string strIcon)
	{
		AddItem(intValue.ToString(), strText, strIcon);
	}

	public virtual void AddItem(int intValue, string strText, string strIcon, string strSub)
	{
		AddItem(intValue.ToString(), strText, strIcon, strSub);
	}

	public virtual void AddItem(int intValue, string strText, string strIcon, int intSub)
	{
		AddItem(intValue.ToString(), strText, strIcon, intSub.ToString());
	}

	public virtual void AddItem(int intValue, string strText, string strIcon, float fSub)
	{
		AddItem(intValue.ToString(), strText, strIcon, fSub.ToString());
	}

	public virtual void AddItem(int intValue, string strText, Sprite sprIcon)
	{
		AddItem(intValue.ToString(), strText, sprIcon);
	}

	public virtual void AddItem(int intValue, string strText, Sprite sprIcon, string strSub)
	{
		AddItem(intValue.ToString(), strText, sprIcon, strSub);
	}

	public virtual void AddItem(int intValue, string strText, Sprite sprIcon, int intSub)
	{
		AddItem(intValue.ToString(), strText, sprIcon, intSub.ToString());
	}

	public virtual void AddItem(int intValue, string strText, Sprite sprIcon, float fSub)
	{
		AddItem(intValue.ToString(), strText, sprIcon, fSub.ToString());
	}

	public virtual void RemoveItemByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= Items.Count)
		{
			return;
		}
		for (int num = Items.Count - 1; num >= intIndex; num--)
		{
			if (num > intIndex)
			{
				Items[num].Index = num - 1;
				Items[num].AutoSize();
			}
			else
			{
				Items[num].Destroy();
				Items.RemoveAt(num);
			}
		}
		_intSelectedItem = -1;
		_intSelectedList = new List<int>();
		ResizeContainer();
	}

	public virtual void RemoveItemByValue(string strValue)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant());
		if (num >= 0)
		{
			RemoveItemByIndex(num);
		}
	}

	public virtual void RemoveItemByText(string strText)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		if (num >= 0)
		{
			RemoveItemByIndex(num);
		}
	}

	public virtual void Sort()
	{
		SortByText();
	}

	public virtual void SortByText()
	{
		Items.Sort((ListBoxLineItem p1, ListBoxLineItem p2) => p1.Text.CompareTo(p2.Text));
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i].Index = i;
			Items[i].AutoSize();
		}
	}

	public virtual void SortByValue()
	{
		Items.Sort((ListBoxLineItem p1, ListBoxLineItem p2) => p1.Text.CompareTo(p2.Value));
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i].Index = i;
			Items[i].AutoSize();
		}
	}

	public virtual void SortBySubText()
	{
		Items.Sort((ListBoxLineItem p1, ListBoxLineItem p2) => p1.SubText.CompareTo(p2.Value));
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i].Index = i;
			Items[i].AutoSize();
		}
	}

	public virtual void SetToTop()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(SetScroll(1f));
		}
	}

	public virtual void SetToBottom()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(SetScroll(0f));
		}
	}

	public virtual bool HasItemWithValue(string strValue)
	{
		return Items.FindIndex((ListBoxLineItem x) => x.Value.Trim().ToLowerInvariant() == strValue.Trim().ToLowerInvariant()) >= 0;
	}

	public virtual bool HasItemWithValue(int intValue)
	{
		return HasItemWithValue(intValue.ToString());
	}

	public virtual bool HasItemWithValue(float fValue)
	{
		return HasItemWithValue(fValue.ToString());
	}

	public virtual void EnableByIndex(int intIndex)
	{
		if (intIndex >= 0 && intIndex < Items.Count)
		{
			Items[intIndex].Enabled = true;
		}
	}

	public virtual void EnableByValue(string strValue)
	{
		EnableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant()));
	}

	public virtual void EnableByValue(int intValue)
	{
		EnableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == intValue.ToString().ToLowerInvariant()));
	}

	public virtual void EnableByText(string strText)
	{
		EnableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Text.ToLowerInvariant() == strText.ToLowerInvariant()));
	}

	public virtual void DisableByIndex(int intIndex)
	{
		if (intIndex >= 0 && intIndex < Items.Count)
		{
			Items[intIndex].Enabled = false;
		}
	}

	public virtual void DisableByValue(string strValue)
	{
		DisableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant()));
	}

	public virtual void DisableByValue(int intValue)
	{
		DisableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == intValue.ToString().ToLowerInvariant()));
	}

	public virtual void DisableByText(string strText)
	{
		DisableByIndex(Items.FindIndex((ListBoxLineItem x) => x.Text.ToLowerInvariant() == strText.ToLowerInvariant()));
	}

	public virtual void SetItemTextByIndex(int intIndex, string strNewText)
	{
		Items[intIndex].Text = strNewText;
	}

	public virtual void SetItemTextByValue(string strValue, string strNewText)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value == strValue);
		if (num >= 0)
		{
			SetItemTextByIndex(num, strNewText);
		}
	}

	public virtual void SetItemTextByValue(int intValue, string strNewText)
	{
		SetItemTextByValue(intValue.ToString(), strNewText);
	}

	public virtual void SetItemSubTextByIndex(int intIndex, string strNewText)
	{
		Items[intIndex].SubText = strNewText;
	}

	public virtual void SetItemSubTextByValue(string strValue, string strNewText)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value == strValue);
		if (num >= 0)
		{
			SetItemSubTextByIndex(num, strNewText);
		}
	}

	public virtual void SetItemSubTextByValue(int intValue, string strNewText)
	{
		SetItemSubTextByValue(intValue.ToString(), strNewText);
	}

	public virtual bool MoveItemUp(int intIndex)
	{
		if (intIndex < 1)
		{
			return false;
		}
		bool num = IsSelectedByIndex(intIndex);
		bool num2 = IsSelectedByIndex(intIndex - 1);
		UnSelectItem(intIndex);
		UnSelectItem(intIndex - 1);
		ListBoxLineItem listBoxLineItem = _items[intIndex];
		ListBoxLineItem listBoxLineItem2 = _items[intIndex - 1];
		listBoxLineItem.Index--;
		listBoxLineItem2.Index++;
		_items[intIndex] = listBoxLineItem2;
		_items[intIndex - 1] = listBoxLineItem;
		_items[intIndex].AutoSize();
		_items[intIndex - 1].AutoSize();
		if (num2)
		{
			SelectByIndex(intIndex);
		}
		if (num)
		{
			SelectByIndex(intIndex - 1);
		}
		if (_intSelectedItem == intIndex)
		{
			_intSelectedItem--;
		}
		return true;
	}

	public virtual bool MoveItemDown(int intIndex)
	{
		if (intIndex < 0 || intIndex >= _items.Count - 1)
		{
			return false;
		}
		bool num = IsSelectedByIndex(intIndex);
		bool num2 = IsSelectedByIndex(intIndex + 1);
		UnSelectItem(intIndex);
		UnSelectItem(intIndex + 1);
		ListBoxLineItem listBoxLineItem = _items[intIndex];
		ListBoxLineItem listBoxLineItem2 = _items[intIndex + 1];
		listBoxLineItem.Index++;
		listBoxLineItem2.Index--;
		_items[intIndex] = listBoxLineItem2;
		_items[intIndex + 1] = listBoxLineItem;
		_items[intIndex].AutoSize();
		_items[intIndex + 1].AutoSize();
		if (num2)
		{
			SelectByIndex(intIndex);
		}
		if (num)
		{
			SelectByIndex(intIndex + 1);
		}
		if (_intSelectedItem == intIndex)
		{
			_intSelectedItem++;
		}
		return true;
	}

	public virtual string GetValueByText(string strText)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Text.ToLowerInvariant() == strText.Trim().ToLowerInvariant());
		if (num < 0)
		{
			return "";
		}
		return Items[num].Value;
	}

	public virtual string GetValueByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= Items.Count)
		{
			return "";
		}
		return Items[intIndex].Value;
	}

	public virtual int GetIntValueByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= Items.Count)
		{
			return -1;
		}
		return Util.ConvertToInt(Items[intIndex].Value);
	}

	public virtual string GetTextByValue(string strvalue)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strvalue.Trim().ToLowerInvariant());
		if (num < 0)
		{
			return "";
		}
		return Items[num].Text;
	}

	public virtual string GetTextByValue(int intValue)
	{
		return GetTextByValue(intValue.ToString());
	}

	public virtual string GetTextByValue(float fValue)
	{
		return GetTextByValue(fValue.ToString());
	}

	public virtual string GetTextByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= Items.Count)
		{
			return "";
		}
		return Items[intIndex].Text;
	}

	public virtual string GetSubTextByValue(string strvalue)
	{
		int num = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strvalue.Trim().ToLowerInvariant());
		if (num < 0)
		{
			return "";
		}
		return Items[num].SubText;
	}

	public virtual string GetSubTextByValue(int intValue)
	{
		return GetSubTextByValue(intValue.ToString());
	}

	public virtual string GetSubTextByValue(float fValue)
	{
		return GetSubTextByValue(fValue.ToString());
	}

	public virtual string GetSubTextByIndex(int intIndex)
	{
		if (intIndex < 0 || intIndex >= Items.Count)
		{
			return "";
		}
		return Items[intIndex].SubText;
	}

	public virtual void SelectByIndex(int intIndex, bool blnShifted = false, bool blnCtrled = false)
	{
		if (intIndex < -1 || intIndex >= Items.Count)
		{
			return;
		}
		blnShifted = blnShifted && CanMultiSelect;
		blnCtrled = blnCtrled && CanMultiSelect;
		if ((!blnShifted && !blnCtrled) || _intSelectedItem < 0)
		{
			UnSelectAllItems();
			_intSelectedItem = intIndex;
			if (_intSelectedItem >= 0 && Items[_intSelectedItem].Enabled)
			{
				Items[_intSelectedItem].Select();
				_intSelectedList.Add(intIndex);
			}
		}
		else if (blnCtrled)
		{
			if (intIndex >= 0 && intIndex < Items.Count && Items[intIndex].Enabled)
			{
				if (IsSelectedByIndex(intIndex))
				{
					UnSelectItem(intIndex);
				}
				else
				{
					Items[intIndex].Select();
					_intSelectedList.Add(intIndex);
				}
			}
		}
		else if (blnShifted)
		{
			UnSelectAllItems();
			SelectByRange(intIndex);
		}
		if (_intSelectedItem >= 0 && this.OnChange != null)
		{
			this.OnChange(base.gameObject, _intSelectedItem);
		}
	}

	public virtual void SelectByValue(string strValue)
	{
		int intIndex = Items.FindIndex((ListBoxLineItem x) => x.Value.ToLowerInvariant() == strValue.ToLowerInvariant());
		SelectByIndex(intIndex);
	}

	public virtual void SelectByText(string strText)
	{
		int intIndex = Items.FindIndex((ListBoxLineItem x) => x.Text.ToLowerInvariant() == strText.ToLowerInvariant());
		SelectByIndex(intIndex);
	}

	public virtual void Unselect()
	{
		UnSelectAllItems();
		_intSelectedItem = -1;
		_intSelectedList = new List<int>();
	}

	public virtual void HandleDoubleClick(int intIndex)
	{
		if (AllowDoubleClick && intIndex >= -1 && intIndex < Items.Count)
		{
			UnSelectAllItems();
			_intSelectedItem = intIndex;
			if (_intSelectedItem >= 0 && Items[_intSelectedItem].Enabled)
			{
				Items[_intSelectedItem].Select();
				_intSelectedList.Add(intIndex);
			}
			if (_intSelectedItem >= 0 && this.OnDoubleClick != null)
			{
				this.OnDoubleClick(base.gameObject, _intSelectedItem);
			}
		}
	}

	public virtual bool IsSelectedByIndex(int intIndex)
	{
		if (_intSelectedItem != intIndex)
		{
			return _intSelectedList.FindIndex((int x) => x == intIndex) >= 0;
		}
		return true;
	}

	public virtual void UpdateListBoxContainerSize()
	{
		Vector2 sizeDelta = ContainerRect.sizeDelta;
		sizeDelta.y = (Height + Spacing) * (float)Items.Count + Spacing;
		ContainerRect.sizeDelta = sizeDelta;
	}

	public virtual void Hide()
	{
		base.gameObject.SetActive(value: true);
		if (ListBoxMode == ListBoxModes.ListBox)
		{
			GetComponent<Image>().enabled = false;
			if (ScrollBarObject != null)
			{
				ScrollBarObject.SetActive(value: false);
			}
			if (ScrollRectObject != null)
			{
				ScrollRectObject.SetActive(value: false);
			}
			if (ListBoxTitle != null)
			{
				ListBoxTitle.gameObject.SetActive(value: false);
			}
		}
	}

	public virtual void Show()
	{
		base.gameObject.SetActive(value: true);
		if (ListBoxMode == ListBoxModes.ListBox)
		{
			GetComponent<Image>().enabled = true;
			if (ScrollBarObject != null)
			{
				ScrollBarObject.SetActive(value: true);
			}
			if (ScrollRectObject != null)
			{
				ScrollRectObject.SetActive(value: true);
			}
			if (ListBoxTitle != null)
			{
				ListBoxTitle.gameObject.SetActive(value: true);
			}
		}
	}
}
