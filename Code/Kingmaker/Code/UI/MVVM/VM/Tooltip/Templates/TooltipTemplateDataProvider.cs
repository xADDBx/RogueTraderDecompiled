using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

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
		if (!(m_DataProvider.Icon != null))
		{
			UIUtility.GetAbilityAcronym(m_DataProvider.NameForAcronym);
		}
		Sprite icon = ((m_DataProvider.Icon != null) ? m_DataProvider.Icon : UIUtility.GetIconByText(m_DataProvider.NameForAcronym));
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_DataProvider.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		yield return new TooltipBrickIconPattern(icon, null, titleValues);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(m_DataProvider.Description, null), TooltipTextType.Paragraph);
	}
}
