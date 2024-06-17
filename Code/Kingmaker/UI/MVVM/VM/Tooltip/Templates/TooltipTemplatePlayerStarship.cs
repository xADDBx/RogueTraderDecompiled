using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplatePlayerStarship : TooltipBaseTemplate
{
	private readonly BlueprintStarship m_BlueprintStarship;

	public TooltipTemplatePlayerStarship(BlueprintStarship blueprintStarship)
	{
		m_BlueprintStarship = blueprintStarship;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_BlueprintStarship.PlayerShipName, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickPicture(m_BlueprintStarship.PlayerShipBigPicture);
		yield return new TooltipBrickText(m_BlueprintStarship.PlayerShipDescription + "\n" + m_BlueprintStarship.PlayerShipFlavor);
	}
}
