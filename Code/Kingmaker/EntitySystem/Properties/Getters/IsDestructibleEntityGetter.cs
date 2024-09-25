using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ca45e2fbf9ca3c6478bd4533289cf12d")]
public class IsDestructibleEntityGetter : MechanicEntityPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is DestructibleEntity))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is destructible entity";
	}
}
