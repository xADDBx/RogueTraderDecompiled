using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Utility.DotNetExtensions;

namespace TurnBased.Utility;

public class TurnBasedZeroDeltaTime : IDisposable
{
	private float m_SavedDelta;

	private static readonly List<TurnBasedZeroDeltaTime> s_Cache = new List<TurnBasedZeroDeltaTime>();

	private TurnBasedZeroDeltaTime()
	{
	}

	private void Init()
	{
		m_SavedDelta = Game.Instance.TimeController.DeltaTime;
		if (TurnController.IsInTurnBasedCombat())
		{
			Game.Instance.TimeController.SetDeltaTime(0f);
		}
	}

	public static TurnBasedZeroDeltaTime Create()
	{
		TurnBasedZeroDeltaTime turnBasedZeroDeltaTime;
		if (s_Cache.Empty())
		{
			turnBasedZeroDeltaTime = new TurnBasedZeroDeltaTime();
		}
		else
		{
			turnBasedZeroDeltaTime = s_Cache[s_Cache.Count - 1];
			s_Cache.RemoveAt(s_Cache.Count - 1);
		}
		turnBasedZeroDeltaTime.Init();
		return turnBasedZeroDeltaTime;
	}

	public void Dispose()
	{
		Game.Instance.TimeController.SetDeltaTime(m_SavedDelta);
		s_Cache.Add(this);
	}
}
