using JetBrains.Annotations;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitDirectInteract : UnitCommand<UnitDirectInteractParams>
{
	public InteractionPart Interaction => base.Params.Interaction;

	public override bool IsMoveUnit => false;

	public override bool IsUnitEnoughClose => true;

	public UnitDirectInteract([NotNull] UnitDirectInteractParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		InteractionPart interaction = Interaction;
		if (interaction == null || !interaction.CanInteract())
		{
			return ResultType.Fail;
		}
		interaction.Interact(base.Executor);
		return ResultType.Success;
	}

	public static UnitDirectInteractParams CreateCommandParams(InteractionPart interaction)
	{
		return new UnitDirectInteractParams(interaction)
		{
			IsSynchronized = true,
			NeedLoS = false
		};
	}
}
