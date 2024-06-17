using JetBrains.Annotations;

namespace Kingmaker.UnitLogic.Commands;

public sealed class PlayerUseAbility : UnitUseAbility
{
	public PlayerUseAbility([NotNull] PlayerUseAbilityParams @params)
		: base(@params)
	{
		@params.Prepare();
	}
}
