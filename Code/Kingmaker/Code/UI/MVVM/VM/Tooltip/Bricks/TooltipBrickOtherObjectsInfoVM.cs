using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickOtherObjectsInfoVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly BlueprintArtificialObject BlueprintOtherObject;

	public readonly BlueprintStarSystemMap BlueprintStarSystemMap;

	public TooltipBrickOtherObjectsInfoVM(BlueprintArtificialObject blueprintOtherObject, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		Name = blueprintOtherObject.Name;
		Icon = blueprintOtherObject.Icon;
		BlueprintOtherObject = blueprintOtherObject;
		BlueprintStarSystemMap = blueprintStarSystemMap;
	}
}
