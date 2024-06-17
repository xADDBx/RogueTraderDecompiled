using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.AreaLogic.Etudes;

public class EtudeBracketGameModeWaiter : IGameModeHandler, ISubscriber
{
	private Action m_Start;

	private bool m_IsStarted;

	private Action m_Stop;

	private bool m_IsStopped;

	public EtudeBracketGameModeWaiter(Action start, Action stop)
	{
		m_Start = start;
		m_Stop = stop;
		if (Game.Instance.CurrentMode != GameModeType.None)
		{
			Start();
		}
		else
		{
			EventBus.Subscribe(this);
		}
	}

	private void Start()
	{
		if (!m_IsStarted && !m_IsStopped)
		{
			m_Start?.Invoke();
			m_IsStarted = true;
		}
	}

	private void Stop()
	{
		if (m_IsStarted && !m_IsStopped)
		{
			m_Stop?.Invoke();
			m_IsStopped = true;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode == GameModeType.None) && !m_IsStarted && !m_IsStopped)
		{
			Start();
			EventBus.Unsubscribe(this);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void Dispose()
	{
		Stop();
		EventBus.Unsubscribe(this);
	}
}
