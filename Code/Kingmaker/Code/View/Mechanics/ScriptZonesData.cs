using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.Mechanics;

public struct ScriptZonesData
{
	public Vector3 Position;

	public Vector3 Scale;

	public List<Vector3> NodePositions;

	public ScriptZonesData(Vector3 position, Vector3 scale)
	{
		Position = position;
		Scale = scale;
		NodePositions = new List<Vector3>();
	}
}
