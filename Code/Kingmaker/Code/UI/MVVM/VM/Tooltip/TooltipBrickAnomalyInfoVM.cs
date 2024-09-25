using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class TooltipBrickAnomalyInfoVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly BlueprintAnomaly BlueprintAnomaly;

	public readonly BlueprintStarSystemMap BlueprintStarSystemMap;

	public TooltipBrickAnomalyInfoVM(BlueprintAnomaly blueprintAnomaly, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		Name = blueprintAnomaly.Name;
		Icon = blueprintAnomaly.Icon;
		BlueprintAnomaly = blueprintAnomaly;
		BlueprintStarSystemMap = blueprintStarSystemMap;
	}
}
