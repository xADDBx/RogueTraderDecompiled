using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct PathBuilder
{
	private SplinePlotter<PathSplineMetaData, PathSplinePlotterListener> m_SplinePlotter;

	private readonly float m_HalfCellSize;

	private readonly PathLineSettings m_PathLineSettings;

	private NativeArray<CellUnion> m_Cells;

	private NativeArray<int> m_CellIndices;

	private int m_SegmentedDistance;

	public PathBuilder(SplinePlotter<PathSplineMetaData, PathSplinePlotterListener> splinePlotter, float halfCellSize, PathLineSettings pathLineSettings, NativeArray<CellUnion> cells, NativeArray<int> cellIndices)
	{
		m_SplinePlotter = splinePlotter;
		m_HalfCellSize = halfCellSize;
		m_PathLineSettings = pathLineSettings;
		m_Cells = cells;
		m_CellIndices = cellIndices;
		m_SegmentedDistance = 0;
	}

	public void Build()
	{
		int index = m_CellIndices[0];
		int index2 = m_CellIndices[1];
		float3 center = m_Cells[index].cell.center;
		float3 center2 = m_Cells[index2].cell.center;
		m_SplinePlotter.StartLine(center, quaternion.LookRotation(center2 - center, new float3(0f, 1f, 0f)));
		Cell cell = m_Cells[index].cell;
		for (int i = 1; i < m_CellIndices.Length; i++)
		{
			int index3 = m_CellIndices[i];
			Cell nextCell = m_Cells[index3].cell;
			MoveToNextCell(in cell, in nextCell);
			cell = nextCell;
		}
		m_SplinePlotter.FinishLine(default(PathSplineMetaData));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void MoveToNextCell(in Cell cell, in Cell nextCell)
	{
		GetNextCellInfo(in cell, in nextCell, out var midPointPosition, out var midPointHasCut);
		float x = nextCell.center.y - cell.center.y;
		float num = math.abs(x);
		if (midPointHasCut && num > m_PathLineSettings.stepHeightDeltaThreshold)
		{
			float2 xz = cell.center.xz;
			float2 xz2 = nextCell.center.xz;
			float2 @float = math.lerp(xz, xz2, 0.5f);
			float2 float2 = math.normalize(xz - @float);
			float num2 = math.sign(x);
			if (m_PathLineSettings.stepOffset > 0f)
			{
				@float += num2 * float2 * m_PathLineSettings.stepOffset;
			}
			float3 position = new float3(@float.x, cell.center.y, @float.y);
			float3 position2 = new float3(@float.x, nextCell.center.y, @float.y);
			float3 forward = new float3(0f, num2, 0f);
			float3 up = num2 * new float3(float2.x, 0f, float2.y);
			float smoothDistance = math.min(num / 2f, m_PathLineSettings.stepSmoothDistance);
			m_SplinePlotter.PushPoint(position, smoothDistance, m_SegmentedDistance, masked: false);
			m_SplinePlotter.PushPoint(position2, quaternion.LookRotation(forward, up), smoothDistance, m_SegmentedDistance, masked: false);
			m_SplinePlotter.PushPoint(nextCell.center, m_PathLineSettings.turnSmoothDistance, m_SegmentedDistance, masked: false);
			m_SegmentedDistance++;
		}
		else
		{
			float num3 = math.lerp(cell.center.y, nextCell.center.y, 0.5f);
			if (num3 > midPointPosition.y - m_PathLineSettings.edgePenetrationThreshold && num3 < midPointPosition.y + m_PathLineSettings.edgeHoverThreshold)
			{
				m_SplinePlotter.PushPoint(nextCell.center, m_PathLineSettings.turnSmoothDistance, m_SegmentedDistance, masked: false);
				m_SegmentedDistance++;
			}
			else
			{
				m_SplinePlotter.PushPoint(midPointPosition, m_PathLineSettings.edgeSmoothDistance, m_SegmentedDistance, masked: false);
				m_SplinePlotter.PushPoint(nextCell.center, m_PathLineSettings.turnSmoothDistance, m_SegmentedDistance, masked: false);
				m_SegmentedDistance++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetNextCellInfo(in Cell cell, in Cell nextCell, out float3 midPointPosition, out bool midPointHasCut)
	{
		float2 @float = math.sign((nextCell.center.xz - cell.center.xz) / 1000f);
		float y;
		if (@float.x < 0f)
		{
			if (@float.y < 0f)
			{
				midPointHasCut = (nextCell.HasCut(in EdgeDirection.N) || cell.HasCut(in EdgeDirection.W)) && (nextCell.HasCut(in EdgeDirection.E) || cell.HasCut(in EdgeDirection.S));
				y = cell.cornerHeights.sw;
			}
			else if (@float.y > 0f)
			{
				midPointHasCut = (nextCell.HasCut(in EdgeDirection.S) || cell.HasCut(in EdgeDirection.W)) && (nextCell.HasCut(in EdgeDirection.E) || cell.HasCut(in EdgeDirection.N));
				y = cell.cornerHeights.nw;
			}
			else
			{
				midPointHasCut = cell.HasCut(in EdgeDirection.W);
				y = math.lerp(cell.cornerHeights.sw, cell.cornerHeights.nw, 0.5f);
			}
		}
		else if (@float.x > 0f)
		{
			if (@float.y < 0f)
			{
				midPointHasCut = (nextCell.HasCut(in EdgeDirection.N) || cell.HasCut(in EdgeDirection.E)) && (nextCell.HasCut(in EdgeDirection.W) || cell.HasCut(in EdgeDirection.S));
				y = cell.cornerHeights.se;
			}
			else if (@float.y > 0f)
			{
				midPointHasCut = (nextCell.HasCut(in EdgeDirection.S) || cell.HasCut(in EdgeDirection.E)) && (nextCell.HasCut(in EdgeDirection.W) || cell.HasCut(in EdgeDirection.N));
				y = cell.cornerHeights.ne;
			}
			else
			{
				midPointHasCut = cell.HasCut(in EdgeDirection.E);
				y = math.lerp(cell.cornerHeights.se, cell.cornerHeights.ne, 0.5f);
			}
		}
		else if (@float.y < 0f)
		{
			midPointHasCut = cell.HasCut(in EdgeDirection.S);
			y = math.lerp(cell.cornerHeights.sw, cell.cornerHeights.se, 0.5f);
		}
		else
		{
			midPointHasCut = cell.HasCut(in EdgeDirection.N);
			y = math.lerp(cell.cornerHeights.nw, cell.cornerHeights.ne, 0.5f);
		}
		float2 float2 = cell.center.xz + @float * m_HalfCellSize;
		midPointPosition = new float3(float2.x, y, float2.y);
	}
}
