using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d5a470fc5f394ffc9903ddf373af8e1b")]
public class PetOwnerLevelGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " owner Level";
	}

	protected override int GetBaseValue()
	{
		return (((BaseUnitEntity)base.CurrentEntity).Master?.GetProgressionOptional()?.CharacterLevel).GetValueOrDefault();
	}
}
