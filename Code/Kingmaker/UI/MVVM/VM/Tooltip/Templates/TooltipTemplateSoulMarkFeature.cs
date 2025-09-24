using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSoulMarkFeature : TooltipBaseTemplate
{
	private readonly int m_Tier;

	private readonly BlueprintSoulMark m_BlueprintSoulMark;

	private readonly List<int> m_RankThresholds = new List<int>();

	private readonly int m_CurrentValue;

	private readonly SoulMarkDirection m_SoulMarkDirection;

	private readonly SoulMarkDirection? m_MainDirection;

	private readonly BlueprintFeature m_Feature;

	public TooltipTemplateSoulMarkFeature(BaseUnitEntity unit, SoulMarkDirection direction, int tier, SoulMarkDirection? mainDirection)
	{
		m_Tier = tier;
		m_MainDirection = mainDirection;
		m_SoulMarkDirection = direction;
		BlueprintSoulMark baseSoulMarkFor = SoulMarkShiftExtension.GetBaseSoulMarkFor(direction);
		if (baseSoulMarkFor != null && tier >= 0 && tier <= baseSoulMarkFor.ComponentsArray.Length)
		{
			m_BlueprintSoulMark = SoulMarkTooltipExtensions.GetSoulMarkWithTier(baseSoulMarkFor, tier);
			m_Feature = SoulMarkTooltipExtensions.GetSoulMarkFeature(baseSoulMarkFor, tier);
			SoulMarkTooltipExtensions.GetSoulMarkInfo(baseSoulMarkFor, unit, out m_RankThresholds, out var _, out m_CurrentValue, out var _);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
		int tier = m_Tier;
		string title = string.Format(format, tier.ToString(), UIUtility.GetSoulMarkRankText(m_Tier).Text);
		yield return new TooltipBrickTitle(title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		bool isLocked = m_MainDirection.HasValue && m_MainDirection.Value != m_SoulMarkDirection && m_Tier > 2;
		bool isRougeTraderSoulMark = UIUtility.GetCurrentSelectedUnit() == Game.Instance.Player.MainCharacter.Get();
		if (!m_MainDirection.HasValue || (m_MainDirection.Value == m_SoulMarkDirection && isRougeTraderSoulMark))
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.SoulMarkMayBeLocked, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		}
		else if (isLocked)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.SoulMarkIsLocked, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		}
		if (!isRougeTraderSoulMark)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.SoulMarkCompanion, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		}
		yield return new TooltipBrickText(m_BlueprintSoulMark.Description, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		if (!isLocked && isRougeTraderSoulMark)
		{
			yield return new TooltipBricksGroupStart(hasBackground: true, null, Color.white);
			yield return new TooltipBrickFeature(m_Feature);
			yield return new TooltipBricksGroupEnd();
		}
		int num = Mathf.Min(m_Tier, m_RankThresholds.Count - 1);
		IEnumerable<ITooltipBrick> slider = SoulMarkTooltipExtensions.GetSlider(m_RankThresholds.GetRange(0, num + 1), m_CurrentValue, m_RankThresholds[num]);
		foreach (ITooltipBrick item in slider)
		{
			yield return item;
		}
	}
}
