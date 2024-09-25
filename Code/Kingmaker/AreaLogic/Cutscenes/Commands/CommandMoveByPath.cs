using System.Collections.Generic;
using CatmullRomSplines;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("09f4f8064a60e864cad32ee3f101819b")]
public class CommandMoveByPath : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		[CanBeNull]
		public UnitCommandHandle CommandHandle;

		[CanBeNull]
		public UnitMoveAlongPath Command => CommandHandle?.Cmd as UnitMoveAlongPath;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[AllowedEntityType(typeof(CutscenePath))]
	[ValidateNotEmpty]
	public EntityReference Path;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ConditionalShow("OverrideSpeed")]
	public float Speed = 5f;

	public float PointsPerMeter = 20f;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.CommandHandle = null;
		IEntityViewBase entityViewBase = Path.FindView();
		VectorSpline vectorSpline = (entityViewBase.GO ? entityViewBase.GO.GetComponent<VectorSpline>() : null);
		if (commandData.Unit != null && (bool)vectorSpline)
		{
			List<Vector3> list = new List<Vector3>();
			int num = Mathf.Max(Mathf.CeilToInt(vectorSpline.Length * PointsPerMeter), 2);
			for (int i = 0; i < num; i++)
			{
				list.Add(vectorSpline.EvaluatePosition((float)i / (float)(num - 1)));
			}
			if (skipping)
			{
				commandData.Unit.Translocate(list.LastItem(), null);
				return;
			}
			UnitMoveToParams cmdParams = new UnitMoveToParams(ForcedPath.Construct(list), new Vector3(0f, 0f, 0f))
			{
				MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation),
				OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null)
			};
			commandData.CommandHandle = commandData.Unit.Commands.Run(cmdParams);
			commandData.Unit.View.MovementAgent.AvoidanceDisabled = true;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		if (unit != null)
		{
			PartUnitState stateOptional = unit.GetStateOptional();
			if ((stateOptional == null || stateOptional.CanMove) && unit.IsInState && commandData.Command != null)
			{
				return !unit.Commands.Contains(commandData.Command);
			}
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		UnitMoveAlongPath command = commandData.Command;
		if (unit != null)
		{
			PartUnitState stateOptional = unit.GetStateOptional();
			if ((stateOptional == null || stateOptional.CanMove) && unit.IsInState && command != null && unit.Commands.Contains(command))
			{
				command.Interrupt();
				unit.Translocate(command.Params.ForcedPath.vectorPath.LastItem(), null);
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Unit != null && commandData.Command != null)
		{
			commandData.Command.Interrupt();
			commandData.Unit.View.MovementAgent.AvoidanceDisabled = false;
		}
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return Unit?.ToString() + " <b>move along</b> " + Path;
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}
}
