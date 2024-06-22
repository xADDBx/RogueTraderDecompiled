using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("ed0e545d0f3c91c42bba1e4dceb1b6e6")]
public class AbilityTargetInAreaEffectGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_Area;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = m_Area.Get();
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == blueprintAbilityAreaEffect && areaEffect.Contains(this.GetTargetPositionByType(Target)))
			{
				return 1;
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is in Area Effect";
	}
}
