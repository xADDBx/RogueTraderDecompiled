using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using UnityEngine;

namespace Kingmaker.Mechanics.Conditions;

[TypeId("608ef415ce4e4251bf721c0096297785")]
public class ContextConditionCasterIsInAreaEffect : ContextCondition
{
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_Area;

	protected override bool CheckCondition()
	{
		MechanicEntity mechanicEntity = base.Context?.MaybeCaster;
		BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = m_Area.Get();
		if (mechanicEntity == null || blueprintAbilityAreaEffect == null)
		{
			return false;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == blueprintAbilityAreaEffect && areaEffect.Contains(mechanicEntity.Position))
			{
				return true;
			}
		}
		return false;
	}

	protected override string GetConditionCaption()
	{
		return "Check if Caster is in Area Effect";
	}
}
