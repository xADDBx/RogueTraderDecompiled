using UnityEngine;

public class ShadowStatic : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "ShadowStatic_Gizmo");
	}
}
