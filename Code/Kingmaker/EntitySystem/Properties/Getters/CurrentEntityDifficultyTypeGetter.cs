using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("20908d2906855504abc36ab381c7ea6f")]
public class CurrentEntityDifficultyTypeGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "Difficulty Type";
	}

	protected override int GetBaseValue()
	{
		return (int)base.CurrentEntity.Blueprint.DifficultyType;
	}
}
