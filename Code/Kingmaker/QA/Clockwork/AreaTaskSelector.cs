using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.SriptZones;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class AreaTaskSelector
{
	private const int PriorityDeadLoot = 18;

	private const int PriorityDialog = 18;

	private const int PriorityLoot = 18;

	private const int PriorityMapObject = 18;

	private const int PriorityTrap = 18;

	private const int PriorityDoor = 17;

	private const int PriorityLocalTransition = 8;

	private const int PriorityAttack = 7;

	private const int PriorityAreaTransition = 6;

	private const int PriorityExit = 5;

	private const int PriorityDialogRepeat = 3;

	private const int PriorityScriptZone = 3;

	private ClockworkRunner m_Runner;

	public bool IsRandom;

	public Vector3 SafePoint;

	private bool m_TriedToResolveStuck;

	private NNConstraint m_NNConstraint = new NNConstraint();

	public IEnumerable<TargetAndPriority> PrioritizedAreaTargets { get; private set; } = new List<TargetAndPriority>();


	public AreaTaskSelector()
	{
		m_NNConstraint.Reset();
		m_NNConstraint.graphMask = 1;
	}

	public ClockworkRunnerTask SelectNewTask(ClockworkRunner runner, bool checkNavMesh = true)
	{
		m_Runner = runner;
		BaseUnitEntity unitForLevelUp = GetUnitForLevelUp();
		if (unitForLevelUp != null)
		{
			return new TaskLevelup(runner, unitForLevelUp);
		}
		if (Game.Instance.Player.Party.Any((BaseUnitEntity u) => u.Health.Damage > u.Health.HitPoints.ModifiedValue / 2 || u.LifeState.IsDead))
		{
			return new TaskHeal(runner);
		}
		IEnumerable<BaseUnitEntity> source = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity u) => !m_Runner.AIConfig.IsExcludedUnit(u.Blueprint));
		List<TargetAndPriority> source2 = (from i in Enumerable.Concat(second: from o in Game.Instance.State.MapObjects
				where Clockwork.Instance.Scenario.CanInterractMapObject(o)
				where o.View != null
				select new TargetAndPriority
				{
					AreaTarget = o,
					Priority = GetInteractionPriority(o),
					Name = (o.ViewSettings?.Blueprint.NameSafe() ?? o.View.name),
					Distance = GetDistanceToObject(m_Runner, o, checkNavMesh)
				}, first: from u in source
				where u.View != null
				select new TargetAndPriority
				{
					AreaTarget = u,
					Priority = GetInteractionPriority(u),
					Name = u.CharacterName,
					Distance = GetDistanceToObject(m_Runner, u)
				})
			where i.Priority > 0
			select i).ToList();
		PrioritizedAreaTargets = (from i in source2
			where i.Distance >= 0f
			orderby i.Priority descending, i.Distance
			select i).ToList();
		foreach (TargetAndPriority prioritizedAreaTarget in PrioritizedAreaTargets)
		{
			ClockworkRunnerTask clockworkRunnerTask = CreateInteractionTask(m_Runner, prioritizedAreaTarget.AreaTarget);
			if (clockworkRunnerTask != null && !clockworkRunnerTask.TooManyAttempts())
			{
				prioritizedAreaTarget.IsCurrent = true;
				m_TriedToResolveStuck = false;
				clockworkRunnerTask.TimeLeft += Mathf.Min(prioritizedAreaTarget.Distance / 10f, 100f);
				return clockworkRunnerTask;
			}
		}
		if (source2.Any() && !m_TriedToResolveStuck)
		{
			m_TriedToResolveStuck = true;
			return new TaskMovePartyToPoint(runner, AstarPath.active.GetNearest(SafePoint).position)
			{
				TimeLeft = 2f
			};
		}
		Clockwork.Instance.Reporter.HandleError("No available actions in exploration mode");
		return new TaskDummy(runner);
	}

	[Obsolete]
	private BaseUnitEntity GetUnitForLevelUp()
	{
		return null;
	}

	private static bool IsLocalTransition(TargetAndPriority obj)
	{
		AreaTransition areaTransition = (obj.AreaTarget.View as MapObjectView)?.GetComponent<AreaTransition>();
		if ((bool)areaTransition)
		{
			return areaTransition.Settings.AreaEnterPoint.Area == Game.Instance.CurrentlyLoadedArea;
		}
		return false;
	}

	public static ClockworkRunnerTask CreateInteractionTask(ClockworkRunner runner, Entity bestObject)
	{
		BaseUnitEntity baseUnitEntity = bestObject as BaseUnitEntity;
		MapObjectEntity mapObjectEntity = bestObject as MapObjectEntity;
		if (baseUnitEntity != null)
		{
			return new TaskInteractWithUnit(runner, baseUnitEntity);
		}
		if (mapObjectEntity != null)
		{
			AreaTransitionPart optional = mapObjectEntity.View.Data.GetOptional<AreaTransitionPart>();
			if ((bool)optional)
			{
				return new TaskUseAreaTransition(runner, optional);
			}
			if ((bool)(mapObjectEntity.View as ScriptZone))
			{
				return new TaskInteractWithScriptZone(runner, bestObject);
			}
			return new TaskInteractWithMapObject(runner, mapObjectEntity);
		}
		PFLog.Clockwork.Error("Bad interaction object");
		return null;
	}

	private ClockworkRunnerTask TryChangeArea(ClockworkRunner runner)
	{
		AreaTransitionPart areaTransitionPart = (from i in UnityEngine.Object.FindObjectsOfType<MapObjectView>()
			select i.Data?.GetOptional<AreaTransitionPart>() into i
			where i != null
			select i).ToList().FirstOrDefault((AreaTransitionPart t) => t.AreaEnterPoint.Area != Game.Instance.CurrentlyLoadedArea && !runner.Data.UnreachableObjects.Contains(t.View.GameObjectName));
		if ((bool)areaTransitionPart)
		{
			return new TaskUseAreaTransition(runner, areaTransitionPart);
		}
		Clockwork.Instance.Reporter.HandleError("No available actions in exploration mode");
		return new TaskDummy(runner);
	}

	private int GetInteractionPriority(BaseUnitEntity unit)
	{
		if (Game.Instance.Player.ActiveCompanions.Contains(unit) || Game.Instance.Player.ActiveCompanions.Contains(unit.Master))
		{
			return 0;
		}
		if (m_Runner.Data.UnreachableObjects.Contains(unit.UniqueId))
		{
			return 0;
		}
		if (unit.IsDeadAndHasLoot)
		{
			return 18;
		}
		int result = 0;
		if (unit.Faction.IsPlayerEnemy && !unit.LifeState.IsDead)
		{
			result = 7;
		}
		UnitPartInteractions optional = unit.GetOptional<UnitPartInteractions>();
		if (optional == null)
		{
			return result;
		}
		IUnitInteraction unitInteraction = optional.SelectClickInteraction(m_Runner.Player);
		BlueprintDialog blueprintDialog = ExtractDialogFromUnit(unitInteraction);
		if (blueprintDialog == null)
		{
			if (unitInteraction is SpawnerInteractionActions)
			{
				return 18;
			}
			return result;
		}
		PlayData.DialogEntry dialogData = m_Runner.Data.GetDialogData(blueprintDialog);
		if (!dialogData.Seen || TaskSolveDialog.HasUntriedAnswer(blueprintDialog))
		{
			return 18;
		}
		if (!(Game.Instance.TimeController.GameTime - dialogData.LastTalkTime < 10.Days()))
		{
			if (!dialogData.Seen)
			{
				return 18;
			}
			return 3;
		}
		return result;
	}

	private int GetInteractionPriority(MapObjectEntity mo)
	{
		if (m_Runner.Data.UnreachableObjects.Contains(mo.UniqueId))
		{
			return 0;
		}
		int num = -1;
		foreach (InteractionPart interaction in mo.Interactions)
		{
			num++;
			if (interaction.Enabled && !m_Runner.Data.InteractedObjects.Contains(mo.UniqueId + num) && interaction.SelectUnit(Game.Instance.Player.Party) != null)
			{
				if ((bool)(interaction as InteractionBarkPart))
				{
					return 0;
				}
				if ((bool)(interaction as InteractionLootPart))
				{
					return 18;
				}
				InteractionSkillCheckPart interactionSkillCheckPart = interaction as InteractionSkillCheckPart;
				if ((bool)interactionSkillCheckPart)
				{
					return interactionSkillCheckPart.Settings.TeleportOnSuccess ? 8 : 18;
				}
				InteractionDialogPart interactionDialogPart = interaction as InteractionDialogPart;
				if ((bool)interactionDialogPart)
				{
					return (!m_Runner.Data.InteractedObjects.Contains(interactionDialogPart.Settings.Dialog.AssetGuid)) ? 18 : 0;
				}
				if ((bool)(interaction as DisableTrapInteractionPart))
				{
					return 18;
				}
				InteractionDoorPart interactionDoorPart = interaction as InteractionDoorPart;
				if ((bool)interactionDoorPart)
				{
					return (!interactionDoorPart.IsOpen) ? 17 : 0;
				}
				return 18;
			}
		}
		AreaTransition component = mo.View.GetComponent<AreaTransition>();
		if ((bool)component)
		{
			BlueprintAreaEnterPoint areaEnterPoint = component.Settings.AreaEnterPoint;
			PlayData.AreaEntry areaData = m_Runner.Data.GetAreaData(areaEnterPoint.Area);
			if (areaEnterPoint.Area == Game.Instance.CurrentlyLoadedArea)
			{
				return 8 - m_Runner.Data.LocalTransitionUseCount.Get(areaEnterPoint.AssetGuid, 0) - (areaData.Visited ? 1 : 0);
			}
			if (!areaData.Visited)
			{
				return 6;
			}
			_ = areaData.Depth;
			_ = m_Runner.Data.GetAreaData(Game.Instance.CurrentlyLoadedArea).Depth;
			return 5;
		}
		ScriptZone scriptZone = mo.View as ScriptZone;
		if ((bool)scriptZone && scriptZone.IsActive && !scriptZone.name.Contains("Trap") && !m_Runner.Data.InteractedObjects.Contains(scriptZone.UniqueId))
		{
			return 3;
		}
		return 0;
	}

	private BlueprintDialog ExtractDialogFromUnit(IUnitInteraction unitInteraction)
	{
		if (unitInteraction is SpawnerInteractionPart.Wrapper { Source: SpawnerInteractionDialog source } && source != null)
		{
			return source.Dialog;
		}
		if (unitInteraction is DialogOnClick dialogOnClick)
		{
			return dialogOnClick.Dialog;
		}
		return null;
	}

	private float GetDistanceToObject(ClockworkRunner runner, Entity obj, bool checkNavMesh = false)
	{
		Vector3 position = obj.View.ViewTransform.position;
		if (checkNavMesh && !IsTransition(obj) && !CheckNavmeshes(runner.Player.Position, position))
		{
			return -1f;
		}
		ABPath aBPath;
		try
		{
			aBPath = ABPath.Construct(runner.Player.Position, position);
			aBPath.nnConstraint.graphMask = 1;
			aBPath.Claim(this);
			AstarPath.StartPath(aBPath, pushToFront: true);
			AstarPath.BlockUntilCalculated(aBPath);
		}
		catch (Exception ex)
		{
			Clockwork.Instance.Reporter.HandleWarning($"Cannot find path to object {obj}, exception: {ex.Message} \n {ex.StackTrace}");
			return -1f;
		}
		if (aBPath.error || aBPath.vectorPath == null)
		{
			return -1f;
		}
		if (Vector3.Distance(aBPath.endPoint, position) > 3f)
		{
			return 0f - Vector3.Distance(aBPath.endPoint, position);
		}
		float pathLength = GetPathLength(aBPath.vectorPath);
		aBPath.Release(this);
		return pathLength;
	}

	private bool IsTransition(Entity obj)
	{
		if (!(obj is MapObjectEntity mapObjectEntity))
		{
			return false;
		}
		if (mapObjectEntity.View.GetComponent<AreaTransition>() != null)
		{
			return true;
		}
		foreach (InteractionPart interaction in mapObjectEntity.Interactions)
		{
			if (interaction.Enabled)
			{
				if ((bool)(interaction as InteractionDoorPart))
				{
					return true;
				}
				if (interaction is InteractionSkillCheckPart interactionSkillCheckPart && interactionSkillCheckPart.Settings.UIType != UIInteractionType.Info)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckNavmeshes(Vector3 p1, Vector3 p2)
	{
		return AstarPath.active.GetNearest(p1, m_NNConstraint).node?.Area == AstarPath.active.GetNearest(p2, m_NNConstraint).node?.Area;
	}

	private float GetPathLength(List<Vector3> vectorPath)
	{
		float num = 0f;
		for (int i = 0; i < vectorPath.Count - 1; i++)
		{
			num += Vector3.SqrMagnitude(vectorPath[i + 1] - vectorPath[i]);
		}
		return num;
	}
}
