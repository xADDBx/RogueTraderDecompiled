using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTextSignatureValueVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly string SignatureText;

	public readonly string Value;

	public TooltipBrickTextSignatureValueVM(string text, string signatureText, string value)
	{
		Text = text;
		SignatureText = signatureText;
		Value = value;
	}
}
