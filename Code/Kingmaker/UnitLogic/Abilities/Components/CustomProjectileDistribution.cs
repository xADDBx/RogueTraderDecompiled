using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("5416f6e35cd84885ac073f4cc6b1d70a")]
public abstract class CustomProjectileDistribution : BlueprintComponent
{
	public abstract List<Projectile> Launch(AbilityExecutionContext context, BlueprintProjectile blueprintProjectile, IEnumerable<MechanicEntity> targets);
}
