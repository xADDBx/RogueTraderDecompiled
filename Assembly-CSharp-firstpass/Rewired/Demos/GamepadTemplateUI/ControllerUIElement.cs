using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Demos.GamepadTemplateUI;

[RequireComponent(typeof(Image))]
public class ControllerUIElement : MonoBehaviour
{
	[SerializeField]
	private Color _highlightColor = Color.white;

	[SerializeField]
	private ControllerUIEffect _positiveUIEffect;

	[SerializeField]
	private ControllerUIEffect _negativeUIEffect;

	[SerializeField]
	private Text _label;

	[SerializeField]
	private Text _positiveLabel;

	[SerializeField]
	private Text _negativeLabel;

	[SerializeField]
	private ControllerUIElement[] _childElements = new ControllerUIElement[0];

	private Image _image;

	private Color _color;

	private Color _origColor;

	private bool _isActive;

	private float _highlightAmount;

	private bool hasEffects
	{
		get
		{
			if (!(_positiveUIEffect != null))
			{
				return _negativeUIEffect != null;
			}
			return true;
		}
	}

	private void Awake()
	{
		_image = GetComponent<Image>();
		_origColor = _image.color;
		_color = _origColor;
		ClearLabels();
	}

	public void Activate(float amount)
	{
		amount = Mathf.Clamp(amount, -1f, 1f);
		if (hasEffects)
		{
			if (amount < 0f && _negativeUIEffect != null)
			{
				_negativeUIEffect.Activate(Mathf.Abs(amount));
			}
			if (amount > 0f && _positiveUIEffect != null)
			{
				_positiveUIEffect.Activate(Mathf.Abs(amount));
			}
		}
		else
		{
			if (_isActive && amount == _highlightAmount)
			{
				return;
			}
			_highlightAmount = amount;
			_color = Color.Lerp(_origColor, _highlightColor, _highlightAmount);
		}
		_isActive = true;
		RedrawImage();
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!(_childElements[i] == null))
			{
				_childElements[i].Activate(amount);
			}
		}
	}

	public void Deactivate()
	{
		if (!_isActive)
		{
			return;
		}
		_color = _origColor;
		_highlightAmount = 0f;
		if (_positiveUIEffect != null)
		{
			_positiveUIEffect.Deactivate();
		}
		if (_negativeUIEffect != null)
		{
			_negativeUIEffect.Deactivate();
		}
		_isActive = false;
		RedrawImage();
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!(_childElements[i] == null))
			{
				_childElements[i].Deactivate();
			}
		}
	}

	public void SetLabel(string text, AxisRange labelType)
	{
		Text text2 = labelType switch
		{
			AxisRange.Full => _label, 
			AxisRange.Positive => _positiveLabel, 
			AxisRange.Negative => _negativeLabel, 
			_ => null, 
		};
		if (text2 != null)
		{
			text2.text = text;
		}
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!(_childElements[i] == null))
			{
				_childElements[i].SetLabel(text, labelType);
			}
		}
	}

	public void ClearLabels()
	{
		if (_label != null)
		{
			_label.text = string.Empty;
		}
		if (_positiveLabel != null)
		{
			_positiveLabel.text = string.Empty;
		}
		if (_negativeLabel != null)
		{
			_negativeLabel.text = string.Empty;
		}
		if (_childElements.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _childElements.Length; i++)
		{
			if (!(_childElements[i] == null))
			{
				_childElements[i].ClearLabels();
			}
		}
	}

	private void RedrawImage()
	{
		_image.color = _color;
	}
}
