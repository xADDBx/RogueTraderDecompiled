using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CurvedLineRenderer : MonoBehaviour
{
	public float lineSegmentSize = 0.15f;

	public float lineWidth = 0.1f;

	public bool closePoints;

	[Header("Gizmos")]
	public bool showGizmos = true;

	public float gizmoSize = 0.1f;

	public Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

	private CurvedLinePoint[] linePoints = new CurvedLinePoint[0];

	private Vector3[] linePositions = new Vector3[0];

	private Vector3[] linePositionsOld = new Vector3[0];

	public void Update()
	{
	}

	public void ManualUpdate()
	{
		GetPoints();
		SetPointsToLine();
	}

	private void GetPoints()
	{
		linePoints = GetComponentsInChildren<CurvedLinePoint>();
		int num = (closePoints ? (linePoints.Length + 1) : linePoints.Length);
		linePositions = new Vector3[num];
		for (int i = 0; i < linePoints.Length; i++)
		{
			linePositions[i] = linePoints[i].transform.position;
		}
		if (closePoints)
		{
			linePositions[num - 1] = linePositions[0];
		}
	}

	private void SetPointsToLine()
	{
		if (linePositionsOld.Length != linePositions.Length)
		{
			linePositionsOld = new Vector3[linePositions.Length];
		}
		bool flag = false;
		for (int i = 0; i < linePositions.Length; i++)
		{
			if (linePositions[i] != linePositionsOld[i])
			{
				flag = true;
			}
		}
		if (flag)
		{
			LineRenderer component = GetComponent<LineRenderer>();
			Vector3[] array = LineSmoother.SmoothLine(linePositions, lineSegmentSize);
			component.positionCount = array.Length;
			component.SetPositions(array);
			component.startWidth = lineWidth;
			component.endWidth = lineWidth;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Update();
	}

	private void OnDrawGizmos()
	{
		if (linePoints.Length == 0)
		{
			GetPoints();
		}
		CurvedLinePoint[] array = linePoints;
		foreach (CurvedLinePoint obj in array)
		{
			obj.showGizmo = showGizmos;
			obj.gizmoSize = gizmoSize;
			obj.gizmoColor = gizmoColor;
		}
	}
}
