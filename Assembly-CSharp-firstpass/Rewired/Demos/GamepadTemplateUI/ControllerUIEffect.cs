using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Demos.GamepadTemplateUI;

[RequireComponent(typeof(Image))]
public class ControllerUIEffect : MonoBehaviour
{
	[SerializeField]
	private Color _highlightColor = Color.white;

	private Image _image;

	private Color _color;

	private Color _origColor;

	private bool _isActive;

	private float _highlightAmount;

	private void Awake()
	{
		_image = GetComponent<Image>();
		_origColor = _image.color;
		_color = _origColor;
	}

	public void Activate(float amount)
	{
		amount = Mathf.Clamp01(amount);
		if (!_isActive || amount != _highlightAmount)
		{
			_highlightAmount = amount;
			_color = Color.Lerp(_origColor, _highlightColor, _highlightAmount);
			_isActive = true;
			RedrawImage();
		}
	}

	public void Deactivate()
	{
		if (_isActive)
		{
			_color = _origColor;
			_highlightAmount = 0f;
			_isActive = false;
			RedrawImage();
		}
	}

	private void RedrawImage()
	{
		_image.color = _color;
		_image.enabled = _isActive;
	}
}
