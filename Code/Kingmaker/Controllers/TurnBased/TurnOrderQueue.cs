using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class TurnOrderQueue : IHashable
{
	private EntityRef<MechanicEntity> m_CurrentUnit;

	[JsonProperty]
	public CombatTurnType CurrentTurnType { get; private set; }

	[JsonProperty]
	public TimeSpan? RoamingUnitsTurnEndTime { get; private set; }

	public IEnumerable<MechanicEntity> InterruptingTurnOrder => from i in UnitsInCombat
		where i.Initiative.InterruptingOrder > 0
		orderby i.Initiative.InterruptingOrder descending
		select i;

	public IEnumerable<MechanicEntity> UnitsOrder => UnitsInCombat.OrderByDescending((MechanicEntity i) => i.Initiative.TurnOrderPriority);

	public IEnumerable<MechanicEntity> CurrentRoundUnitsOrder => InterruptingTurnOrder.Concat(from i in UnitsOrder
		where !i.Initiative.ActedThisRound
		orderby i.Initiative.WasPreparedForRound descending
		select i);

	public IEnumerable<MechanicEntity> NextRoundUnitsOrder => UnitsOrder.Where((MechanicEntity i) => i.Initiative.ActedThisRound);

	private static IEnumerable<MechanicEntity> UnitsInCombat
	{
		get
		{
			if (!Game.Instance.TurnController.TurnBasedModeActive)
			{
				return Enumerable.Empty<MechanicEntity>();
			}
			return Game.Instance.TurnController.AllUnits.Where((MechanicEntity i) => i.IsInCombat);
		}
	}

	[CanBeNull]
	public MechanicEntity CurrentUnit
	{
		get
		{
			return m_CurrentUnit;
		}
		private set
		{
			m_CurrentUnit = value;
		}
	}

	public bool IsRoamingUnitsTurn
	{
		get
		{
			if (RoamingUnitsTurnEndTime.HasValue)
			{
				TimeSpan gameTime = Game.Instance.TimeController.GameTime;
				TimeSpan? roamingUnitsTurnEndTime = RoamingUnitsTurnEndTime;
				return gameTime < roamingUnitsTurnEndTime;
			}
			return false;
		}
	}

	[JsonConstructor]
	public TurnOrderQueue()
	{
	}

	public void Clear()
	{
		CurrentUnit = null;
		RoamingUnitsTurnEndTime = null;
	}

	public bool IsEmpty()
	{
		if (CurrentUnit == null && UnitsOrder.Empty())
		{
			return InterruptingTurnOrder.Empty();
		}
		return false;
	}

	public void RestoreCurrentUnit()
	{
		NextTurn(out var _, out var _, out var _);
	}

	public MechanicEntity NextTurn(out bool nextRound, out bool endOfCombat, out CombatTurnType turnType)
	{
		nextRound = false;
		endOfCombat = false;
		turnType = CurrentTurnType;
		if (turnType == CombatTurnType.ManualCombat)
		{
			int num;
			if (!UnitsOrder.Any((MechanicEntity i) => i.IsPlayerEnemy))
			{
				num = (int)turnType;
			}
			else
			{
				CombatTurnType combatTurnType2 = (CurrentTurnType = CombatTurnType.Preparation);
				num = (int)combatTurnType2;
			}
			turnType = (CombatTurnType)num;
			return CurrentUnit = null;
		}
		if (turnType == CombatTurnType.Preparation)
		{
			return CurrentUnit = null;
		}
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		TimeSpan? roamingUnitsTurnEndTime = RoamingUnitsTurnEndTime;
		bool flag = gameTime >= roamingUnitsTurnEndTime;
		if (turnType == CombatTurnType.Roaming)
		{
			if (flag)
			{
				EndRoamingUnitsTurn();
				turnType = CurrentTurnType;
			}
			if (turnType == CombatTurnType.Roaming)
			{
				return CurrentUnit = null;
			}
		}
		endOfCombat = UnitsOrder.Empty();
		if (endOfCombat)
		{
			return CurrentUnit = null;
		}
		ProcessToNextUnit();
		if (CurrentUnit != null)
		{
			return CurrentUnit;
		}
		nextRound = true;
		turnType = CurrentTurnType;
		return CurrentUnit = null;
	}

	public void ProcessToNextUnit()
	{
		MechanicEntity mechanicEntity = CurrentRoundUnitsOrder.FirstOrDefault();
		if (mechanicEntity != null)
		{
			if ((mechanicEntity as InitiativePlaceholderEntity)?.Delegate != null)
			{
				CurrentUnit = ((InitiativePlaceholderEntity)mechanicEntity).Delegate;
				mechanicEntity.Initiative.LastTurn = Game.Instance.TurnController.GameRound;
				mechanicEntity.Initiative.WasPreparedForRound = Game.Instance.TurnController.CombatRound;
			}
			else
			{
				CurrentUnit = mechanicEntity;
			}
		}
		else
		{
			CurrentUnit = null;
		}
	}

	public void InterruptCurrentUnit(MechanicEntity interruptingUnit)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return;
		}
		if (CurrentUnit == null)
		{
			PFLog.Default.ErrorWithReport($"Unit {interruptingUnit} can't interrupt turn because CurrentUnit is null");
			return;
		}
		if (!interruptingUnit.IsInCombat)
		{
			PFLog.Default.ErrorWithReport($"Interrupting unit {interruptingUnit} is not in combat");
			return;
		}
		if (interruptingUnit.Initiative.InterruptingOrder > 0)
		{
			PFLog.Default.ErrorWithReport($"Unit {interruptingUnit} can't interrupt turn while interrupting turn");
			return;
		}
		if (interruptingUnit.IsDirectlyControllable)
		{
			CurrentUnit.GetCommandsOptional()?.RequestUnlockPlayerInputEarlier();
		}
		interruptingUnit.Initiative.InterruptingOrder = InterruptingTurnOrder.Count() + 1;
		CurrentUnit = interruptingUnit;
	}

	private void BeginRoamingUnitsTurn()
	{
		RoamingUnitsTurnEndTime = Game.Instance.TimeController.GameTime + 1.Rounds().Seconds;
		CurrentTurnType = CombatTurnType.Roaming;
		EventBus.RaiseEvent(delegate(IRoamingTurnBeginHandler h)
		{
			h.HandleBeginRoamingTurn();
		});
	}

	public void EndRoamingUnitsTurn()
	{
		RoamingUnitsTurnEndTime = null;
		CurrentTurnType = CombatTurnType.Default;
		EventBus.RaiseEvent(delegate(IRoamingTurnEndHandler h)
		{
			h.HandleEndRoamingTurn();
		});
	}

	public void BeginPreparationTurn()
	{
		CurrentTurnType = CombatTurnType.Preparation;
	}

	public void EndPreparationTurn()
	{
		CurrentTurnType = CombatTurnType.Default;
	}

	public void BeginManualCombat()
	{
		CurrentTurnType = CombatTurnType.ManualCombat;
	}

	public void EndManualCombat()
	{
		CurrentTurnType = CombatTurnType.Default;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		CombatTurnType val = CurrentTurnType;
		result.Append(ref val);
		if (RoamingUnitsTurnEndTime.HasValue)
		{
			TimeSpan val2 = RoamingUnitsTurnEndTime.Value;
			result.Append(ref val2);
		}
		return result;
	}
}
