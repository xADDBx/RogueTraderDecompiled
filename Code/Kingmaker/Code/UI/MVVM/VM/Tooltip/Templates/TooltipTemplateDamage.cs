using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateDamage : TooltipBaseTemplate
{
	private readonly List<UnitDescription.UIDamageInfo> m_DamageInfos;

	private readonly string m_DamageTitle;

	private readonly string m_DamageDesc;

	private readonly Sprite m_DamageIcon;

	private string m_DamageValue;

	private List<string> m_Values;

	private List<string> m_Symbols;

	private List<string> m_Names;

	public TooltipTemplateDamage(List<UnitDescription.UIDamageInfo> damageInfos)
	{
		if (damageInfos != null && damageInfos.Count > 0)
		{
			m_DamageInfos = damageInfos;
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtility.GetGlossaryEntry("Damage");
			m_DamageTitle = glossaryEntry?.Title;
			m_DamageDesc = glossaryEntry?.GetDescription();
			m_DamageIcon = BlueprintRoot.Instance.UIConfig.UIIcons.Damage;
			SetDamageValue();
			SetDamageFormula();
		}
	}

	private void SetDamageValue()
	{
		int num = 0;
		int num2 = 0;
		foreach (UnitDescription.UIDamageInfo damageInfo in m_DamageInfos)
		{
			num += UIUtilityItem.MinDiceValue(damageInfo);
			num2 += UIUtilityItem.MaxDiceValue(damageInfo);
		}
		m_DamageValue = $"{num}-{num2}";
	}

	private void SetDamageFormula()
	{
		m_Values = new List<string>();
		m_Symbols = new List<string>();
		m_Names = new List<string>();
		foreach (UnitDescription.UIDamageInfo damageInfo in m_DamageInfos)
		{
			string text = UIUtilityItem.GetDamageDiceText(damageInfo.Damage.Dice, damageInfo.Damage.Bonus);
			if (!object.Equals(damageInfo, m_DamageInfos.First()) && !string.IsNullOrEmpty(text))
			{
				text = "+" + text;
			}
			string damageSymbols = UIUtilityTexts.GetDamageSymbols(damageInfo.Damage.TypeDescription);
			string damageNames = UIUtilityTexts.GetDamageNames(damageInfo.Damage.TypeDescription);
			m_Values.Add(text);
			m_Symbols.Add(damageSymbols);
			m_Names.Add(damageNames);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_DamageTitle);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickIconValueStat(m_DamageTitle, m_DamageValue, m_DamageIcon);
		for (int i = 0; i < m_Values.Count; i++)
		{
			yield return new TooltipBrickValueStatFormula(m_Values[i], m_Symbols[i], m_Names[i]);
		}
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
		yield return new TooltipBrickText(m_DamageDesc, TooltipTextType.Paragraph);
	}
}
