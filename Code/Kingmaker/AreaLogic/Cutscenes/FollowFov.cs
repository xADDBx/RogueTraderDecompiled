using System;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[ExecuteInEditMode]
public class FollowFov : MonoBehaviour
{
	[SerializeField]
	public Camera? Camera;

	public Func<float>? UpdateFov;

	private void Update()
	{
		if (Camera != null && UpdateFov != null)
		{
			Camera.fieldOfView = UpdateFov();
		}
	}
}
