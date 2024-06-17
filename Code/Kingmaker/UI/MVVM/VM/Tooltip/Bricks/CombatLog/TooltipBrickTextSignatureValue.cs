using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTextSignatureValue : ITooltipBrick
{
	private readonly string m_Text;

	private readonly string m_SignatureText;

	private readonly string m_Value;

	public TooltipBrickTextSignatureValue(string text, string signatureText, string value)
	{
		m_Text = text;
		m_SignatureText = signatureText;
		m_Value = value;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTextSignatureValueVM(m_Text, m_SignatureText, m_Value);
	}
}
