using System.Text;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

namespace Kingmaker.Designers.Mechanics.Prerequisites;

[AllowMultipleComponents]
[TypeId("0eda35167aa6fe047892f012931bc5a2")]
public class PrerequisiteObsoleteFullStatValue : Prerequisite_Obsolete
{
	public StatType Stat;

	public int Value;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return CheckUnit(unit);
	}

	public bool CheckUnit(BaseUnitEntity unit)
	{
		return GetFullStatValue(unit) >= Value;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = LocalizedTexts.Instance.Stats.GetText(Stat);
		stringBuilder.Append($"{text} {UIStrings.Instance.Tooltips.MoreThan}: <b>{Value}</b>");
		if (unit != null)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(string.Format(UIStrings.Instance.Tooltips.CurrentValue, GetFullStatValue(unit)));
		}
		return stringBuilder.ToString();
	}

	private int GetFullStatValue(BaseUnitEntity unit)
	{
		int num = 0;
		foreach (ModifiableValue.Modifier modifier in unit.Stats.GetStat(Stat).Modifiers)
		{
			if (modifier.ModDescriptor == ModifierDescriptor.CareerAdvancement)
			{
				num += modifier.ModValue;
			}
		}
		int num2 = unit.Stats.GetStat(Stat).PermanentValue + num;
		foreach (ReplaceStatForPrerequisites item in unit.Progression.Features.SelectComponents<ReplaceStatForPrerequisites>())
		{
			if (item.OldStat == Stat)
			{
				num2 = ReplaceStatForPrerequisites.ResultStat(item, num2, unit, fullValue: true);
			}
		}
		return num2;
	}
}
