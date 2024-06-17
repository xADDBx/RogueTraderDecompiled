using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.AI.Blueprints;

[TypeId("a89e5e0465016b448b297c4d108f9add")]
public abstract class BlueprintBrainBase : BlueprintScriptableObject
{
	public virtual List<TargetInfo> GetHatedTargets(PropertyContext context, List<TargetInfo> enemies)
	{
		return new List<TargetInfo>();
	}

	public virtual AbilitySettings GetCustomAbilitySettings(BlueprintAbility ability)
	{
		return null;
	}

	public virtual int GetAbilityValue(BlueprintAbility ability, PropertyContext context)
	{
		return 0;
	}

	public virtual List<MechanicEntity> GetPriorityDestroyTarget()
	{
		return null;
	}
}
