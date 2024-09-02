using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuffDOTVM : TooltipBaseBrickVM
{
	public string Damage;

	private readonly Buff m_Buff;

	public TooltipBrickBuffDOTVM(Buff buff)
	{
		m_Buff = buff;
		Damage = ((m_Buff != null) ? m_Buff.Rank.ToString() : string.Empty);
	}
}
