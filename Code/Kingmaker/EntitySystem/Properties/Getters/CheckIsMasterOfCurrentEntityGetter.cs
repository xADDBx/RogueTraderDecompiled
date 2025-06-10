using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4b195f4488084c3e9940cc7e4bc1cc61")]
public class CheckIsMasterOfCurrentEntityGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ICurrentTargetEntity, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current entity is pet of Target";
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity { IsPet: not false } baseUnitEntity) || baseUnitEntity.Master != this.GetCurrentTarget())
		{
			return 0;
		}
		return 1;
	}
}
