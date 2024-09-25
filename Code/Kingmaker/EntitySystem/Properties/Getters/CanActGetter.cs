using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("76a29dc4ed46a474b8fceb18690879bd")]
public class CanActGetter : MechanicEntityPropertyGetter
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!base.CurrentEntity.CanAct)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Can " + FormulaTargetScope.Current + " act";
	}
}
