using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateEncyclopedia : TooltipBaseTemplate
{
	private readonly BlueprintEncyclopediaPage m_Page;

	public TooltipTemplateEncyclopedia(BlueprintEncyclopediaPage page)
	{
		m_Page = page;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Page != null)
		{
			yield return new TooltipBrickTitle(m_Page.Title);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_Page == null)
		{
			return null;
		}
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddEncyclopediaPage(list, m_Page);
		return list;
	}

	private void AddEncyclopediaPage(List<ITooltipBrick> bricks, BlueprintEncyclopediaPage page)
	{
		foreach (BlueprintEncyclopediaBlock block in m_Page.Blocks)
		{
			if (!(block is BlueprintEncyclopediaBlockText blueprintEncyclopediaBlockText))
			{
				if (!(block is BlueprintEncyclopediaBlockImage blueprintEncyclopediaBlockImage))
				{
					if (!(block is BlueprintEncyclopediaBlockPages blueprintEncyclopediaBlockPages))
					{
						continue;
					}
					List<BlueprintEncyclopediaPage> list = page.ChildPages.Select((BlueprintEncyclopediaPageReference p) => p.Get()).ToList();
					if (blueprintEncyclopediaBlockPages.Source == BlueprintEncyclopediaBlockPages.SourcePages.ByList)
					{
						list = blueprintEncyclopediaBlockPages.Pages.Select((BlueprintEncyclopediaPageReference p) => p.Get()).ToList();
					}
					foreach (BlueprintEncyclopediaPage item in list)
					{
						bricks.Add(new TooltipBrickTitle(item.Title, TooltipTitleType.H6));
						bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
						AddEncyclopediaPage(bricks, item);
					}
				}
				else
				{
					bricks.Add(new TooltipBrickPicture(blueprintEncyclopediaBlockImage.Image));
				}
			}
			else
			{
				bricks.Add(new TooltipBrickText(blueprintEncyclopediaBlockText.GetText(), TooltipTextType.Paragraph));
			}
		}
	}
}
