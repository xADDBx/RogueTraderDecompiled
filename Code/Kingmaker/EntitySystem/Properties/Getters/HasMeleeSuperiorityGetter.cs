using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("90b7ab406b294daf8b7db495efc383d4")]
public class HasMeleeSuperiorityGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		AbilityData ability = base.PropertyContext.Ability;
		if (ability == null)
		{
			return 0;
		}
		MechanicEntity currentEntity = base.CurrentEntity;
		if (!(this.GetTargetByType(Target) is MechanicEntity mechanicEntity) || mechanicEntity == currentEntity)
		{
			return 0;
		}
		if (ability.Caster != currentEntity && ability.Caster != mechanicEntity)
		{
			return 0;
		}
		bool num = ability.Caster == currentEntity;
		MechanicEntity target = ((!num) ? currentEntity : mechanicEntity);
		RuleCalculateSuperiority ruleCalculateSuperiority = Rulebook.Trigger(new RuleCalculateSuperiority(ability.Caster, target, ability));
		if (!num)
		{
			if (ruleCalculateSuperiority.ResultRawSuperiorityNumber >= 0)
			{
				return 0;
			}
			return 1;
		}
		if (ruleCalculateSuperiority.ResultRawSuperiorityNumber <= 0)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if has melee superiority for " + Target.Colorized();
	}
}
