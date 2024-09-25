using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateLootEntity : TooltipBaseTemplate
{
	private readonly Sprite m_Icon;

	private readonly string m_Title;

	private readonly string m_Description;

	private readonly int m_Count;

	private readonly float m_ProfitFactorCost;

	public TooltipTemplateLootEntity(LootEntry lootEntry)
	{
		m_Icon = lootEntry.Item.Icon;
		m_Title = lootEntry.Item.Name;
		m_Description = lootEntry.Item.Description;
		m_Count = lootEntry.Count;
		m_ProfitFactorCost = lootEntry.ProfitFactorCost;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickIconAndName(m_Icon, m_Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(m_Description, TooltipTextType.Paragraph);
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		yield return new TooltipBrickSeparator();
		int count = m_Count;
		string leftLine = count.ToString();
		float profitFactorCost = m_ProfitFactorCost;
		yield return new TooltipBrickDoubleText(leftLine, profitFactorCost.ToString());
	}
}
