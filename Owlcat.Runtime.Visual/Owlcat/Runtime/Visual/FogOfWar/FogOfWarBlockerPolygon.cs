using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

[RequireComponent(typeof(FogOfWarBlocker))]
public class FogOfWarBlockerPolygon : PolygonComponent
{
	public float Height;

	protected override void OnDrawGizmosSelected()
	{
	}

	private void OnDrawGizmos()
	{
	}

	private void DrawWall(Vector3 from, Vector3 to, Matrix4x4 localToWorld, bool twoSided)
	{
		from.y = to.y;
		float num = 0.01f;
		float shadowCullingHeight = FogOfWarSettings.Instance.ShadowCullingHeight;
		Vector3 vector = to - from;
		float magnitude = vector.magnitude;
		Vector3 pos = from + vector / 2f;
		pos.y -= shadowCullingHeight / 2f;
		Gizmos.matrix = localToWorld * Matrix4x4.TRS(pos, Quaternion.FromToRotation(Vector3.right, vector), new Vector3(magnitude, shadowCullingHeight, num));
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		DrawDirections(twoSided, 0.25f / num);
	}

	private void DrawDirections(bool twoSided, float normalLength)
	{
		Vector3 vector = Vector3.forward * normalLength;
		Vector3 vector2 = new Vector3(0f, 0.5f, 0f);
		Vector3 vector3 = -vector2;
		if (twoSided)
		{
			vector2 -= vector;
			vector3 -= vector;
		}
		Color color = Color.white - GizmoColor;
		color.a = GizmoColor.a;
		Gizmos.color = color;
		Gizmos.DrawLine(vector2, vector2 + vector * 2f);
		Gizmos.DrawLine(vector3, vector3 + vector * 2f);
	}
}
