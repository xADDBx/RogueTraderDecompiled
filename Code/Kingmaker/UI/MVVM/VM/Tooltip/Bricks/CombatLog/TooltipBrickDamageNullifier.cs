using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickDamageNullifier : ITooltipBrick
{
	private readonly int m_ChanceRoll;

	private readonly int m_ResultRoll;

	private readonly int m_ResultValue;

	private readonly string m_ReasonText;

	private readonly IReadOnlyList<NullifyInformation.BuffInformation> m_ReasonItems;

	private readonly string m_ResultText;

	public TooltipBrickDamageNullifier(int chanceRoll, int resultRoll, int resultValue, string reasonText, IReadOnlyList<NullifyInformation.BuffInformation> reasonItems, string resultText)
	{
		m_ChanceRoll = chanceRoll;
		m_ResultRoll = resultRoll;
		m_ResultValue = resultValue;
		m_ReasonText = reasonText;
		m_ReasonItems = reasonItems;
		m_ResultText = resultText;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickDamageNullifierVM(m_ChanceRoll, m_ResultRoll, m_ResultValue, m_ReasonText, m_ReasonItems, m_ResultText);
	}
}
