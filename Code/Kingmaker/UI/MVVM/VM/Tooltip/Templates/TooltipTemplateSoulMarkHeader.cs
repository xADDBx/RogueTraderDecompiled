using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSoulMarkHeader : TooltipBaseTemplate
{
	private readonly SoulMarkDirection m_Direction;

	private readonly BlueprintSoulMark m_BlueprintSoulMark;

	private readonly List<int> m_RankThresholds = new List<int>();

	private readonly int m_CurrentTier;

	private readonly int m_CurrentValue;

	private readonly int m_MaxValue;

	public TooltipTemplateSoulMarkHeader(BaseUnitEntity unit, SoulMarkDirection direction)
	{
		m_Direction = direction;
		m_BlueprintSoulMark = SoulMarkShiftExtension.GetBaseSoulMarkFor(direction);
		if (m_BlueprintSoulMark != null)
		{
			SoulMarkTooltipExtensions.GetSoulMarkInfo(m_BlueprintSoulMark, unit, out m_RankThresholds, out m_MaxValue, out m_CurrentValue, out m_CurrentTier);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIUtility.GetSoulMarkDirectionText(m_Direction).Text, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return GetBodyBricks(type);
	}

	private IEnumerable<ITooltipBrick> GetBodyBricks(TooltipTemplateType type)
	{
		return type switch
		{
			TooltipTemplateType.Tooltip => GetBodyTooltip(), 
			TooltipTemplateType.Info => GetBodyInfo(), 
			_ => new List<ITooltipBrick>(), 
		};
	}

	private List<ITooltipBrick> GetBodyTooltip()
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
		int currentTier = m_CurrentTier;
		string title = string.Format(format, currentTier.ToString(), UIUtility.GetSoulMarkRankText(m_CurrentTier).Text);
		list.Add(new TooltipBrickTitle(title, TooltipTitleType.H4, TextAlignmentOptions.Left));
		string glossaryKeyByDirection = SoulMarkTooltipExtensions.GetGlossaryKeyByDirection(m_Direction);
		list.Add(new TooltipBrickText(UIUtility.GetGlossaryEntry(glossaryKeyByDirection).GetDescription().Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left, needChangeSize: true, 20));
		list.AddRange(SoulMarkTooltipExtensions.GetSlider(m_RankThresholds, m_CurrentValue, m_MaxValue));
		return list;
	}

	private List<ITooltipBrick> GetBodyInfo()
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_CurrentTier >= 0)
		{
			string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
			int currentTier = m_CurrentTier;
			string title = string.Format(format, currentTier.ToString(), UIUtility.GetSoulMarkRankText(m_CurrentTier).Text);
			list.Add(new TooltipBrickTitle(title, TooltipTitleType.H4, TextAlignmentOptions.Left));
		}
		string glossaryKeyByDirection = SoulMarkTooltipExtensions.GetGlossaryKeyByDirection(m_Direction);
		list.Add(new TooltipBrickText(UIUtility.GetGlossaryEntry(glossaryKeyByDirection).GetDescription().Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left, needChangeSize: true, 20));
		for (int i = 0; i < m_BlueprintSoulMark.ComponentsArray.Length; i++)
		{
			int num = i + 1;
			list.AddRange(SoulMarkTooltipExtensions.GetFeatureBlock(m_BlueprintSoulMark, num, num == m_CurrentTier));
		}
		return list;
	}
}
