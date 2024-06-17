namespace Kingmaker.Globalmap.SystemMap;

public class ArcPathSegment : PathSegment
{
	public Circle Circle;

	public float AngleStart;

	public float AngleEnd;

	public ArcPathSegment(Circle circle, float angleStart, float angleEnd)
	{
		Circle = circle;
		AngleStart = angleStart;
		AngleEnd = angleEnd;
	}
}
