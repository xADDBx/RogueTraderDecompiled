using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.GameCore.Mics;

public class GameCoreHistoryLog
{
	[CanBeNull]
	private static GameCoreHistoryLog s_Instance;

	public Action<UnityEngine.Object, string> EtudeEventAction;

	[NotNull]
	public static GameCoreHistoryLog Instance
	{
		get
		{
			Initialize();
			return s_Instance;
		}
	}

	public static void Initialize()
	{
		if (s_Instance == null)
		{
			s_Instance = new GameCoreHistoryLog();
		}
	}

	public void EtudeEvent(UnityEngine.Object context, string message)
	{
		EtudeEventAction?.Invoke(context, message);
	}
}
