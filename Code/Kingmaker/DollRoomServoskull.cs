using UnityEngine;

namespace Kingmaker;

[ExecuteInEditMode]
public class DollRoomServoskull : MonoBehaviour
{
	public Transform LookAtObject;

	public Transform RotationDonorRoot;

	public Transform RotationDonor;

	public Transform LookAtTarget;

	public Vector3 ServitorRootUpDirection = Vector3.up;

	public bool EditorMode;

	private void LateUpdate()
	{
		if (Application.isPlaying || (!Application.isPlaying && EditorMode))
		{
			RotationDonorRoot.transform.LookAt(LookAtTarget, ServitorRootUpDirection.normalized);
			LookAtObject.transform.rotation = RotationDonor.transform.rotation;
		}
	}
}
