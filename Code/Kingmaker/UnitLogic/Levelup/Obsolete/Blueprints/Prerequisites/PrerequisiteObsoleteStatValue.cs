using System.Text;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowMultipleComponents]
[TypeId("04406431439974e489cc8fdea779cf46")]
public class PrerequisiteObsoleteStatValue : Prerequisite_Obsolete
{
	public StatType Stat;

	public int Value;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return CheckUnit(unit);
	}

	public bool CheckUnit(BaseUnitEntity unit)
	{
		return GetStatValue(unit) >= Value;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = LocalizedTexts.Instance.Stats.GetText(Stat);
		stringBuilder.Append($"{text}: <b>{Value}</b>");
		if (unit != null)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(string.Format(UIStrings.Instance.Tooltips.CurrentValue, GetStatValue(unit)));
		}
		return stringBuilder.ToString();
	}

	private int GetStatValue(BaseUnitEntity unit)
	{
		int num = (Stat.IsSkill() ? unit.Stats.GetStat(Stat).BaseValue : unit.Stats.GetStat(Stat).PermanentValue);
		foreach (ReplaceStatForPrerequisites item in unit.Progression.Features.SelectComponents<ReplaceStatForPrerequisites>())
		{
			if (item.OldStat == Stat)
			{
				num = ReplaceStatForPrerequisites.ResultStat(item, num, unit, fullValue: false);
			}
		}
		return num;
	}
}
