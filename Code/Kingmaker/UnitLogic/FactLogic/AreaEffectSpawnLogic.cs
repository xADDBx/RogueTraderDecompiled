using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[TypeId("3e69f4b80be10d0489945af405b0d95f")]
public class AreaEffectSpawnLogic : BlueprintComponent
{
	public void HandleAreaEffectSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		OnAreaEffectSpawn(context, areaEffect);
	}

	protected virtual void OnAreaEffectSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
	}
}
