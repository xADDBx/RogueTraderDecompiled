using System;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[ExecuteInEditMode]
public class FollowTransform : MonoBehaviour
{
	public Func<Vector3>? UpdatePosition;

	public Func<Quaternion>? UpdateRotation;
}
