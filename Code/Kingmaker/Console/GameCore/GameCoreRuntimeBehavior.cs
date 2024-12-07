using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Console.GameCore;

public class GameCoreRuntimeBehavior : MonoSingleton<GameCoreRuntimeBehavior>
{
	public event Action OnApplicationQuitEvent;

	private void OnApplicationQuit()
	{
		Debug.Log("GameCoreRuntimeBehavior.OnQuit()");
		this.OnApplicationQuitEvent?.Invoke();
	}
}
