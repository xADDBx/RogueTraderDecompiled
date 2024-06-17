using UnityEngine;
using UnityEngine.UI;

namespace UnityUIControls;

public class ProgressBar : MonoBehaviour
{
	private GameObject _goProgressBar;

	private GameObject _goProgressContainer;

	private GameObject _goProgressText;

	private GameObject _goProgressCaption;

	[SerializeField]
	private string _strCaption = "";

	[SerializeField]
	private float _fValue;

	[SerializeField]
	private float _fWidth;

	[SerializeField]
	private Color _colTextColor = Color.white;

	[SerializeField]
	private Color _colTextShadow = Color.black;

	[SerializeField]
	private Color _colBarColor;

	private GameObject ProgressBarLine
	{
		get
		{
			if (_goProgressBar == null)
			{
				_goProgressBar = base.gameObject.transform.GetChild(1).GetChild(0).gameObject;
			}
			return _goProgressBar;
		}
	}

	private GameObject ProgressContainer
	{
		get
		{
			if (_goProgressContainer == null)
			{
				_goProgressContainer = base.gameObject.transform.GetChild(1).gameObject;
			}
			return _goProgressContainer;
		}
	}

	private GameObject ProgressText
	{
		get
		{
			if (_goProgressText == null)
			{
				_goProgressText = base.gameObject.transform.GetChild(2).gameObject;
			}
			return _goProgressText;
		}
	}

	private GameObject ProgressCaption
	{
		get
		{
			if (_goProgressCaption == null)
			{
				_goProgressCaption = base.gameObject.transform.GetChild(3).gameObject;
			}
			return _goProgressCaption;
		}
	}

	private float ProgressWidth
	{
		get
		{
			if (_fWidth <= 0f)
			{
				_fWidth = ProgressContainer.GetComponent<RectTransform>().rect.width;
			}
			return _fWidth;
		}
	}

	public string Caption
	{
		set
		{
			_strCaption = value.Trim();
			ProgressCaption.GetComponent<Text>().text = _strCaption;
		}
	}

	public float Progress
	{
		get
		{
			return _fValue;
		}
		set
		{
			_fValue = value;
			if (_fValue > 1f)
			{
				_fValue = Mathf.Clamp(_fValue / 100f, 0f, 1f);
			}
			Vector2 sizeDelta = ProgressBarLine.GetComponent<RectTransform>().sizeDelta;
			sizeDelta.x = ProgressWidth * _fValue;
			ProgressBarLine.GetComponent<RectTransform>().sizeDelta = sizeDelta;
			ProgressText.GetComponent<Text>().text = (_fValue * 100f).ToString("##0") + "%";
		}
	}

	public Color TextColor
	{
		get
		{
			return _colTextColor;
		}
		set
		{
			_colTextColor = value;
			ProgressText.GetComponent<Text>().color = value;
		}
	}

	public Color TextShadow
	{
		get
		{
			return _colTextShadow;
		}
		set
		{
			_colTextShadow = value;
			ProgressText.GetComponent<Shadow>().effectColor = value;
		}
	}

	public Color ProgressBarColor
	{
		get
		{
			return _colBarColor;
		}
		set
		{
			_colBarColor = value;
			ProgressBarLine.GetComponent<Image>().color = value;
		}
	}

	private void Awake()
	{
		_fValue = 0f;
		Progress = _fValue;
		Caption = "";
	}

	public void Reset()
	{
		_fValue = 0f;
		Progress = _fValue;
		Caption = "";
	}

	public void SetProgress(float fCurrent, float fMaximum)
	{
		_fValue = Mathf.Clamp(fCurrent / fMaximum, 0f, 1f);
		Progress = _fValue;
	}
}
