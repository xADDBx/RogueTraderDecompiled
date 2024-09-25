using UnityEngine;

public class MotionStatic : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "MotionStatic_Gizmo");
	}
}
