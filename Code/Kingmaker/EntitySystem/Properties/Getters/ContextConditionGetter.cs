using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b742461a99f449458cd0df048e50c1fe")]
public class ContextConditionGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public ConditionsChecker Conditions;

	protected override int GetBaseValue()
	{
		using (this.GetMechanicContext().GetDataScope((TargetWrapper)base.CurrentEntity))
		{
			return Conditions.Check() ? 1 : 0;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (Conditions.Conditions == null || Conditions.Conditions.Length < 1)
		{
			return "ContextConditionGetter";
		}
		if (Conditions.Conditions.Length < 2)
		{
			return Conditions.Conditions[0].GetCaption();
		}
		return $"ContextConditionGetter({Conditions.Conditions.Length})";
	}
}
