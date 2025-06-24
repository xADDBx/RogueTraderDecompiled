using System;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[ExecuteInEditMode]
public class FollowTransform : MonoBehaviour
{
	public Func<Vector3>? UpdatePosition;

	public Func<Quaternion>? UpdateRotation;

	private void Update()
	{
		if (UpdatePosition != null)
		{
			base.transform.position = UpdatePosition();
		}
		if (UpdateRotation != null)
		{
			base.transform.rotation = UpdateRotation();
		}
	}
}
