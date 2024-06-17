using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[Obsolete]
[AllowMultipleComponents]
[TypeId("fb876f3084f01754db3a4acbbb328a89")]
public class PrerequisiteObsoleteClassLevel : Prerequisite_Obsolete
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharacterClass")]
	private BlueprintCharacterClassReference m_CharacterClass;

	public int Level = 1;

	public BlueprintCharacterClass CharacterClass => m_CharacterClass?.Get();

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return GetClassLevel(unit) >= Level;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{CharacterClass.Name} {UIStrings.Instance.Tooltips.Level}: {Level}");
		if (unit != null)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(string.Format(UIStrings.Instance.Tooltips.CurrentValue, GetClassLevel(unit)));
		}
		return stringBuilder.ToString();
	}

	private int GetClassLevel(BaseUnitEntity unit)
	{
		return unit.Progression.GetClassLevel(CharacterClass);
	}
}
