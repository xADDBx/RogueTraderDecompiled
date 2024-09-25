using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Controllers;

public class SlowMoController : IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ISlowMoCutsceneHandler, IUnitCommandActHandler, IUnitCommandEndHandler, IControllerDisable, IController, IControllerEnable, IControllerTick
{
	private readonly HashSet<AbstractUnitEntity> m_IncomingUnits = new HashSet<AbstractUnitEntity>();

	private readonly HashSet<AbstractUnitEntity> m_ActingUnits = new HashSet<AbstractUnitEntity>();

	private readonly HashSet<AbstractUnitEntity> m_OutgoingUnits = new HashSet<AbstractUnitEntity>();

	[Cheat]
	public static float SlowMoFactor { get; set; } = 0.1f;


	private void AddUnitToNormalTimeFlow(AbstractUnitEntity unit)
	{
		m_OutgoingUnits.Remove(unit);
		m_IncomingUnits.Add(unit);
	}

	private void AddUnitToNormalTimeFlowImpl(AbstractUnitEntity unit)
	{
		if (m_ActingUnits.Empty())
		{
			Game.Instance.TimeController.SlowMoTimeScale = SlowMoFactor;
		}
		m_ActingUnits.Add(unit);
		UnitAnimationManager maybeAnimationManager = unit.MaybeAnimationManager;
		if ((object)maybeAnimationManager != null)
		{
			maybeAnimationManager.DefaultMixerSpeed = 1f / SlowMoFactor;
		}
		unit.Movable.SlowMoSpeedMod = 1f / SlowMoFactor;
	}

	private void RemoveUnitFromNormalTimeFlow(AbstractUnitEntity unit)
	{
		m_IncomingUnits.Remove(unit);
		m_OutgoingUnits.Add(unit);
	}

	private void RemoveUnitFromNormalTimeFlowImpl(AbstractUnitEntity unit)
	{
		UnitAnimationManager maybeAnimationManager = unit.MaybeAnimationManager;
		if ((object)maybeAnimationManager != null)
		{
			maybeAnimationManager.DefaultMixerSpeed = 1f;
		}
		unit.Movable.SlowMoSpeedMod = 1f;
		m_ActingUnits.Remove(unit);
		if (m_ActingUnits.Empty())
		{
			Game.Instance.TimeController.SlowMoTimeScale = 1f;
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.SlowMotionRequired)
		{
			AddUnitToNormalTimeFlow(command.Executor);
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (command.SlowMotionRequired)
		{
			RemoveUnitFromNormalTimeFlow(command.Executor);
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.SlowMotionRequired)
		{
			RemoveUnitFromNormalTimeFlow(command.Executor);
		}
	}

	public void OnEnable()
	{
		foreach (AbstractUnitEntity item in from unit in Game.Instance.State.AllAwakeUnits
			let cmd = unit.Commands.Current
			where cmd != null
			where cmd.SlowMotionRequired && cmd.IsStarted && !cmd.IsActed && !cmd.IsFinished
			select unit)
		{
			AddUnitToNormalTimeFlow(item);
		}
	}

	public void OnDisable()
	{
		while (true)
		{
			AbstractUnitEntity abstractUnitEntity = m_ActingUnits.FirstOrDefault();
			if (abstractUnitEntity == null)
			{
				break;
			}
			RemoveUnitFromNormalTimeFlowImpl(abstractUnitEntity);
		}
		m_IncomingUnits.Clear();
		m_OutgoingUnits.Clear();
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (AbstractUnitEntity outgoingUnit in m_OutgoingUnits)
		{
			if (m_ActingUnits.Contains(outgoingUnit))
			{
				RemoveUnitFromNormalTimeFlowImpl(outgoingUnit);
			}
		}
		m_OutgoingUnits.Clear();
		foreach (AbstractUnitEntity incomingUnit in m_IncomingUnits)
		{
			if (!m_ActingUnits.Contains(incomingUnit))
			{
				AddUnitToNormalTimeFlowImpl(incomingUnit);
			}
		}
		m_IncomingUnits.Clear();
	}

	[Cheat]
	public static void Debug_Dodge()
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			throw new Exception("Need turn-based mode and current unit");
		}
		Vector3? virtualPosition = Game.Instance.VirtualPositionController.VirtualPosition;
		if (!virtualPosition.HasValue)
		{
			throw new Exception("Need virtual position");
		}
		UnitMovementAgentBase maybeMovementAgent = currentUnit.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			throw new Exception($"Unit {currentUnit} has no movement agent");
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(maybeMovementAgent, virtualPosition.Value);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, currentUnit))
		{
			currentUnit.GetOrCreate<UnitPartJumpAsideDodge>().Dodge(ForcedPath.Construct(warhammerPathPlayer), 0);
		}
	}

	public void AddUnitToNormalTimeline([CanBeNull] AbstractUnitEntity unit)
	{
		if (unit != null)
		{
			AddUnitToNormalTimeFlow(unit);
		}
		else
		{
			Game.Instance.TimeController.SlowMoTimeScale = SlowMoFactor;
		}
	}

	public void OffSlowMo()
	{
		m_OutgoingUnits.AddRange(m_IncomingUnits);
		m_IncomingUnits.Clear();
		Game.Instance.TimeController.SlowMoTimeScale = 1f;
		SlowMoFactor = 0.1f;
	}
}
