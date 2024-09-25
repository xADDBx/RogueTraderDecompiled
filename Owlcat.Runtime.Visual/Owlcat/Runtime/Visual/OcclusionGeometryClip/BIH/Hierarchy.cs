using System;
using Unity.Burst;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[BurstCompile]
internal struct Hierarchy<TGeometry> : IDisposable where TGeometry : unmanaged, IGeometry
{
	public ABox sceneBounds;

	public int depth;

	public NativeList<Node> nodeList;

	public NativeArray<TGeometry> geometryArray;

	public void Dispose()
	{
		nodeList.Dispose();
		geometryArray.Dispose();
	}
}
