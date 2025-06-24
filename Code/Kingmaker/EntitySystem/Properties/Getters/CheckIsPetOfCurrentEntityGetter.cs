using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("655b7056801c4be585dd2d12b327b6c3")]
public class CheckIsPetOfCurrentEntityGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target = PropertyTargetType.CurrentTarget;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current entity is master of " + Target.Colorized();
	}

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is BaseUnitEntity { IsPet: not false } baseUnitEntity) || baseUnitEntity.Master != base.CurrentEntity)
		{
			return 0;
		}
		return 1;
	}
}
