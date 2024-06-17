using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickResourceInfoVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly int Count;

	public TooltipBrickResourceInfoVM(BlueprintResource blueprintResource, int count)
	{
		Name = blueprintResource.Name;
		Icon = blueprintResource.Icon;
		Count = count;
	}
}
