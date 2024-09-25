using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a4f4f63f879f4d0c8b9a69c1d8d50bc4")]
public class EntityIsAreaSourceGetter : PropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "CurrentTarget is AreaEffectEntity and " + FormulaTargetScope.Current + " is it's source";
	}

	protected override int GetBaseValue()
	{
		if (!(base.PropertyContext.CurrentTarget is AreaEffectEntity { SourceFact: { Entity: var entity } }))
		{
			return 0;
		}
		if (entity == null)
		{
			return 0;
		}
		if (entity != base.PropertyContext.CurrentEntity)
		{
			return 0;
		}
		return 1;
	}
}
