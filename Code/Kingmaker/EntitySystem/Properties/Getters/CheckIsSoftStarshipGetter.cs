using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("723d037abc23ced4fb02c9cb299d1659")]
public class CheckIsSoftStarshipGetter : MechanicEntityPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is StarshipEntity starshipEntity) || !starshipEntity.Blueprint.IsSoftUnit)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "CurrentEntity is SOFT starship";
	}
}
