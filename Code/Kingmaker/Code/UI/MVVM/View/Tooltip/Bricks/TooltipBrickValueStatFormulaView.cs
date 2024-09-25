using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickValueStatFormulaView : TooltipBaseBrickView<TooltipBrickValueStatFormulaVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Symbol;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Value.text = base.ViewModel.Value;
		m_Symbol.text = base.ViewModel.Symbol;
		m_Title.text = base.ViewModel.Name;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Value.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_Symbol.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_Title.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}
