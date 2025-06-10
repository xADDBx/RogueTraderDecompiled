using System.Collections;
using System.Linq;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class TaskMovePartyToPoint : ClockworkRunnerTask
{
	private readonly Vector3 m_Point;

	public TaskMovePartyToPoint(ClockworkRunner runner, Vector3 point)
		: base(runner)
	{
		m_Point = point;
	}

	protected override IEnumerator Routine()
	{
		if (Game.Instance.Player.MainCharacter.Entity.IsInCombat)
		{
			yield return null;
		}
		if (Game.Instance.CurrentMode != GameModeType.Default)
		{
			yield return null;
		}
		UIAccess.SelectionManager.SelectAll();
		UnitCommandsRunner.MoveSelectedUnitsToPoint(m_Point);
		yield return null;
		UnitMoveTo command = null;
		while (command == null && TimeLeft > 1f)
		{
			command = Game.Instance.Player.PartyAndPets.Select((BaseUnitEntity u) => u.Commands.Current as UnitMoveTo).NotNull().FirstOrDefault();
			yield return null;
		}
		if (command == null)
		{
			PFLog.Clockwork.Error("TaskMovePartyToPoint failed: no UnitMoveTo command created");
			yield break;
		}
		while (!command.IsFinished || !Game.Instance.Player.PartyAndPets.All(ReachedDestination))
		{
			yield return 0.5f;
		}
		PFLog.Clockwork.Log($"End of TaskMovePartyToPoint. Command hash: ${command.GetHashCode()}");
	}

	private bool ReachedDestination(BaseUnitEntity arg)
	{
		if (Vector3.Distance(arg.Position, m_Point) < 4f)
		{
			return true;
		}
		if (!arg.View.MovementAgent.IsReallyMoving)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Party go to {m_Point}";
	}
}
