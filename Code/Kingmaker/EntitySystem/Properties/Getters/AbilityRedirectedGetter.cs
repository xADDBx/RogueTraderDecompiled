using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("665495023402478e8711188968b9a514")]
public class AbilityRedirectedGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if ((object)ability == null || !ability.IsRedirected)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is redirecetd";
	}
}
