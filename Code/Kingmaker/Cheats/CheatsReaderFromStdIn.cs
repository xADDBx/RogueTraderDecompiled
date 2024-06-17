using System;
using System.Collections.Concurrent;
using System.Threading;
using Kingmaker.Utility.BuildModeUtils;
using UniRx;
using UnityEngine;

namespace Kingmaker.Cheats;

public static class CheatsReaderFromStdIn
{
	private static readonly BlockingCollection<string> m_FromStdIn = new BlockingCollection<string>();

	private static bool IsReadCheatsFromStdInEnabled
	{
		get
		{
			if (!Application.isEditor)
			{
				RuntimePlatform platform = Application.platform;
				return platform == RuntimePlatform.PS4 || platform == RuntimePlatform.PS5;
			}
			return false;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Init()
	{
		if (!BuildModeUtility.CheatsEnabled || !IsReadCheatsFromStdInEnabled)
		{
			return;
		}
		new Thread((ThreadStart)delegate
		{
			while (true)
			{
				OnLineReadFromStdIn(System.Console.ReadLine());
			}
		}).Start();
		MainThreadDispatcher.UpdateAsObservable().Subscribe(Update);
	}

	private static void OnLineReadFromStdIn(string s)
	{
		m_FromStdIn.Add(s);
	}

	private static void Update(Unit _)
	{
		if (m_FromStdIn.TryTake(out var item) && !string.IsNullOrEmpty(item))
		{
			SmartConsole.ExecuteLine(item);
		}
	}
}
