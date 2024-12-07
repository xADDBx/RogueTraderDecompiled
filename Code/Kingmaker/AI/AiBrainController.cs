using System;
using Core.Cheats;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.MeteorStream;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.AI;

public class AiBrainController : IControllerTick, IController, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, ITurnBasedModeHandler, IAbilityExecutionProcessHandler
{
	[Cheat]
	public static float SecondsToWaitAtStart { get; set; } = 0.5f;


	[Cheat]
	public static float SecondsToWaitAtEnd { get; set; } = 0.5f;


	[Cheat]
	public static float SecondsAiTimeout { get; set; } = 40f;


	[Cheat]
	public static bool AutoCombat { get; set; } = false;


	public static TimeSpan TimeToWaitAtStart => TimeSpan.FromSeconds(SecondsToWaitAtStart);

	private static TimeSpan TimeToWaitAtEnd => TimeSpan.FromSeconds(SecondsToWaitAtEnd);

	private static TimeSpan AiTimeout => TimeSpan.FromSeconds(SecondsAiTimeout);

	private TimeSpan CurrentUnitTurnStartTime => Game.Instance.TurnController.CurrentUnit.GetBrainOptional()?.TurnStartTime ?? default(TimeSpan);

	private TimeSpan CurrentUnitTurnEndTime => Game.Instance.TurnController.CurrentUnit.GetBrainOptional()?.TurnEndTime ?? default(TimeSpan);

	public float AiAbilitySpeedMod { get; private set; } = 1f;


	public bool IsBusy => Game.Instance.AbilityExecutor.Abilities.Count > 0;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (Game.Instance.TurnController.IsAiTurn || AutoCombat)
			{
				mechanicEntity.GetBrainOptional()?.Init();
			}
			AILogger.Instance.Log(AILogTurn.StartTurn(mechanicEntity));
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (Game.Instance.TurnController.IsAiTurn || AutoCombat)
		{
			mechanicEntity.GetBrainOptional()?.Init();
		}
		AILogger.Instance.Log(AILogTurn.StartTurn(mechanicEntity));
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!Game.Instance.TurnController.IsAiTurn && !AutoCombat)
		{
			return;
		}
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit is MeteorStreamEntity)
		{
			return;
		}
		PartUnitBrain brainOptional = currentUnit.GetBrainOptional();
		if (brainOptional == null)
		{
			AILogger.Instance.Error(new AILogReason(AILogReasonType.BrainIsNull));
			EndUnitTurn(currentUnit);
			return;
		}
		if (Game.Instance.TimeController.RealTime - CurrentUnitTurnStartTime > AiTimeout)
		{
			AILogger.Instance.Error(new AILogReason(AILogReasonType.AITimeout));
			PFLog.AI.Error("current unit " + currentUnit.Name + ". Running ability : " + Game.Instance.AbilityExecutor.Abilities.FirstItem()?.Context?.AbilityBlueprint.Name);
			currentUnit.GetCommandsOptional()?.InterruptAll((AbstractUnitCommand cmd) => true);
			brainOptional.UpdateIdleRoundsCounter();
			EndUnitTurn(currentUnit);
			return;
		}
		brainOptional.Tick();
		if (currentUnit != null && currentUnit.IsInSquad)
		{
			if (Game.Instance.TimeController.RealTime > CurrentUnitTurnEndTime)
			{
				brainOptional.UpdateIdleRoundsCounter();
				EndUnitTurn(currentUnit);
			}
		}
		else if (Game.Instance.TimeController.RealTime - CurrentUnitTurnEndTime > TimeToWaitAtEnd)
		{
			brainOptional.UpdateIdleRoundsCounter();
			EndUnitTurn(currentUnit);
		}
	}

	private static void EndUnitTurn(MechanicEntity unit)
	{
		Game.Instance.TurnController.RequestEndTurn();
		AILogger.Instance.Log(AILogTurn.EndTurn(unit));
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			AILogger.Instance.ClearAll();
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit != null && currentUnit.IsInSquad && context.Caster == currentUnit)
		{
			EndUnitTurn(currentUnit);
		}
	}
}
