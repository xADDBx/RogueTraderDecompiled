using System;
using System.Text;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[Obsolete]
[TypeId("c8a0fe809a127444e91b9d569dc8a7a8")]
public class PrerequisiteObsoleteCharacterLevel : Prerequisite_Obsolete
{
	public int Level;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return unit.Progression.CharacterLevel >= Level;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{UIStrings.Instance.Tooltips.CharacterLevel}: {Level}");
		if (unit != null)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(string.Format(UIStrings.Instance.Tooltips.CurrentValue, unit.Progression.CharacterLevel));
		}
		return stringBuilder.ToString();
	}
}
