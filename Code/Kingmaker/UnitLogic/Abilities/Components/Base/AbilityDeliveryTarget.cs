using JetBrains.Annotations;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public class AbilityDeliveryTarget
{
	[NotNull]
	public readonly TargetWrapper Target;

	[CanBeNull]
	public readonly AbilityDeliveryTarget OriginalTarget;

	[CanBeNull]
	public RulePerformAttack AttackRule { get; set; }

	[CanBeNull]
	public Projectile Projectile { get; set; }

	public AbilityDeliveryTarget([NotNull] TargetWrapper target, [CanBeNull] AbilityDeliveryTarget originalTarget = null)
	{
		Target = target;
		OriginalTarget = originalTarget;
	}

	public AbilityDeliveryTarget([NotNull] MechanicEntity target, [CanBeNull] AbilityDeliveryTarget originalTarget = null)
	{
		Target = target;
		OriginalTarget = originalTarget;
	}
}
