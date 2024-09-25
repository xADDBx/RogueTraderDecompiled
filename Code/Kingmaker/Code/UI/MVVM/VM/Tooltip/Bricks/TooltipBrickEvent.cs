using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEvent : ITooltipBrick
{
	private readonly BlueprintColonyEvent m_Event;

	private readonly EventRelationType m_Type;

	public TooltipBrickEvent(BlueprintColonyEvent @event, EventRelationType type = EventRelationType.Bad)
	{
		m_Event = @event;
		m_Type = type;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickEventVM(m_Event, m_Type);
	}
}
