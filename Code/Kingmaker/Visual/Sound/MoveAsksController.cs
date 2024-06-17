using System;
using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class MoveAsksController : IUnitAsksController, IDisposable, IClickActionHandler, ISubscriber, IRunVirtualMoveCommandHandler, IUnitRunCommandHandler
{
	private readonly List<BaseUnitEntity> m_SelectedUnits = new List<BaseUnitEntity>();

	private BaseUnitEntity m_LastMoveSpeaker;

	public MoveAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
		m_LastMoveSpeaker = null;
	}

	void IRunVirtualMoveCommandHandler.HandleRunVirtualMoveCommand(AbstractUnitCommand command, UnitReference unit)
	{
		ProcessMoveRequest();
	}

	void IClickActionHandler.OnMoveRequested(Vector3 target)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			ProcessMoveRequest();
		}
	}

	private void ProcessMoveRequest()
	{
		bool flag = GetMoveBark(m_LastMoveSpeaker)?.IsOnCooldown ?? false;
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			if (flag && selectedUnit == m_LastMoveSpeaker)
			{
				m_SelectedUnits.Clear();
				return;
			}
			if (!selectedUnit.LifeState.IsConscious)
			{
				continue;
			}
			BarkWrapper moveBark = GetMoveBark(selectedUnit);
			if (moveBark != null && moveBark.HasBarks)
			{
				if (selectedUnit == Game.Instance.Player.MainCharacterEntity)
				{
					baseUnitEntity = selectedUnit;
					break;
				}
				m_SelectedUnits.Add(selectedUnit);
			}
		}
		if (baseUnitEntity == null)
		{
			baseUnitEntity = m_SelectedUnits.Random(PFStatefulRandom.Visuals.UnitAsks);
		}
		if (baseUnitEntity != null)
		{
			ScheduleMove(baseUnitEntity);
		}
		m_SelectedUnits.Clear();
	}

	private static BarkWrapper GetMoveBark(BaseUnitEntity unit)
	{
		if (unit?.View == null)
		{
			return null;
		}
		if (unit.View.Asks == null)
		{
			return null;
		}
		if (!unit.IsInCombat)
		{
			return unit.View.Asks?.OrderMoveExploration;
		}
		return unit.View.Asks?.OrderMove;
	}

	private void ScheduleMove(BaseUnitEntity unit)
	{
		BarkWrapper moveBark = GetMoveBark(unit);
		if (moveBark != null && moveBark.Schedule())
		{
			m_LastMoveSpeaker = unit;
		}
	}

	void IClickActionHandler.OnCastRequested(AbilityData ability, TargetWrapper target)
	{
	}

	void IClickActionHandler.OnItemUseRequested(AbilityData item, TargetWrapper target)
	{
	}

	void IClickActionHandler.OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction)
	{
	}

	void IClickActionHandler.OnAttackRequested(BaseUnitEntity unit, UnitEntityView target)
	{
	}

	void IUnitRunCommandHandler.HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (TurnController.IsInTurnBasedCombat() && command is UnitMoveToProper { Executor: var executor } && !(executor is StarshipEntity) && !executor.IsInPlayerParty && executor.IsInCombat)
		{
			GetMoveBark(executor)?.Schedule();
		}
	}
}
