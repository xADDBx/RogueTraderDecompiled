using System;
using JetBrains.Annotations;
using UnityEngine.LowLevel;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

internal static class PlayerLoopUtility
{
	public static void UnregisterUpdateDelegate([NotNull] Type parentSystemType, [NotNull] Type systemType)
	{
		if (parentSystemType == null)
		{
			throw new ArgumentNullException("parentSystemType");
		}
		if (systemType == null)
		{
			throw new ArgumentNullException("systemType");
		}
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		PlayerLoopSystem[] subSystemList = currentPlayerLoop.subSystemList;
		int num = IndexOfSystem(subSystemList, parentSystemType);
		if (num >= 0)
		{
			PlayerLoopSystem system = subSystemList[num];
			RemoveSubSystem(ref system, systemType);
			subSystemList[num] = system;
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	public static void RegisterUpdateDelegate([NotNull] Type parentSystemType, [NotNull] Type systemType, [NotNull] PlayerLoopSystem.UpdateFunction updateDelegate)
	{
		if (parentSystemType == null)
		{
			throw new ArgumentNullException("parentSystemType");
		}
		if (systemType == null)
		{
			throw new ArgumentNullException("systemType");
		}
		if (updateDelegate == null)
		{
			throw new ArgumentNullException("updateDelegate");
		}
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		PlayerLoopSystem[] subSystemList = currentPlayerLoop.subSystemList;
		int num = IndexOfSystem(subSystemList, parentSystemType);
		if (num >= 0)
		{
			PlayerLoopSystem subSystem = default(PlayerLoopSystem);
			subSystem.type = systemType;
			subSystem.updateDelegate = updateDelegate;
			AddSubSystem(ref subSystemList[num], in subSystem);
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	private static void RemoveSubSystem(ref PlayerLoopSystem system, Type subSystemType)
	{
		PlayerLoopSystem[] subSystemList = system.subSystemList;
		int num = IndexOfSystem(subSystemList, subSystemType);
		if (num >= 0)
		{
			PlayerLoopSystem[] array = new PlayerLoopSystem[subSystemList.Length - 1];
			if (num > 0)
			{
				Array.Copy(subSystemList, 0, array, 0, num);
			}
			if (num < subSystemList.Length - 1)
			{
				Array.Copy(subSystemList, num + 1, array, num, subSystemList.Length - num - 1);
			}
			system.subSystemList = array;
		}
	}

	private static void AddSubSystem(ref PlayerLoopSystem system, in PlayerLoopSystem subSystem)
	{
		PlayerLoopSystem[] subSystemList = system.subSystemList;
		PlayerLoopSystem[] array;
		if (subSystemList != null)
		{
			array = new PlayerLoopSystem[subSystemList.Length + 1];
			Array.Copy(subSystemList, array, subSystemList.Length);
			array[^1] = subSystem;
		}
		else
		{
			array = new PlayerLoopSystem[1] { subSystem };
		}
		system.subSystemList = array;
	}

	private static int IndexOfSystem(PlayerLoopSystem[] systems, Type systemType)
	{
		if (systems == null)
		{
			return -1;
		}
		int i = 0;
		for (int num = systems.Length; i < num; i++)
		{
			if (systems[i].type == systemType)
			{
				return i;
			}
		}
		return -1;
	}
}
