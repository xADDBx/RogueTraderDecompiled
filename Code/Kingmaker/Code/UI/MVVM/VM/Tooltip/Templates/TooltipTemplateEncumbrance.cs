using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public abstract class TooltipTemplateEncumbrance : TooltipBaseTemplate
{
	protected EncumbranceHelper.CarryingCapacity Capacity;

	protected virtual void AddPenalty(List<ITooltipBrick> bricks)
	{
	}

	protected static void AddTitle(List<ITooltipBrick> bricks, string title)
	{
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H1));
	}

	protected static void AddIconValueSmall(List<ITooltipBrick> bricks, string label, string value)
	{
		bricks.Add(new TooltipBrickIconValueStat(label, value, null, TooltipIconValueStatType.Small));
	}

	protected static void AddStatModifier(List<ITooltipBrick> bricks, ModifiableValue value)
	{
	}
}
