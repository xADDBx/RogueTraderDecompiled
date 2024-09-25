namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

internal static class CastJobUtility
{
	public static int EvaluateFrameStackSize(int treeDepth)
	{
		return treeDepth + 1;
	}
}
