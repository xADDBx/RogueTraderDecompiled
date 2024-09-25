using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickCalculatedFormulaView : TooltipBaseBrickView<TooltipBrickCalculatedFormulaVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	protected TextMeshProUGUI m_DescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	[SerializeField]
	private LayoutElement m_ValueLayoutElement;

	[Header("Settings")]
	[SerializeField]
	private float m_FlexibleWidth = 400f;

	private readonly Dictionary<TextMeshProUGUI, TextFieldParams> m_DefaultParams = new Dictionary<TextMeshProUGUI, TextFieldParams>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetText(m_TitleText, base.ViewModel.Title);
		SetText(m_DescriptionText, base.ViewModel.Description);
		SetText(m_ValueText, base.ViewModel.Value);
		SetValuePreferredWidth();
	}

	private void SetText(TextMeshProUGUI text, string label)
	{
		GetOrCreateDefaultParams(text);
		text.text = label;
		if (m_DefaultParams.TryGetValue(text, out var value) && value.FontSize.HasValue)
		{
			text.fontSize = value.FontSize.Value * FontMultiplier;
		}
	}

	private void GetOrCreateDefaultParams(TextMeshProUGUI textField)
	{
		if (textField != null && !m_DefaultParams.ContainsKey(textField))
		{
			m_DefaultParams[textField] = textField.GetTextFieldParams();
		}
	}

	private void SetValuePreferredWidth()
	{
		if ((bool)m_ValueLayoutElement)
		{
			float b = (base.ViewModel.UseFlexibleValueWidth ? ((float)base.ViewModel.Value.Length * 8f + 30f) : (-1f));
			b = Mathf.Min(200f, b);
			m_ValueLayoutElement.preferredWidth = b;
		}
	}
}
