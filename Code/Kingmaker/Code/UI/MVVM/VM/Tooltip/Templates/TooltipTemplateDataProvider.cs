using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateDataProvider : TooltipBaseTemplate
{
	private readonly IUIDataProvider m_DataProvider;

	public TooltipTemplateDataProvider(IUIDataProvider dataProvider)
	{
		m_DataProvider = dataProvider;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickEntityHeader(m_DataProvider.Name, m_DataProvider.Icon, hasUpgrade: false);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(m_DataProvider.Description, null), TooltipTextType.Paragraph);
	}
}
