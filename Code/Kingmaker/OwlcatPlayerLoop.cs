using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kingmaker.Visual.Particles.SnapController;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Kingmaker;

public static class OwlcatPlayerLoop
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct PreLateUpdate
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public readonly struct SnapControllerSystemUpdate
		{
		}
	}

	private static void RemoveSubsystem<TUpdate, TSystem>(ref PlayerLoopSystem root)
	{
		int num = Array.FindIndex(root.subSystemList, (PlayerLoopSystem s) => s.type == typeof(TUpdate));
		PlayerLoopSystem playerLoopSystem = root.subSystemList[num];
		int num2 = Array.FindIndex(playerLoopSystem.subSystemList, (PlayerLoopSystem s) => s.type == typeof(TSystem));
		PlayerLoopSystem[] array = new PlayerLoopSystem[playerLoopSystem.subSystemList.Length - 1];
		Array.Copy(playerLoopSystem.subSystemList, 0, array, 0, num2);
		Array.Copy(playerLoopSystem.subSystemList, num2 + 1, array, num2, playerLoopSystem.subSystemList.Length - num2 - 1);
		playerLoopSystem.subSystemList = array;
		root.subSystemList[num] = playerLoopSystem;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		int num = Array.FindIndex(currentPlayerLoop.subSystemList, (PlayerLoopSystem s) => s.type == typeof(UnityEngine.PlayerLoop.PreLateUpdate));
		PlayerLoopSystem playerLoopSystem = currentPlayerLoop.subSystemList[num];
		int num2 = IndexOfSubSystem<UnityEngine.PlayerLoop.PreLateUpdate.ParticleSystemBeginUpdateAll>(playerLoopSystem.subSystemList);
		int num3 = IndexOfSubSystem<UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate>(playerLoopSystem.subSystemList);
		List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(playerLoopSystem.subSystemList);
		bool flag = false;
		if (num2 < num3)
		{
			PFLog.Default.Log($"Swapping UnityEngine.PlayerLoop.PreLateUpdate subsystems order: ParticleSystemBeginUpdateAll (index:{num2}) and ScriptRunBehaviourLateUpdate (index:{num3})");
			list[num2] = playerLoopSystem.subSystemList[num3];
			list[num3] = playerLoopSystem.subSystemList[num2];
			flag = true;
		}
		if (IndexOfSubSystem<PreLateUpdate.SnapControllerSystemUpdate>(playerLoopSystem.subSystemList) < 0)
		{
			int num4 = Mathf.Max(num2, num3) + 1;
			PFLog.Default.Log($"Inserting SnapController subsystem into UnityEngine.PlayerLoop.PreLateUpdate subsystems at (index:{num4})");
			list.Insert(num4, new PlayerLoopSystem
			{
				type = typeof(PreLateUpdate.SnapControllerSystemUpdate),
				updateDelegate = SnapControllerSystem.Update
			});
			flag = true;
		}
		if (flag)
		{
			PFLog.Default.Log("Applying UnityEngine.PlayerLoop.PreLateUpdate subsystems modifications");
			playerLoopSystem.subSystemList = list.ToArray();
			currentPlayerLoop.subSystemList[num] = playerLoopSystem;
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	private static int IndexOfSubSystem<T>(PlayerLoopSystem[] subSystemList)
	{
		int i = 0;
		for (int num = subSystemList.Length; i < num; i++)
		{
			if (subSystemList[i].type == typeof(T))
			{
				return i;
			}
		}
		return -1;
	}
}
