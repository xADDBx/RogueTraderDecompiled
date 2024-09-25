using JetBrains.Annotations;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitActivateAbility : UnitCommand<UnitActivateAbilityParams>
{
	public ActivatableAbility Ability => base.Params.Ability;

	public override bool IsMoveUnit => false;

	public UnitActivateAbility([NotNull] UnitActivateAbilityParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		return ResultType.Success;
	}
}
