using Core.Cheats;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.StaticPerformanceTest;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker;

public static class Cheats
{
	[Cheat(Name = "arbiter_spt_view", Description = "Set camera position with rotation and zoom <x;y;z> <rotation=up|right|down|left> <zoom=4>")]
	public static void MoveCameraToPoint(Vector3 position, string rotation, float zoom = 4f)
	{
		float rotationForDirection = StaticPerformanceCollectTask.GetRotationForDirection(rotation);
		ArbiterService.Instance.Integration.MoveCameraToPoint(position, rotationForDirection, zoom);
	}

	[Cheat(Name = "set_camera", Description = "Move camera to position with rotation and zoom <x;y;z> <0-360> <zoom=4>")]
	public static void MoveCameraToPoint(Vector3 position, float rotation, float zoom = 4f)
	{
		ArbiterService.Instance.Integration.MoveCameraToPoint(position, rotation, zoom);
	}

	[Cheat(Name = "revealer_teleport", Description = "Move revealer to position <x;y;z>")]
	public static void MoveArbiterRevealerToPoint(Vector3 position)
	{
		GameObject gameObject = GameObject.Find("ArbiterRevealer");
		if (gameObject == null)
		{
			gameObject = new GameObject("ArbiterRevealer");
			Object.DontDestroyOnLoad(gameObject);
		}
		gameObject.transform.position = position;
		ArbiterIntegration.RemoveRevealer(gameObject.transform);
		ArbiterIntegration.AddRevealer(gameObject.transform);
	}
}
