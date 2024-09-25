using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Utility.DotNetExtensions;

namespace TurnBased.Utility;

public class TurnBasedZeroGameDeltaTime : IDisposable
{
	private float m_SavedDelta;

	private static readonly List<TurnBasedZeroGameDeltaTime> s_Cache = new List<TurnBasedZeroGameDeltaTime>();

	private TurnBasedZeroGameDeltaTime()
	{
	}

	private void Init()
	{
		m_SavedDelta = Game.Instance.TimeController.GameDeltaTime;
		if (TurnController.IsInTurnBasedCombat())
		{
			Game.Instance.TimeController.SetGameDeltaTime(0f);
		}
	}

	public static TurnBasedZeroGameDeltaTime Create()
	{
		TurnBasedZeroGameDeltaTime turnBasedZeroGameDeltaTime;
		if (s_Cache.Empty())
		{
			turnBasedZeroGameDeltaTime = new TurnBasedZeroGameDeltaTime();
		}
		else
		{
			turnBasedZeroGameDeltaTime = s_Cache[s_Cache.Count - 1];
			s_Cache.RemoveAt(s_Cache.Count - 1);
		}
		turnBasedZeroGameDeltaTime.Init();
		return turnBasedZeroGameDeltaTime;
	}

	public void Dispose()
	{
		Game.Instance.TimeController.SetGameDeltaTime(m_SavedDelta);
		s_Cache.Add(this);
	}
}
