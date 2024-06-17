using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitOverwatchAttack : UnitUseAbility
{
	public UnitOverwatchAttack([NotNull] UnitOverwatchAttackParams @params)
		: base(@params)
	{
	}
}
