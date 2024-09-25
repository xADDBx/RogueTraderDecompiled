using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct PathSplinePlotterListener : ISplinePlotterListener<PathSplineMetaData>
{
	private PathSplineMeshBuilder m_MeshBuilder;

	private ApproximatePathBuilder m_ApproximatePathBuilder;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PathSplinePlotterListener(PathSplineMeshBuilder meshBuilder, ApproximatePathBuilder approximatePathBuilder)
	{
		m_MeshBuilder = meshBuilder;
		m_ApproximatePathBuilder = approximatePathBuilder;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void StartLine()
	{
		m_MeshBuilder.StartLine();
		m_ApproximatePathBuilder.StartLine();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushPoint(in SplinePoint point)
	{
		m_MeshBuilder.PushPoint(in point);
		m_ApproximatePathBuilder.PushPoint(in point);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FinishLine(in PathSplineMetaData metaData)
	{
		m_MeshBuilder.FinishLine();
		m_ApproximatePathBuilder.FinishLine();
	}

	void ISplinePlotterListener<PathSplineMetaData>.PushPoint(in SplinePoint point)
	{
		PushPoint(in point);
	}

	void ISplinePlotterListener<PathSplineMetaData>.FinishLine(in PathSplineMetaData metaData)
	{
		FinishLine(in metaData);
	}
}
