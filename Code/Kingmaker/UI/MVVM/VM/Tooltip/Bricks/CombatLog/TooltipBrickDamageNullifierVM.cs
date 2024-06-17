using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickDamageNullifierVM : TooltipBaseBrickVM
{
	public readonly int ChanceRoll;

	public readonly int ResultRoll;

	public readonly int ResultValue;

	public readonly string ReasonText;

	public readonly List<ReasonBuffItemVM> ReasonBuffItems = new List<ReasonBuffItemVM>();

	public readonly string ResultText;

	public TooltipBrickDamageNullifierVM(int chanceRoll, int resultRoll, int resultValue, string reasonText, IReadOnlyList<NullifyInformation.BuffInformation> reasonItems, string resultText)
	{
		ChanceRoll = chanceRoll;
		ResultRoll = resultRoll;
		ResultValue = resultValue;
		ReasonText = reasonText;
		ResultText = resultText;
		foreach (NullifyInformation.BuffInformation reasonItem in reasonItems)
		{
			ReasonBuffItemVM item = new ReasonBuffItemVM(reasonItem);
			ReasonBuffItems.Add(item);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		ReasonBuffItems.ForEach(delegate(ReasonBuffItemVM item)
		{
			item.Dispose();
		});
		ReasonBuffItems.Clear();
	}
}
