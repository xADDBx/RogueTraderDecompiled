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
[TypeId("b1808a1edca51c944b47599c893e7528")]
public class PrerequisiteObsoleteNoClassLevel : Prerequisite_Obsolete
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharacterClass")]
	private BlueprintCharacterClassReference m_CharacterClass;

	public BlueprintCharacterClass CharacterClass => m_CharacterClass?.Get();

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return unit.Progression.GetClassLevel(CharacterClass) < 1;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{UIStrings.Instance.Tooltips.NoClass} {CharacterClass}");
		return stringBuilder.ToString();
	}
}
