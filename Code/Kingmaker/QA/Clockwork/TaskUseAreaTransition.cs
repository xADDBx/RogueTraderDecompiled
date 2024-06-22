using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class TaskUseAreaTransition : ClockworkRunnerTask
{
	private class ExitListener : IAreaHandler, ISubscriber
	{
		public bool Finished { get; private set; }

		public void OnAreaBeginUnloading()
		{
		}

		public void OnAreaDidLoad()
		{
			Finished = true;
		}
	}

	private readonly AreaTransitionPart m_Transition;

	public TaskUseAreaTransition(ClockworkRunner runner, AreaTransitionPart transition)
		: base(runner)
	{
		m_Transition = transition;
	}

	protected override IEnumerator Routine()
	{
		EntityViewBase unityObject = (EntityViewBase)m_Transition.View;
		Transform transform = ObjectExtensions.Or(unityObject, null)?.ViewTransform;
		if (transform == null)
		{
			yield break;
		}
		yield return new TaskMovePartyToPoint(Runner, transform.position);
		Game.Instance.GroupCommands.ClearDuplicates(typeof(AreaTransitionGroupCommand));
		UIAccess.SelectionManager.SelectAll();
		Guid groupGuid = Guid.NewGuid();
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(transform.position, ClickGroundHandler.GetDefaultDirection(transform.position), Game.Instance.IsControllerGamepad, preview: false, BlueprintRoot.Instance.Formations.MinSpaceFactor, null, delegate(BaseUnitEntity unit, MoveCommandSettings settings)
		{
			ForcedPath forcedPath = PathfindingService.Instance.FindPathRT_Blocking(unit.MovementAgent, ((MechanicEntity)m_Transition.Owner).Position, 2f);
			if (forcedPath.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (unit.IsMovementLockedByGameModeOrCombat())
			{
				PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
			}
			else
			{
				UnitMoveToParams cmdParams = new UnitMoveToParams(forcedPath, (MechanicEntity)m_Transition.Owner, 2f)
				{
					IsSynchronized = true,
					CanBeAccelerated = true
				};
				unit.Commands.Run(cmdParams);
				List<EntityRef<BaseUnitEntity>> units = ((IEnumerable<BaseUnitEntity>)Game.Instance.SelectionCharacter.SelectedUnits).Select((Func<BaseUnitEntity, EntityRef<BaseUnitEntity>>)((BaseUnitEntity elem) => elem)).ToList();
				UnitAreaTransitionParams cmdParams2 = new UnitAreaTransitionParams(groupGuid, units, settings.Destination, m_Transition);
				unit.Commands.Run(cmdParams2);
			}
		});
		PlayData.AreaEntry ad = Runner.Data.GetAreaData(m_Transition.AreaEnterPoint.Area, m_Transition.AreaEnterPoint.AreaPart);
		PlayData.AreaEntry areaData = Runner.Data.GetAreaData(Game.Instance.CurrentlyLoadedArea, Game.Instance.CurrentlyLoadedAreaPart);
		if (!ad.Visited || ad.Depth > areaData.Depth)
		{
			ad.Depth = areaData.Depth + 1;
		}
		if (m_Transition.AreaEnterPoint.Area == Game.Instance.CurrentlyLoadedArea)
		{
			yield return 5f;
			Runner.Data.CountLocalTransitionUse(m_Transition.AreaEnterPoint.AssetGuid);
			yield break;
		}
		ExitListener waiter = new ExitListener();
		EventBus.Subscribe(waiter);
		yield return null;
		while (!waiter.Finished)
		{
			yield return null;
		}
		EventBus.Unsubscribe(waiter);
		ad.Visited = true;
		Runner.Data.UnreachableObjects.Clear();
	}

	public override string ToString()
	{
		return "Use area transition " + m_Transition.Blueprint.NameSafe() + " to " + m_Transition.AreaEnterPoint.NameSafe();
	}
}
