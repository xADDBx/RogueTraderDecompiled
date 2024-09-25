using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public static class PathPostprocess
{
	[NotNull]
	private static readonly List<Vector3> s_Stack = new List<Vector3>();

	public static void RemoveExcessPoints(Path path)
	{
		s_Stack.Clear();
		List<Vector3> vectorPath = path.vectorPath;
		for (int i = 0; i < vectorPath.Count; i++)
		{
			Vector3 vector = vectorPath[i];
			while (s_Stack.Count >= 2 && ObstacleAnalyzer.TraceAlongNavmesh(s_Stack[s_Stack.Count - 2], vector) == vector)
			{
				s_Stack.RemoveAt(s_Stack.Count - 1);
			}
			s_Stack.Add(vectorPath[i]);
		}
		if (s_Stack.Count != vectorPath.Count)
		{
			vectorPath.Clear();
			for (int j = 0; j < s_Stack.Count; j++)
			{
				Vector3 item = s_Stack[j];
				vectorPath.Add(item);
			}
		}
	}

	public static void TrimLength(Path path, int cells)
	{
		path.path.RemoveRange(cells, path.path.Count - cells);
		path.vectorPath.RemoveRange(cells, path.vectorPath.Count - cells);
	}
}
