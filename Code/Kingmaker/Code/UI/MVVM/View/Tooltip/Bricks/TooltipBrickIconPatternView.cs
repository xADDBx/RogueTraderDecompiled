using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks.AbilityPattern;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconPatternView : TooltipBaseBrickView<TooltipBrickIconPatternVM>
{
	[Header("Icon")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Frame;

	[SerializeField]
	private GameObject m_IconBlock;

	[Header("Pattern")]
	[SerializeField]
	private AbilityPatternView m_AbilityPatternView;

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[Header("Secondary")]
	[SerializeField]
	protected TextMeshProUGUI m_SecondaryText;

	[SerializeField]
	private TextMeshProUGUI m_SecondaryValue;

	[Header("Tertiary")]
	[SerializeField]
	protected TextMeshProUGUI m_TertiaryText;

	[SerializeField]
	private TextMeshProUGUI m_TertiaryValue;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 20f;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	private readonly Dictionary<TextMeshProUGUI, TextFieldParams> m_DefaultParams = new Dictionary<TextMeshProUGUI, TextFieldParams>();

	private Color32? m_DefaultFrameColor;

	protected override void BindViewImplementation()
	{
		m_DefaultFrameColor = ((m_Frame != null) ? new Color32?(m_Frame.color) : null);
		base.BindViewImplementation();
		m_AbilityPatternView.Initialize(base.ViewModel.PatternData);
		m_Icon.sprite = base.ViewModel.Icon;
		m_IconBlock.SetActive(base.ViewModel.Icon != null);
		if ((bool)m_Frame && base.ViewModel.FrameColor.HasValue)
		{
			m_Frame.color = base.ViewModel.FrameColor.Value;
		}
		ApplyValues(m_TitleText, null, base.ViewModel.TitleValues);
		ApplyValues(m_SecondaryText, m_SecondaryValue, base.ViewModel.SecondaryValues);
		ApplyValues(m_TertiaryText, m_TertiaryValue, base.ViewModel.TertiaryValues);
		AddDisposable(m_SecondaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		AddDisposable(m_TertiaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(m_Icon.SetTooltip(base.ViewModel.Tooltip));
		}
		SetTextSize();
	}

	private void SetTextSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_TitleText.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * FontMultiplier;
		m_SecondaryText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_SecondaryValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_TertiaryText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_TertiaryValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
		m_AbilityPatternView.Destroy();
		if ((bool)m_Frame && m_DefaultFrameColor.HasValue)
		{
			m_Frame.color = m_DefaultFrameColor.Value;
		}
	}

	private void ApplyValues(TextMeshProUGUI text, TextMeshProUGUI value, TooltipBrickIconPattern.TextFieldValues textFieldValues)
	{
		text.ApplyTextFieldParams(textFieldValues?.TextParams, GetOrCreateDefaultParams(text));
		value.ApplyTextFieldParams(textFieldValues?.ValueParams, GetOrCreateDefaultParams(value));
		if (text != null)
		{
			text.text = textFieldValues?.Text;
			text.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Text));
		}
		if (value != null)
		{
			value.text = textFieldValues?.Value;
			value.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Value));
		}
	}

	private TextFieldParams GetOrCreateDefaultParams(TextMeshProUGUI textField)
	{
		TextFieldParams value = null;
		if (textField != null && !m_DefaultParams.TryGetValue(textField, out value))
		{
			TextFieldParams textFieldParams2 = (m_DefaultParams[textField] = textField.GetTextFieldParams());
			value = textFieldParams2;
		}
		return value;
	}
}
