using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.MapObjects;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitAreaTransition : UnitGroupCommand<UnitAreaTransitionParams>
{
	public MechanicEntity TransitionEntity => base.Params.TransitionEntity;

	public AreaTransitionPart TransitionPart => base.Params.TransitionPart;

	public override bool AwaitMovementFinish => true;

	public override bool IsMoveUnit => false;

	public UnitAreaTransition([NotNull] UnitAreaTransitionParams @params)
		: base(@params)
	{
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (base.Executor.IsInCombat || !TransitionPart.Owner.IsInGame)
		{
			Interrupt();
		}
	}

	public override void TurnToTarget()
	{
		base.Executor.LookAt(TransitionEntity.Position);
	}

	protected override ResultType OnAction()
	{
		return ResultType.Success;
	}

	protected override GroupCommand RequestGroupCommand(Guid groupGuid, [NotNull] IEnumerable<EntityRef<BaseUnitEntity>> units)
	{
		return Game.Instance.GroupCommands.RequestAreaTransition(groupGuid, units, TransitionPart);
	}
}
