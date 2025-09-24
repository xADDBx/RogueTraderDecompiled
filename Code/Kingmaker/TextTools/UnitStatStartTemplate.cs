using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.TextTools;

public class UnitStatStartTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 2;

	public override int Balance => 2;

	private GlossaryColors Colors => BlueprintRoot.Instance.UIConfig.PaperGlossaryColors;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		string text = ((parameters.Count > 0) ? parameters[0] : "");
		bool flag = ((parameters.Count > 1) ? parameters[1] : "") == "bonus";
		bool emptyLink = false;
		if (!UIUtility.CheckLinkKeyHasContent(text))
		{
			text = EntityLink.Type.Empty.ToString();
			emptyLink = true;
		}
		string color = GetColor(emptyLink);
		StatType? statType = UIUtility.TryGetStatType(text);
		if (!statType.HasValue)
		{
			return "<b><color=" + color + ">" + text + "</color></b>";
		}
		UnitEntity unitEntity = (GameLogContext.InScope ? (GameLogContext.UnitEntity.Value as UnitEntity) : null) ?? (UIUtility.GetCurrentSelectedUnit() as UnitEntity);
		if (unitEntity == null)
		{
			string text2 = (flag ? LocalizedTexts.Instance.Stats.GetBonusText(statType.Value) : LocalizedTexts.Instance.Stats.GetShortText(statType.Value));
			return "<b><color=" + color + "><link=\"" + EntityLink.Type.Encyclopedia.GetTag() + ":" + text + "\">" + text2 + "</link></color></b>";
		}
		string text3 = ((!flag) ? ((unitEntity.Stats.GetStatOptional(statType.Value) != null) ? Math.Abs(unitEntity.Stats.GetStatOptional(statType.Value).ModifiedValue).ToString() : "-") : Math.Abs(unitEntity.Stats.GetAttributeOptional(statType.Value)?.Bonus ?? ((int)unitEntity.Stats.GetStatOptional(statType.Value))).ToString());
		return "<b><color=" + color + "><link=\"" + EntityLink.Type.UnitStat.GetTag() + ":" + text + ":" + unitEntity.UniqueId + "\">" + text3 + "</link></color></b>";
	}

	private string GetColor(bool emptyLink)
	{
		if (!emptyLink)
		{
			return Colors.GlossaryMechanicsHTML;
		}
		return Colors.GlossaryEmptyHTML;
	}
}
