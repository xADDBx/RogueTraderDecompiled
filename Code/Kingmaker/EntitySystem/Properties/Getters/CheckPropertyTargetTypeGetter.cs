using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("976625711af845a4fa4b9c6cd63d5193")]
public class CheckPropertyTargetTypeGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check target [" + Target.Colorized() + "] equals " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity != this.GetTargetByType(Target))
		{
			return 0;
		}
		return 1;
	}
}
