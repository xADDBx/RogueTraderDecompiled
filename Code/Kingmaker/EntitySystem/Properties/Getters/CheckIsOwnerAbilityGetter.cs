using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f47d622e2c7f4708b8eb5d723af1e2e5")]
public class CheckIsOwnerAbilityGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (this.GetAbility().Blueprint == base.Owner)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability is from Owner";
	}
}
