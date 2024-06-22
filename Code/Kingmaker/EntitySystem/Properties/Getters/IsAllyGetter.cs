using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b5a7d9bbf95591b49b2985d414d2e360")]
public class IsAllyGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is BaseUnitEntity entity))
		{
			return 0;
		}
		if (!base.CurrentEntity.IsAlly(entity))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " ally to " + Target.Colorized();
	}
}
