using System.Collections;
using System.Linq;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.QA.Clockwork;

public class TaskInteractWithUnit : ClockworkRunnerTask
{
	private BaseUnitEntity m_Unit;

	public TaskInteractWithUnit(ClockworkRunner runner, BaseUnitEntity unit = null)
		: base(runner)
	{
		m_Unit = unit;
	}

	protected override IEnumerator Routine()
	{
		m_Unit = m_Unit ?? Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity o) => o.GetOptional<UnitPartInteractions>() != null).Random(PFStatefulRandom.Qa);
		if (m_Unit == null)
		{
			yield break;
		}
		if (m_Unit.View == null)
		{
			Clockwork.Instance.Reporter.HandleWarning($"Failed interaction with {m_Unit} - no view!");
			yield break;
		}
		yield return new TaskMovePartyToPoint(Runner, m_Unit.Position);
		if (!new ClickUnitHandler().OnClick(m_Unit.View.gameObject, m_Unit.Position, 0))
		{
			PFLog.Clockwork.Error($"Failed interaction with {m_Unit}");
			yield break;
		}
		UnitInteractWithUnit command = Game.Instance.Player.Party.Select((BaseUnitEntity u) => u.Commands.Current as UnitInteractWithUnit).NotNull().FirstOrDefault();
		UnitLootUnit loot = Game.Instance.Player.Party.Select((BaseUnitEntity u) => u.Commands.Current as UnitLootUnit).NotNull().FirstOrDefault();
		while (command != null && !command.IsFinished)
		{
			if (command != null && command.Executor.View.MovementAgent.WantsToMove && !command.Executor.View.MovementAgent.IsReallyMoving)
			{
				PFLog.Clockwork.Error("Failed to reach unit " + m_Unit.CharacterName + " " + m_Unit.UniqueId);
				Runner.Data.MarkUnreachable(m_Unit.UniqueId);
				yield break;
			}
			yield return null;
		}
		while (loot != null && !loot.IsFinished)
		{
			if (loot != null && loot.Executor.View.MovementAgent.WantsToMove && !loot.Executor.View.MovementAgent.IsReallyMoving)
			{
				PFLog.Clockwork.Error("Failed to reach unit " + m_Unit.CharacterName + " " + m_Unit.UniqueId);
				Runner.Data.MarkUnreachable(m_Unit.UniqueId);
				yield break;
			}
			yield return null;
		}
		if (command != null && command.Result != AbstractUnitCommand.ResultType.Success)
		{
			PFLog.Clockwork.Error($"Failed to interact with unit {m_Unit} - interruped?");
			yield break;
		}
		yield return 0.5f;
		Runner.MaybeCollectLoot();
	}

	public override string ToString()
	{
		return $"Interact with: {m_Unit}";
	}
}
