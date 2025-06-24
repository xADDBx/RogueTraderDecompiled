using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Combat;

public class PlayerInputInCombatController : IControllerTick, IController, ITurnBasedModeHandler, ISubscriber
{
	private static readonly TimeSpan CheckIntervalSeconds = 2.Seconds();

	private TimeSpan m_NextCheckTime = TimeSpan.Zero;

	private bool m_IsPlayerInputLocked;

	private bool m_UpdateRequested;

	private int m_LockCounter;

	private Dictionary<object, int> m_LockCounterWithSource = new Dictionary<object, int>();

	public bool IsLocked
	{
		get
		{
			if (m_IsPlayerInputLocked)
			{
				return Game.Instance.TurnController.InCombat;
			}
			return false;
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!Game.Instance.TurnController.InCombat)
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		if (m_NextCheckTime < gameTime || m_UpdateRequested)
		{
			m_UpdateRequested = false;
			m_NextCheckTime = gameTime + CheckIntervalSeconds;
			if (Game.Instance.TurnController.UnitsInCombat.All((MechanicEntity unit) => !unit.IsBusy))
			{
				ForceUnlockPlayerInput();
			}
		}
		UpdateLockStatus();
	}

	public void RequestLockPlayerInput()
	{
		if (Game.Instance.TurnController.InCombat)
		{
			m_LockCounter++;
		}
	}

	public void RequestUnlockPlayerInput()
	{
		if (Game.Instance.TurnController.InCombat)
		{
			m_LockCounter = Math.Max(0, m_LockCounter - 1);
		}
	}

	public void RequestLockPlayerInputWithSource(object source)
	{
		if (Game.Instance.TurnController.InCombat)
		{
			m_LockCounterWithSource.TryGetValue(source, out var value);
			m_LockCounterWithSource[source] = value + 1;
		}
	}

	public void RequestUnlockPlayerInputWithSource(object source)
	{
		if (Game.Instance.TurnController.InCombat)
		{
			m_LockCounterWithSource.TryGetValue(source, out var value);
			if (value > 1)
			{
				m_LockCounterWithSource[source] = value - 1;
			}
			else if (value == 1)
			{
				m_LockCounterWithSource.Remove(source);
			}
		}
	}

	public void RequestUpdate()
	{
		m_UpdateRequested = true;
	}

	private void UpdateLockStatus()
	{
		bool flag = m_LockCounter > 0 || m_LockCounterWithSource.Count > 0;
		if (flag != m_IsPlayerInputLocked)
		{
			if (flag)
			{
				LockPlayerInput();
			}
			else
			{
				UnlockPlayerInput();
			}
		}
	}

	private void ForceUnlockPlayerInput()
	{
		m_LockCounter = 0;
		m_LockCounterWithSource.Clear();
		UnlockPlayerInput();
	}

	private void LockPlayerInput()
	{
		if (!m_IsPlayerInputLocked)
		{
			m_IsPlayerInputLocked = true;
			EventBus.RaiseEvent(delegate(IPlayerInputLockHandler h)
			{
				h.HandlePlayerInputLocked();
			});
		}
	}

	private void UnlockPlayerInput()
	{
		if (m_IsPlayerInputLocked)
		{
			m_IsPlayerInputLocked = false;
			EventBus.RaiseEvent(delegate(IPlayerInputLockHandler h)
			{
				h.HandlePlayerInputUnlocked();
			});
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			ForceUnlockPlayerInput();
		}
	}
}
