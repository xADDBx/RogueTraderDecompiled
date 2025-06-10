using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickOverseerPaperVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Name;

	public readonly string Title;

	public readonly BaseUnitEntity UnitToShow;

	public TooltipBrickOverseerPaperVM(Sprite icon, string name, string title, BaseUnitEntity unitToShow = null)
	{
		Icon = icon;
		Name = name;
		Title = title;
		UnitToShow = unitToShow;
	}
}
