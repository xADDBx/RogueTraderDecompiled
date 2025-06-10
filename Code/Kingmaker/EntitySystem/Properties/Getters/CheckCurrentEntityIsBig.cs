using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("435e2b384db141f3ad32ace00eebd99e")]
public class CheckCurrentEntityIsBig : PropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (!base.CurrentEntity.Size.IsBigAndEvenUnit())
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Entity is big and even";
	}
}
