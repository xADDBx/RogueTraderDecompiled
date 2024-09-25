using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickResourceInfo : ITooltipBrick
{
	private readonly BlueprintResource m_BlueprintResource;

	private readonly int m_Count;

	public TooltipBrickResourceInfo(BlueprintResource blueprintResource, int count)
	{
		m_BlueprintResource = blueprintResource;
		m_Count = count;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickResourceInfoVM(m_BlueprintResource, m_Count);
	}
}
