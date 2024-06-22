using UnityEngine;

namespace Kingmaker.Controllers.FogOfWar.LineOfSight;

public class LineOfSightTester : MonoBehaviour
{
	public Color Color = Color.red;

	public float Radius = 10f;

	public float Angle = 180f;

	public void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = Quaternion.AngleAxis((0f - Angle) / 2f, Vector3.up) * base.transform.forward;
		Gizmos.color = Color;
		int num = Mathf.CeilToInt(Angle * 2f);
		for (int i = 0; i < num; i++)
		{
			Vector3 to = position + vector * Radius;
			to = LineOfSightGeometry.Instance.LineCast(position, to);
			Gizmos.DrawLine(position, to);
			vector = Quaternion.AngleAxis(Angle / (float)num, Vector3.up) * vector;
		}
	}
}
