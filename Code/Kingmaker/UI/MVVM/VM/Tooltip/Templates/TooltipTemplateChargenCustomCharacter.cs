using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateChargenCustomCharacter : TooltipBaseTemplate
{
	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickTitle(UIStrings.Instance.NewGameWin.CreateNewCharacter)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (!RootUIContext.Instance.IsMainMenu)
		{
			return new List<ITooltipBrick>();
		}
		return new List<ITooltipBrick>
		{
			new TooltipBrickText(UIStrings.Instance.NewGameWin.CreateNewCharacterDescription)
		};
	}
}
