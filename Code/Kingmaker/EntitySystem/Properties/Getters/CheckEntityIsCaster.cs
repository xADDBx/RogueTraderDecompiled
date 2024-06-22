using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("9e76955a7d544abd89b5b968ee8f0957")]
public class CheckEntityIsCaster : PropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (this.GetFact()?.MaybeContext?.MaybeCaster != base.CurrentEntity)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Fact caster is the same as the checked entity";
	}
}
