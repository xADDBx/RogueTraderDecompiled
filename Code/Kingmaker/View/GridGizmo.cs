using UnityEngine;

namespace Kingmaker.View;

public class GridGizmo : MonoBehaviour
{
	public Bounds Bounds = new Bounds(Vector3.zero, new Vector3(50f, 10f, 50f));

	public bool Draw;

	public float GridStep = 1.55f;

	public Color LineColor = new Color(0.95f, 0.2f, 0.2f);

	public void OnDrawGizmos()
	{
		if (!Draw)
		{
			return;
		}
		Gizmos.color = LineColor;
		int num = Mathf.CeilToInt(Bounds.size.x / GridStep);
		int num2 = Mathf.CeilToInt(Bounds.size.z / GridStep);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector = new Vector3(Bounds.min.x + (float)i * GridStep, Bounds.max.y, Bounds.min.z + (float)j * GridStep);
				Vector3 vector2 = new Vector3(Bounds.min.x + (float)(i + 1) * GridStep, Bounds.max.y, Bounds.min.z + (float)j * GridStep);
				Vector3 vector3 = new Vector3(Bounds.min.x + (float)i * GridStep, Bounds.max.y, Bounds.min.z + (float)(j + 1) * GridStep);
				vector.y = GetGround(vector);
				if (i < num - 1)
				{
					vector2.y = GetGround(vector2);
					Gizmos.DrawLine(vector, vector2);
				}
				if (j < num2 - 1)
				{
					vector3.y = GetGround(vector3);
					Gizmos.DrawLine(vector, vector3);
				}
			}
		}
	}

	private float GetGround(Vector3 pos)
	{
		if (Physics.Raycast(pos, Vector3.down, out var hitInfo, Bounds.size.y, 2359553))
		{
			return hitInfo.point.y + 0.3f;
		}
		return Bounds.min.y;
	}
}
