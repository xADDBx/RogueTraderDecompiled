using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Buffs;

public interface IBuff
{
	[NotNull]
	MechanicEntity Caster { get; }

	[NotNull]
	TargetWrapper Target { get; }

	[CanBeNull]
	BlueprintAbilityFXSettings FXSettings { get; }
}
