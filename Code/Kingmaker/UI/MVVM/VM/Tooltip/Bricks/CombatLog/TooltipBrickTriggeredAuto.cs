using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTriggeredAuto : ITooltipBrick
{
	private readonly string m_TriggeredAutoText;

	private readonly IReadOnlyList<FeatureCountableFlag.BuffList.Element> m_ReasonItems;

	private readonly bool m_IsSuccess;

	public TooltipBrickTriggeredAuto(string triggeredAutoText, IReadOnlyList<FeatureCountableFlag.BuffList.Element> reasonItems, bool isSuccess)
	{
		m_TriggeredAutoText = triggeredAutoText;
		m_ReasonItems = reasonItems;
		m_IsSuccess = isSuccess;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTriggeredAutoVM(m_TriggeredAutoText, m_ReasonItems, m_IsSuccess);
	}
}
