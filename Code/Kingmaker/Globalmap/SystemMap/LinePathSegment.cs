using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class LinePathSegment : PathSegment
{
	public Vector2 Start;

	public Vector2 End;

	public LinePathSegment(Vector2 start, Vector2 end)
	{
		Start = start;
		End = end;
	}
}
