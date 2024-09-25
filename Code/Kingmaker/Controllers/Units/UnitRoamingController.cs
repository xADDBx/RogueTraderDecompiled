using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Roaming;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitRoamingController : IControllerTick, IController, IRoamingTurnEndHandler, ISubscriber
{
	private const string UnitCutsceneName = "Unit";

	private const float SqrApproachRadius = 0.09f;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController.TurnBasedModeActive && !turnController.IsRoamingTurn)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (ShouldTickOnUnit(allUnit))
			{
				try
				{
					TickOnUnit(allUnit);
				}
				catch (Exception ex)
				{
					PFLog.Default.Error("Exception in " + GetType().Name + " on unit " + allUnit);
					PFLog.Default.Exception(ex);
				}
			}
		}
	}

	private void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartRoaming part = unit.GetOrCreate<UnitPartRoaming>();
		CutscenePlayerData controllingPlayer = CutsceneControlledUnit.GetControllingPlayer(unit);
		if ((controllingPlayer != null && !controllingPlayer.Cutscene.AllowRoaming) || part.Disabled)
		{
			UnitMoveTo currentMoveTo = unit.Commands.CurrentMoveTo;
			if (currentMoveTo != null && currentMoveTo.Roaming)
			{
				currentMoveTo.Interrupt();
			}
			return;
		}
		if (part.NextPoint == null)
		{
			part.NextPoint = RollNextPoint(part);
			if (part.Settings.Radius > 0f)
			{
				part.CachedTargetPosition = part.NextPoint.Position;
			}
		}
		if (IsPlayingIdle(part) || (part.PathInProcess != null && !part.PathInProcess.IsDone() && part.PathInProcess.Claimed.Count != 0))
		{
			return;
		}
		IRoamingPoint point = part.NextPoint;
		if (point == null)
		{
			return;
		}
		if (unit.IsSleeping)
		{
			unit.Wake();
		}
		if (part.CachedTargetPosition.HasValue && (part.CachedTargetPosition.Value - unit.Position).ToXZ().sqrMagnitude > 0.09f)
		{
			part.PathInProcess = PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, part.CachedTargetPosition.Value, 0.3f, 1, delegate(ForcedPath path)
			{
				part.PathInProcess = null;
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (!unit.IsDisposed && !(point is Entity { IsDisposed: not false }))
				{
					path.persistentPath = true;
					UnitMoveToParams obj = new UnitMoveToParams(path, part.CachedTargetPosition.Value)
					{
						MovementType = (part.Settings?.MovementType ?? WalkSpeedType.Walk),
						Orientation = point.Orientation,
						Roaming = true
					};
					RoamingUnitSettings settings = part.Settings;
					obj.OverrideSpeed = ((settings == null || !(settings.MovementSpeed > 0f)) ? null : part.Settings?.MovementSpeed);
					UnitMoveToParams cmdParams = obj;
					unit.Commands.Run(cmdParams);
				}
			});
		}
		else
		{
			TimeSpan gameTime = Game.Instance.TimeController.GameTime;
			part.IdleEndTime = gameTime + point.SelectIdleTime(unit.Random);
			Cutscene cutscene = point.SelectCutscene(unit.Random);
			if (cutscene != null)
			{
				CutscenePlayerView cutscenePlayerView = CutscenePlayerView.Play(cutscene, new ParametrizedContextSetter
				{
					AdditionalParams = { 
					{
						"Unit",
						(object)unit.FromAbstractUnitEntity()
					} }
				}, queued: false, unit.HoldingState);
				part.IdleCutscene = cutscenePlayerView.PlayerData;
			}
			AdvancePoint(part);
		}
	}

	private static IEnumerable<BaseUnitEntity> GetRoamingUnits()
	{
		return Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => unit.GetOptional<UnitPartRoaming>() != null);
	}

	private static IRoamingPoint RollNextPoint(UnitPartRoaming roaming)
	{
		float num = roaming.Settings?.Radius ?? 0f;
		float minInclusive = roaming.Settings?.MinIdleTime ?? 0f;
		float maxInclusive = roaming.Settings?.MaxIdleTime ?? 0f;
		Vector3 pos = roaming.OriginalPoint + roaming.Owner.Random.insideUnitCircle.To3D() * num;
		pos = ObstacleAnalyzer.FindClosestPointToStandOn(pos, roaming.Owner.MovementAgent.Corpulence);
		RoamingPoint obj = new RoamingPoint
		{
			Position = pos,
			IdleTime = roaming.Owner.Random.Range(minInclusive, maxInclusive)
		};
		RoamingUnitSettings settings = roaming.Settings;
		obj.IdleCutscene = ((settings != null) ? settings.IdleCutscenes.Random(roaming.Owner.Random) : null);
		return obj;
	}

	private static void AdvancePoint(UnitPartRoaming part)
	{
		IRoamingPoint nextPoint = part.NextPoint;
		if (nextPoint != null)
		{
			IRoamingPoint roamingPoint = (part.ReverseDirection ? nextPoint.SelectPrevPoint(part.Owner.Random) : nextPoint.SelectNextPoint(part.Owner.Random));
			if (roamingPoint == null)
			{
				part.ReverseDirection = !part.ReverseDirection;
				roamingPoint = (part.ReverseDirection ? nextPoint.SelectPrevPoint(part.Owner.Random) : nextPoint.SelectNextPoint(part.Owner.Random));
			}
			part.NextPoint = roamingPoint;
			part.CachedTargetPosition = ((roamingPoint != null) ? new Vector3?(ObstacleAnalyzer.FindClosestPointToStandOn(roamingPoint.Position, part.Owner.MovementAgent.Corpulence)) : null);
		}
	}

	private bool IsPlayingIdle(UnitPartRoaming part)
	{
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		if (part.IdleEndTime > gameTime)
		{
			return true;
		}
		if (part.IdleCutscene != null && !part.IdleCutscene.IsFinished)
		{
			return true;
		}
		part.IdleEndTime = TimeSpan.Zero;
		part.IdleCutscene = null;
		return false;
	}

	private bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (!unit.LifeState.IsConscious)
		{
			return false;
		}
		if (!unit.Commands.Empty)
		{
			return false;
		}
		if (unit is BaseUnitEntity item && Game.Instance.DialogController.InvolvedUnits.Contains(item))
		{
			return false;
		}
		if (unit.IsInCombat)
		{
			return false;
		}
		if (unit.GetOptional<UnitPartRoaming>() == null)
		{
			return false;
		}
		return true;
	}

	public void HandleEndRoamingTurn()
	{
		foreach (BaseUnitEntity roamingUnit in GetRoamingUnits())
		{
			UnitMoveTo currentMoveTo = roamingUnit.Commands.CurrentMoveTo;
			if (currentMoveTo != null && currentMoveTo.Roaming)
			{
				currentMoveTo.Interrupt();
			}
		}
	}
}
