using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.UI;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUIProperty : TooltipBaseTemplate
{
	public readonly UIPropertySettings PropertySettings;

	public TooltipTemplateUIProperty(UIPropertySettings propertySettings)
	{
		PropertySettings = propertySettings;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		if (type == TooltipTemplateType.Info)
		{
			AddSource(list);
		}
		return list;
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(PropertySettings.Description, TooltipTextType.BoldCentered));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (PropertySettings.DescriptionFact != null)
		{
			bricks.Add(new TooltipBrickSeparator());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
			bricks.Add(new TooltipBrickFeature(PropertySettings.DescriptionFact));
		}
	}
}
