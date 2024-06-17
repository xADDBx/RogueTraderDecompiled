using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitInteractWithUnit : UnitCommand<UnitInteractWithUnitParams>
{
	[JsonProperty]
	private IUnitInteraction m_Interaction;

	private new AbstractUnitEntity TargetUnit => base.Params.TargetUnit.Entity;

	public override bool ShouldBeInterrupted => base.Executor.IsInCombat;

	public override bool IsMoveUnit => false;

	public UnitInteractWithUnit([NotNull] UnitInteractWithUnitParams @params)
		: base(@params)
	{
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		m_Interaction = TargetUnit?.SelectClickInteraction(base.Executor);
		if (m_Interaction != null)
		{
			base.Params.ApproachRadius = m_Interaction.Distance;
		}
	}

	protected override ResultType OnAction()
	{
		if (m_Interaction == null || base.Executor.IsInCombat)
		{
			return ResultType.Fail;
		}
		return m_Interaction.Interact(base.Executor, TargetUnit);
	}
}
