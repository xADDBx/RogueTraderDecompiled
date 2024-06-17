using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class GridGraphHelpers
{
	public static Int2 GetCellIndex(this CustomGridGraph graph, Vector3 point)
	{
		Vector3 vector = graph.transform.InverseTransform(point);
		int x = (int)vector.x;
		int y = (int)vector.z;
		return new Int2(x, y);
	}

	public static Vector3 GetCellCenterPos(this CustomGridGraph graph, Vector3 point)
	{
		Vector3 vector = graph.transform.InverseTransform(point);
		int num = (int)vector.x;
		int num2 = (int)vector.z;
		return graph.transform.Transform(new Vector3((float)num + 0.5f, 0f, (float)num2 + 0.5f));
	}
}
