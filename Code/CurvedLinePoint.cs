using UnityEngine;

public class CurvedLinePoint : MonoBehaviour
{
	[HideInInspector]
	public bool showGizmo = true;

	[HideInInspector]
	public float gizmoSize = 0.1f;

	[HideInInspector]
	public Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

	private CurvedLineRenderer curvedLine;

	private void OnDrawGizmos()
	{
		if (curvedLine == null)
		{
			curvedLine = base.transform.parent.GetComponent<CurvedLineRenderer>();
		}
		if (showGizmo)
		{
			Gizmos.color = gizmoColor;
			Gizmos.DrawSphere(base.transform.position, gizmoSize);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (curvedLine == null)
		{
			curvedLine = base.transform.parent.GetComponent<CurvedLineRenderer>();
		}
		if (curvedLine != null)
		{
			curvedLine.Update();
		}
	}
}
