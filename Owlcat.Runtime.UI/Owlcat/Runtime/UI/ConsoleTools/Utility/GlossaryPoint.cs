using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.Utility;

public class GlossaryPoint
{
	public Vector3 StartTextCoordinate;

	public Vector2 StartSize;

	public Vector3 FinishTextCoordinate;

	public Vector2 FinishSize;

	public string LinkID;

	public GlossaryPoint(Vector3 startCoordinate, Vector2 startSize, Vector3 finishCoordinate, Vector2 finishSize, string link)
	{
		StartTextCoordinate = startCoordinate;
		StartSize = startSize;
		FinishTextCoordinate = finishCoordinate;
		FinishSize = finishSize;
		LinkID = link;
	}
}
