using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickTextSignatureValueView : TooltipBaseBrickView<TooltipBrickTextSignatureValueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_SignatureText;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Text, m_SignatureText, m_Value);
		m_Text.text = base.ViewModel.Text;
		m_SignatureText.text = base.ViewModel.SignatureText;
		m_Value.text = base.ViewModel.Value;
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
