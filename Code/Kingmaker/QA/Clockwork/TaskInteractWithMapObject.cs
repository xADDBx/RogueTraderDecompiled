using System.Collections;
using System.Linq;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class TaskInteractWithMapObject : ClockworkRunnerTask
{
	private MapObjectEntity m_MapObject;

	public TaskInteractWithMapObject(ClockworkRunner runner, MapObjectEntity mapObject = null)
		: base(runner)
	{
		m_MapObject = mapObject;
	}

	protected override IEnumerator Routine()
	{
		if (m_MapObject == null)
		{
			m_MapObject = Game.Instance.State.MapObjects.Where((MapObjectEntity o) => o.Interactions.Any()).Random(PFStatefulRandom.Qa);
		}
		if (m_MapObject == null)
		{
			yield break;
		}
		yield return new TaskMovePartyToPoint(Runner, m_MapObject.View.ViewTransform.position);
		if (!m_MapObject.IsAwarenessCheckPassed && Game.Instance.Player.Party.Any((BaseUnitEntity c) => c.Vision.HasLOS(m_MapObject.View)))
		{
			Runner?.MarkAsInteracted(m_MapObject.UniqueId + "0");
			PFLog.Clockwork.Log($"Failed preception on {m_MapObject}");
			yield break;
		}
		if (!ClickMapObjectHandler.Interact(m_MapObject.View.gameObject, Game.Instance.Player.Party, forceOvertipInteractions: true))
		{
			PFLog.Clockwork.Error($"Failed interaction with {m_MapObject}");
			yield break;
		}
		UnitInteractWithObject command = null;
		while (command == null && TimeLeft > 1f)
		{
			command = Game.Instance.Player.Party.Select((BaseUnitEntity u) => u.Commands.Current as UnitInteractWithObject).NotNull().FirstOrDefault();
			yield return null;
		}
		if (command == null)
		{
			PFLog.Clockwork.Error("Failed to interact with " + m_MapObject.View.name + " " + m_MapObject.UniqueId + ": no command created");
			Runner?.Data.MarkUnreachable(m_MapObject.UniqueId);
			yield break;
		}
		while (!command.IsFinished)
		{
			if (command.Executor.View.MovementAgent.PathFailed || ReachedButCantSee())
			{
				PFLog.Clockwork.Error("Failed to reach map obj " + m_MapObject.View.name + " " + m_MapObject.UniqueId);
				Runner?.Data.MarkUnreachable(m_MapObject.UniqueId);
				yield break;
			}
			yield return null;
		}
		yield return null;
		if (command.Result != AbstractUnitCommand.ResultType.Success)
		{
			PFLog.Clockwork.Error("Failed to interact with map obj " + m_MapObject.View.name + " " + m_MapObject.UniqueId + " - interrupted?");
			yield break;
		}
		yield return null;
		if (command?.Interaction is InteractionSkillCheckPart { CheckPassed: false } && Clockwork.Instance.Scenario.IsSkillCheckRetriable(m_MapObject))
		{
			PFLog.Clockwork.Log("Skill check is not passed, try again");
			yield break;
		}
		Runner?.MarkAsInteracted(m_MapObject.UniqueId + m_MapObject.Interactions.IndexOf(command?.Interaction));
		yield return 0.5f;
		Runner?.MaybeCollectLoot();
		yield return null;
		InteractionPart interactionPart = command?.Interaction;
		if (interactionPart is InteractionDoorPart door)
		{
			yield return 2f;
			if (door.IsOpen)
			{
				PFLog.Clockwork.Log($"Opened door {m_MapObject}. Clear unreachables.");
				Runner?.Data.UnreachableObjects.Clear();
			}
		}
	}

	private bool ReachedButCantSee()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!item.View.MovementAgent.IsReallyMoving && Vector3.Distance(item.Position, m_MapObject.View.ViewTransform.position) < 4f && (m_MapObject.IsInFogOfWar || !m_MapObject.IsVisibleForPlayer || !m_MapObject.IsRevealed))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		if (!m_MapObject.View)
		{
			return "Interact with: [destroyed] " + m_MapObject.UniqueId;
		}
		return "Interact with: " + m_MapObject.View.name + " " + m_MapObject.UniqueId;
	}
}
