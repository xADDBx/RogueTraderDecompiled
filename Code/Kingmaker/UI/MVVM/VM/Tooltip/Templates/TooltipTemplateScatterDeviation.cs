using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateScatterDeviation : TooltipBaseTemplate
{
	private string m_Header;

	private int m_Min;

	private int m_Max;

	private int m_Value;

	private RuleRollScatterShotHitDirection.Ray m_ShotDeviationType;

	private Color m_Color;

	public TooltipTemplateScatterDeviation(string header, int min, int max, int value, RuleRollScatterShotHitDirection.Ray shotDeviationType, Color color)
	{
		m_Header = header;
		m_Min = min;
		m_Max = max;
		m_Value = value;
		m_ShotDeviationType = shotDeviationType;
		m_Color = color;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Header);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickShotDirection(m_Min, m_Max, m_Value);
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		string text = AddColor(UIStrings.Instance.CombatLog.CentralShotDirection.Text, m_ShotDeviationType == RuleRollScatterShotHitDirection.Ray.Main);
		TooltipTemplateScatterDeviation tooltipTemplateScatterDeviation = this;
		string text2 = UIStrings.Instance.CombatLog.SlightDeviationShotDirection.Text;
		RuleRollScatterShotHitDirection.Ray shotDeviationType = m_ShotDeviationType;
		string slightDeviationText = tooltipTemplateScatterDeviation.AddColor(text2, shotDeviationType == RuleRollScatterShotHitDirection.Ray.LeftClose || shotDeviationType == RuleRollScatterShotHitDirection.Ray.RightClose);
		TooltipTemplateScatterDeviation tooltipTemplateScatterDeviation2 = this;
		string text3 = UIStrings.Instance.CombatLog.StrongDeviationShotDirection.Text;
		shotDeviationType = m_ShotDeviationType;
		string strongDeviationText = tooltipTemplateScatterDeviation2.AddColor(text3, shotDeviationType == RuleRollScatterShotHitDirection.Ray.LeftFar || shotDeviationType == RuleRollScatterShotHitDirection.Ray.RightFar);
		yield return new TooltipBrickTextValue("<b>" + text + "</b>", "0—" + m_Min);
		yield return new TooltipBrickTextValue("<b>" + slightDeviationText + "</b>", m_Min + 1 + "—" + m_Max);
		yield return new TooltipBrickTextValue("<b>" + strongDeviationText + "</b>", m_Max + 1 + "—100");
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		yield return new TooltipBrickText(UIStrings.Instance.CombatLog.DeviationDescription.Text);
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtility.GetGlossaryEntry("BurstAttacks");
		yield return new TooltipBrickText(glossaryEntry.GetDescription());
	}

	private string AddColor(string text, bool value)
	{
		if (!value)
		{
			return text;
		}
		return "<color=#" + ColorUtility.ToHtmlStringRGB(m_Color) + ">" + text + "</color>";
	}
}
