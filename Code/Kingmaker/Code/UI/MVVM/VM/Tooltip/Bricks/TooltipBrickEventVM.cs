using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEventVM : TooltipBaseBrickVM
{
	public readonly string EventName;

	public readonly string EventDescription;

	public readonly EventRelationType Type;

	public TooltipBrickEventVM(BlueprintColonyEvent @event, EventRelationType type)
	{
		EventName = @event.Name;
		EventDescription = @event.Description;
		Type = type;
	}
}
