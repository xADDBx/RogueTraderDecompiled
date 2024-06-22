using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2823df2bc8754a409cb9d0340ff6a379")]
public class IsBaseUnitEntityGetter : MechanicEntityPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is base unit entity";
	}
}
