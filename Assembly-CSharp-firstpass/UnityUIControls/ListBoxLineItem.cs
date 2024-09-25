using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityUIControls;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class ListBoxLineItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	private const float ELEMENT_SPACING = 3f;

	private ListBoxControl _lbControl;

	private RectTransform _rt;

	private RectTransform _rtMainTxt;

	private RectTransform _rtSubTxt;

	private RectTransform _rtImg;

	private Image _img;

	private Text _txtMainText;

	private Text _txtSubText;

	private Image _imgIcon;

	private Color _itemNormalColor;

	private Color _itemHighlightColor;

	private Color _itemSelectedColor;

	private Color _itemDisabledColor;

	private int _intIndex;

	private string _strValue = "";

	private string _strMainText = "";

	private string _strSubText = "";

	private float _fXpos = 2f;

	private float _fYpos = -2f;

	private float _fWidth;

	private float _fHeight;

	private float _fSpace = 2f;

	private bool _blnSelected;

	private bool _blnEnabled = true;

	private bool _blnShown = true;

	public GameObject ListBoxControlObject;

	private ListBoxControl LBcontrol
	{
		get
		{
			if (_lbControl == null && ListBoxControlObject != null && ListBoxControlObject.GetComponent<ListBoxControl>() != null)
			{
				_lbControl = ListBoxControlObject.GetComponent<ListBoxControl>();
			}
			return _lbControl;
		}
	}

	private Image DisplayedIcon
	{
		get
		{
			if (_imgIcon == null && base.transform.GetChild(0).gameObject != null && base.transform.GetChild(0).GetComponent<Image>() != null)
			{
				_imgIcon = base.transform.GetChild(0).GetComponent<Image>();
				_rtImg = _imgIcon.GetComponent<RectTransform>();
			}
			return _imgIcon;
		}
	}

	private Text DisplayedMainText
	{
		get
		{
			if (_txtMainText == null && base.transform.GetChild(1).gameObject != null && base.transform.GetChild(1).GetComponent<Text>() != null)
			{
				_txtMainText = base.transform.GetChild(1).GetComponent<Text>();
				_rtMainTxt = _txtMainText.GetComponent<RectTransform>();
			}
			return _txtMainText;
		}
	}

	private Text DisplayedSubText
	{
		get
		{
			if (_txtSubText == null && base.transform.GetChild(2).gameObject != null && base.transform.GetChild(2).GetComponent<Text>() != null)
			{
				_txtSubText = base.transform.GetChild(2).GetComponent<Text>();
				_rtSubTxt = _txtSubText.GetComponent<RectTransform>();
			}
			return _txtSubText;
		}
	}

	private RectTransform ImageRT
	{
		get
		{
			if (_rtImg == null)
			{
				_rtImg = DisplayedIcon.GetComponent<RectTransform>();
			}
			return _rtImg;
		}
	}

	private RectTransform MainTextRT
	{
		get
		{
			if (_rtMainTxt == null)
			{
				_rtMainTxt = DisplayedMainText.GetComponent<RectTransform>();
			}
			return _rtMainTxt;
		}
	}

	private RectTransform SubTextRT
	{
		get
		{
			if (_rtSubTxt == null)
			{
				_rtSubTxt = DisplayedSubText.GetComponent<RectTransform>();
			}
			return _rtSubTxt;
		}
	}

	private bool HasIcon => DisplayedIcon.sprite != null;

	public int Index
	{
		get
		{
			return _intIndex;
		}
		set
		{
			_intIndex = value;
		}
	}

	public string Value
	{
		get
		{
			return _strValue;
		}
		set
		{
			_strValue = value.Trim();
		}
	}

	public int IntValue
	{
		get
		{
			return Util.ConvertToInt(_strValue);
		}
		set
		{
			_strValue = value.ToString();
		}
	}

	public string Text
	{
		get
		{
			return _strMainText;
		}
		set
		{
			_strMainText = value.Trim();
			DisplayedMainText.text = _strMainText;
			base.gameObject.name = _strMainText;
			UpdateContent();
		}
	}

	public string SubText
	{
		get
		{
			return _strSubText;
		}
		set
		{
			_strSubText = value.Trim();
			DisplayedSubText.text = _strSubText;
			UpdateContent();
		}
	}

	public float X
	{
		get
		{
			return _fXpos;
		}
		set
		{
			_fXpos = value;
			UpdatePosition();
		}
	}

	public float Y
	{
		get
		{
			return _fYpos;
		}
		set
		{
			_fYpos = value;
			UpdatePosition();
		}
	}

	public float Width
	{
		get
		{
			return _fWidth;
		}
		set
		{
			_fWidth = value;
			UpdatePosition();
		}
	}

	public float Height
	{
		get
		{
			return _fHeight;
		}
		set
		{
			_fHeight = value;
			UpdatePosition();
		}
	}

	public float Spacing
	{
		get
		{
			return _fSpace;
		}
		set
		{
			_fSpace = value;
		}
	}

	public Color ItemNormalColor
	{
		get
		{
			return _itemNormalColor;
		}
		set
		{
			_itemNormalColor = value;
		}
	}

	public Color ItemHighlightColor
	{
		get
		{
			return _itemHighlightColor;
		}
		set
		{
			_itemHighlightColor = value;
		}
	}

	public Color ItemSelectedColor
	{
		get
		{
			return _itemSelectedColor;
		}
		set
		{
			_itemSelectedColor = value;
		}
	}

	public Color ItemDisabledColor
	{
		get
		{
			return _itemDisabledColor;
		}
		set
		{
			_itemDisabledColor = value;
			UpdateContent();
		}
	}

	public bool Selected
	{
		get
		{
			return _blnSelected;
		}
		set
		{
			_blnSelected = value;
		}
	}

	public bool Enabled
	{
		get
		{
			return _blnEnabled;
		}
		set
		{
			_blnEnabled = value;
			UpdateContent();
		}
	}

	public bool Shown
	{
		get
		{
			return _blnShown;
		}
		set
		{
			_blnShown = value;
			UpdateContent();
		}
	}

	public void SetIcon(Sprite sprImage)
	{
		DisplayedIcon.sprite = sprImage;
		UpdateContent();
	}

	public void SetIcon(string strImagePath)
	{
		DisplayedIcon.sprite = Resources.Load<Sprite>(strImagePath);
		UpdateContent();
	}

	private void Awake()
	{
		_rt = GetComponent<RectTransform>();
		_img = GetComponent<Image>();
		_fWidth = _rt.sizeDelta.x;
		_fHeight = _rt.sizeDelta.y;
	}

	protected void UpdatePosition()
	{
		if (Application.isPlaying)
		{
			_rt.localPosition = new Vector3(_fXpos, _fYpos, 0f);
			_rt.sizeDelta = new Vector2(_fWidth, _fHeight);
		}
	}

	protected void UpdateContent()
	{
		if (Application.isPlaying)
		{
			Vector2 zero = Vector2.zero;
			if (HasIcon)
			{
				DisplayedIcon.gameObject.SetActive(value: true);
				zero.y = _fHeight - 6f;
				zero.x = zero.y;
				ImageRT.sizeDelta = zero;
				zero.x = 3f;
				zero.y = 0f - _fHeight / 2f;
				ImageRT.localPosition = zero;
				zero.y = _fHeight - 6f;
				zero.x = _fWidth - (ImageRT.sizeDelta.x + 9f) - (float)((!(_strSubText == "")) ? 50 : 0);
				MainTextRT.sizeDelta = zero;
				zero.x = ImageRT.sizeDelta.x + 6f;
				zero.y = 0f - _fHeight / 2f;
				MainTextRT.localPosition = zero;
			}
			else
			{
				DisplayedIcon.gameObject.SetActive(value: false);
				zero.y = _fHeight - 6f;
				zero.x = _fWidth - 6f - (float)((!(_strSubText == "")) ? 50 : 0);
				MainTextRT.sizeDelta = zero;
				zero.x = 3f;
				zero.y = 0f - _fHeight / 2f;
				MainTextRT.localPosition = zero;
			}
			if (_strSubText != "")
			{
				zero = SubTextRT.sizeDelta;
				zero.y = _fHeight - 6f;
				SubTextRT.sizeDelta = zero;
			}
			if (!Enabled)
			{
				_img.color = ItemDisabledColor;
			}
			else if (Selected)
			{
				_img.color = ItemSelectedColor;
			}
			else
			{
				_img.color = ItemNormalColor;
			}
		}
	}

	public void AutoSize()
	{
		_fXpos = _fSpace;
		_fYpos = ((_fHeight + _fSpace) * (float)_intIndex + _fSpace) * -1f;
		UpdatePosition();
	}

	public void Destroy()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(base.gameObject, 0.01f);
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public void Select()
	{
		if (Enabled)
		{
			Selected = true;
			_img.color = ItemSelectedColor;
		}
	}

	public void UnSelect()
	{
		if (Enabled)
		{
			Selected = false;
			_img.color = ItemNormalColor;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Enabled)
		{
			_img.color = ItemHighlightColor;
		}
		else
		{
			_img.color = ItemDisabledColor;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!Enabled)
		{
			_img.color = ItemDisabledColor;
		}
		else if (LBcontrol.IsSelectedByIndex(Index))
		{
			_img.color = ItemSelectedColor;
		}
		else
		{
			_img.color = ItemNormalColor;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Enabled)
		{
			bool blnShifted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool blnCtrled = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			LBcontrol.SelectByIndex(Index, blnShifted, blnCtrled);
			if (eventData.clickCount > 1 || Input.touchCount > 1)
			{
				LBcontrol.HandleDoubleClick(Index);
			}
		}
	}
}
