using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUIFeature : TooltipBaseTemplate
{
	public readonly UIFeature UIFeature;

	public TooltipTemplateUIFeature(UIFeature uiFeature)
	{
		UIFeature = uiFeature;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickFeature(UIFeature, isHeader: true);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		AddSource(list);
		AddSelected(list);
		return list;
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(UIFeature.Description, null)));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (UIFeature?.Source != null)
		{
			bricks.Add(new TooltipBrickSeparator());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
			bricks.Add(new TooltipBrickFeature(UIFeature?.Source));
		}
	}

	private void AddSelected(List<ITooltipBrick> bricks)
	{
	}
}
