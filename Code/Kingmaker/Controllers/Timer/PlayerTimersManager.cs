using System.Collections.Generic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Timer;

public class PlayerTimersManager : IAreaHandler, ISubscriber, IPartyCombatHandler, ITurnEndHandler, ISubscriber<IMechanicEntity>, IInterruptTurnEndHandler, IGameModeHandler, IHashable
{
	[JsonProperty]
	private readonly List<PlayerTimer> m_Timers = new List<PlayerTimer>();

	public IReadOnlyList<PlayerTimer> Timers => m_Timers;

	public void Start(PlayerTimer timer)
	{
		StopAllTimersByBlueprint(timer.Blueprint);
		m_Timers.Add(timer);
		InitTimer(timer);
	}

	public void Stop(BlueprintPlayerTimer timerBp)
	{
		StopAllTimersByBlueprint(timerBp);
	}

	public void SetPaused(BlueprintPlayerTimer timerBp, bool isPaused)
	{
		foreach (PlayerTimer timer in m_Timers)
		{
			if (timer.Blueprint != timerBp)
			{
				continue;
			}
			bool isPaused2 = timer.IsPaused;
			timer.IsPaused = isPaused;
			if (!IsTimersAllowedForGameMode(Game.Instance.CurrentMode))
			{
				continue;
			}
			if (isPaused)
			{
				if (!isPaused2)
				{
					SendHideTimerUIEvent(timer);
					SendCancelTimerEvent(timer);
				}
			}
			else if (isPaused2)
			{
				SendSubscribeTimerEvent(timer);
			}
		}
	}

	public void OnPostLoad()
	{
		foreach (PlayerTimer timer in m_Timers)
		{
			InitTimer(timer);
		}
	}

	private void InitTimer(PlayerTimer timer)
	{
		timer.Stopped += OnStopped;
		SendSubscribeTimerEvent(timer);
		void OnStopped()
		{
			timer.Stopped -= OnStopped;
			m_Timers.Remove(timer);
			SendHideTimerUIEvent(timer);
		}
	}

	public void OnAreaBeginUnloading()
	{
		StopAllTimersByScope(PlayerTimer.ScopeType.Area);
	}

	public void OnAreaDidLoad()
	{
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			StopAllTimersByScope(PlayerTimer.ScopeType.Combat);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			StopAllTimersByScope(PlayerTimer.ScopeType.CombatTurn);
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		StopAllTimersByScope(PlayerTimer.ScopeType.CombatTurn);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!IsTimersAllowedForGameMode(gameMode))
		{
			return;
		}
		foreach (PlayerTimer timer in m_Timers)
		{
			if (!timer.IsPaused)
			{
				SendSubscribeTimerEvent(timer);
			}
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!IsTimersAllowedForGameMode(gameMode))
		{
			return;
		}
		foreach (PlayerTimer timer in m_Timers)
		{
			if (!timer.IsPaused)
			{
				SendHideTimerUIEvent(timer);
				SendCancelTimerEvent(timer);
			}
		}
	}

	private static bool IsTimersAllowedForGameMode(GameModeType gameMode)
	{
		return gameMode == GameModeType.Default;
	}

	private void StopAllTimersByScope(PlayerTimer.ScopeType scope)
	{
		foreach (PlayerTimer timer in m_Timers)
		{
			if (timer.Scope == scope)
			{
				StopTimer(timer);
			}
		}
	}

	private void StopAllTimersByBlueprint(BlueprintPlayerTimer timerBp)
	{
		foreach (PlayerTimer timer in m_Timers)
		{
			if (timer.Blueprint == timerBp)
			{
				StopTimer(timer);
			}
		}
	}

	private static void StopTimer(PlayerTimer timer)
	{
		SendCancelTimerEvent(timer);
		timer.Stop();
	}

	private static void SendSubscribeTimerEvent(PlayerTimer timer)
	{
		EventBus.RaiseEvent(delegate(ITimerHandler e)
		{
			e.SubscribeTimer(timer);
		});
		EventBus.RaiseEvent(delegate(ITimerCounterUIHandler h)
		{
			h.ShowTimerCounter(new TimerShowCounterUIStruct(timer));
		});
	}

	private static void SendCancelTimerEvent(PlayerTimer timer)
	{
		EventBus.RaiseEvent(delegate(ITimerHandler e)
		{
			e.CancelTimer(timer);
		});
	}

	private static void SendHideTimerUIEvent(PlayerTimer timer)
	{
		EventBus.RaiseEvent(delegate(ITimerCounterUIHandler h)
		{
			h.HideTimerCounter(timer.Blueprint.AssetGuid);
		});
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<PlayerTimer> timers = m_Timers;
		if (timers != null)
		{
			for (int i = 0; i < timers.Count; i++)
			{
				Hash128 val = ClassHasher<PlayerTimer>.GetHash128(timers[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
