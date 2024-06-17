using System;
using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal static class SnapControllerSystem
{
	private static LinkedList<SnapControllerBase> s_NextUpdateControllers = new LinkedList<SnapControllerBase>();

	private static LinkedList<SnapControllerBase> s_UpdatingControllers = new LinkedList<SnapControllerBase>();

	public static void RegisterControllerForSystemUpdate(LinkedListNode<SnapControllerBase> controllerNode)
	{
		s_NextUpdateControllers.AddLast(controllerNode);
	}

	public static void UnregisterControllerFromSystemUpdate(LinkedListNode<SnapControllerBase> controllerNode)
	{
		controllerNode.List?.Remove(controllerNode);
	}

	public static void Update()
	{
		if (s_NextUpdateControllers.Count == 0 || !TryGetCameraData(out var result))
		{
			return;
		}
		LinkedList<SnapControllerBase> linkedList = s_NextUpdateControllers;
		LinkedList<SnapControllerBase> linkedList2 = s_UpdatingControllers;
		s_UpdatingControllers = linkedList;
		s_NextUpdateControllers = linkedList2;
		while (s_UpdatingControllers.Count > 0)
		{
			LinkedListNode<SnapControllerBase> first = s_UpdatingControllers.First;
			s_UpdatingControllers.RemoveFirst();
			s_NextUpdateControllers.AddLast(first);
			try
			{
				first.Value.OnSystemUpdate(result);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				first.List?.Remove(first);
			}
		}
	}

	private static bool TryGetCameraData(out CameraData result)
	{
		Camera camera = GetCamera();
		if (camera != null)
		{
			result = new CameraData(camera.transform.position);
			return true;
		}
		result = default(CameraData);
		return false;
	}

	private static Camera GetCamera()
	{
		if (UIDollRooms.Instance != null)
		{
			CharacterDollRoom characterDollRoom = UIDollRooms.Instance.CharacterDollRoom;
			if (characterDollRoom != null && characterDollRoom.IsVisible)
			{
				return characterDollRoom.GetComponentInChildren<Camera>();
			}
		}
		return Game.GetCamera();
	}
}
