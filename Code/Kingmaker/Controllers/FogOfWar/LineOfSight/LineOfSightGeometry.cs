using System;
using System.Collections.Generic;
using System.Threading;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Controllers.FogOfWar.LineOfSight;

public class LineOfSightGeometry
{
	private class GeometryCacheCell
	{
		public Vector2 Center;

		public int X;

		public int Y;

		public readonly List<LineSegment> Lines = new List<LineSegment>();

		public void AddSegment(Vector2 p1, Vector2 p2, FogOfWarBlocker blocker)
		{
			Lines.Add(new LineSegment
			{
				A = p1,
				B = p2,
				Height = (blocker ? blocker.HeightMinMax.y : 0f),
				Source = blocker
			});
		}
	}

	private struct LineSegment
	{
		public Vector2 A;

		public Vector2 B;

		public float Height;

		public int SourceID;

		public FogOfWarBlocker Source
		{
			set
			{
				SourceID = 0;
				if (value != null)
				{
					SourceID = value.GetInstanceID();
				}
			}
		}
	}

	public static Vector3 EyeShift = Vector3.up;

	public static Vector3 LegShift = Vector3.up * 0.5f;

	public const float GridSize = 3.8999999f;

	public static readonly LineOfSightGeometry Instance = new LineOfSightGeometry();

	private readonly ReaderWriterLockSlim m_Lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private GeometryCacheCell[,] m_Cells;

	private Bounds m_Bounds;

	[ThreadStatic]
	private static List<GeometryCacheCell> s_CellsForSegment;

	public float LastUpdateTime { get; private set; }

	private LineOfSightGeometry()
	{
	}

	public void Init(Bounds bounds)
	{
		int num = Mathf.CeilToInt(bounds.size.x / 3.8999999f);
		int num2 = Mathf.CeilToInt(bounds.size.z / 3.8999999f);
		m_Cells = new GeometryCacheCell[num, num2];
		float x = (float)num * 3.8999999f;
		float z = (float)num2 * 3.8999999f;
		m_Bounds = new Bounds(bounds.center, new Vector3(x, 0f, z));
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				m_Cells[i, j] = new GeometryCacheCell
				{
					Center = new Vector2(m_Bounds.min.x + 3.8999999f * ((float)i + 0.5f), m_Bounds.min.z + 3.8999999f * ((float)j + 0.5f)),
					X = i,
					Y = j
				};
			}
		}
		foreach (FogOfWarBlocker value in FogOfWarBlocker.All.Values)
		{
			RegisterBlocker(value);
		}
		LastUpdateTime = Time.time;
	}

	public void AddPointsList(Vector2[] points, bool closed, FogOfWarBlocker blocker)
	{
		int num = (closed ? points.Length : (points.Length - 1));
		for (int i = 0; i < num; i++)
		{
			Vector2 p = points[i];
			Vector2 p2 = points[(i + 1) % points.Length];
			GetCellsForSegment(p, p2, skipEmpty: false);
			foreach (GeometryCacheCell item in s_CellsForSegment)
			{
				item.AddSegment(p, p2, blocker);
			}
		}
	}

	public void RegisterBlocker(FogOfWarBlocker blocker)
	{
		if (blocker.Points == null)
		{
			Debug.LogWarning("FogOfWarBlocker " + blocker.name + " has no points! Need to add FogOfWarPolygon or build points from mesh.");
			return;
		}
		m_Lock.EnterWriteLock();
		try
		{
			AddPointsList(blocker.Points, blocker.Closed, blocker);
		}
		finally
		{
			m_Lock.ExitWriteLock();
		}
	}

	public void UpdateBlocker(FogOfWarBlocker blocker)
	{
		if (m_Cells != null && (!Application.isPlaying || !(FogOfWarSettings.Instance == null)))
		{
			RemoveBlocker(blocker);
			RegisterBlocker(blocker);
			LastUpdateTime = Time.time;
		}
	}

	public void RemoveBlocker(FogOfWarBlocker blocker)
	{
		m_Lock.EnterWriteLock();
		try
		{
			if (m_Cells == null || (Application.isPlaying && FogOfWarSettings.Instance == null))
			{
				return;
			}
			int instanceId = blocker.GetInstanceID();
			GeometryCacheCell[,] cells = m_Cells;
			int upperBound = cells.GetUpperBound(0);
			int upperBound2 = cells.GetUpperBound(1);
			for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
				{
					cells[i, j].Lines.RemoveAll((LineSegment l) => l.SourceID == instanceId);
				}
			}
		}
		finally
		{
			m_Lock.ExitWriteLock();
		}
	}

	public bool HasObstacle(Vector3 from, Vector3 to, float fudgeRadius = 0f)
	{
		float d;
		return HasObstacle(from, to, fudgeRadius, needsCoords: false, out d);
	}

	public Vector3 LineCast(Vector3 from, Vector3 to)
	{
		if (HasObstacle(from, to, 0f, needsCoords: true, out var d))
		{
			return from + (to - from) * d;
		}
		return to;
	}

	private bool HasObstacle(Vector3 from, Vector3 to, float fudgeRadius, bool needsCoords, out float d)
	{
		m_Lock.EnterReadLock();
		try
		{
			float num = (d = 1f);
			if (m_Cells == null)
			{
				return false;
			}
			float y = from.y;
			Vector2 vector = from.To2D();
			Vector2 vector2 = to.To2D();
			if (fudgeRadius > 0f)
			{
				vector2 += (vector - vector2).normalized * fudgeRadius;
			}
			GetCellsForSegment(vector, vector2, skipEmpty: true);
			for (int i = 0; i < s_CellsForSegment.Count; i++)
			{
				if (LinecastCell(vector, vector2, s_CellsForSegment[i], y, needsCoords, out d))
				{
					if (!needsCoords)
					{
						return true;
					}
					num = Mathf.Min(num, d);
				}
			}
			d = num;
			return d < 1f;
		}
		finally
		{
			m_Lock.ExitReadLock();
		}
	}

	private bool LinecastCell(Vector2 a, Vector2 b, GeometryCacheCell cell, float height, bool needsCoords, out float d)
	{
		float num = (d = 1f);
		for (int i = 0; i < cell.Lines.Count; i++)
		{
			LineSegment lineSegment = cell.Lines[i];
			if (!(lineSegment.Height < height) && VectorMath.SegmentsIntersect2D(a, b, lineSegment.A, lineSegment.B, out d))
			{
				if (!needsCoords)
				{
					return true;
				}
				num = Mathf.Min(num, d);
			}
		}
		d = num;
		return d < 1f;
	}

	private GeometryCacheCell GetCellForPoint(Vector2 p)
	{
		int num = Mathf.FloorToInt((p.x - m_Bounds.min.x) / 3.8999999f);
		int num2 = Mathf.FloorToInt((p.y - m_Bounds.min.z) / 3.8999999f);
		if (num < 0 || num >= m_Cells.GetLength(0) || num2 < 0 || num2 >= m_Cells.GetLength(1))
		{
			return null;
		}
		return m_Cells[num, num2];
	}

	private void GetCellsForSegment(Vector2 p1, Vector2 p2, bool skipEmpty)
	{
		s_CellsForSegment = s_CellsForSegment ?? new List<GeometryCacheCell>();
		s_CellsForSegment.Clear();
		GeometryCacheCell cellForPoint = GetCellForPoint(p1);
		GeometryCacheCell cellForPoint2 = GetCellForPoint(p2);
		if (cellForPoint == null)
		{
			if (cellForPoint2 != null)
			{
				GetCellsForSegment(p2, p1, skipEmpty);
			}
			return;
		}
		if (!skipEmpty || cellForPoint.Lines.Count > 0)
		{
			s_CellsForSegment.Add(cellForPoint);
		}
		float magnitude = (p2 - p1).magnitude;
		Vector2 vector = (p2 - p1) / magnitude;
		int num = (int)Mathf.Sign(vector.x);
		int num2 = (int)Mathf.Sign(vector.y);
		GeometryCacheCell geometryCacheCell = cellForPoint;
		float num3 = 0f;
		while (geometryCacheCell != cellForPoint2)
		{
			Vector2 vector2 = p1 + vector * num3;
			float num4 = geometryCacheCell.Center.x + (float)num * 3.8999999f / 2f - vector2.x;
			float num5 = geometryCacheCell.Center.y + (float)num2 * 3.8999999f / 2f - vector2.y;
			float num6 = ((vector.x == 0f) ? float.MaxValue : (num4 / vector.x));
			float num7 = ((vector.y == 0f) ? float.MaxValue : (num5 / vector.y));
			int num8 = geometryCacheCell.X;
			int num9 = geometryCacheCell.Y;
			if (num6 <= num7)
			{
				num8 += num;
				if (num8 < 0 || num8 >= m_Cells.GetLength(0))
				{
					break;
				}
				geometryCacheCell = m_Cells[num8, num9];
				if (!skipEmpty || geometryCacheCell.Lines.Count > 0)
				{
					s_CellsForSegment.Add(geometryCacheCell);
				}
			}
			if (num6 >= num7)
			{
				num9 += num2;
				if (num9 < 0 || num9 >= m_Cells.GetLength(1))
				{
					break;
				}
				geometryCacheCell = m_Cells[num8, num9];
				if (!skipEmpty || geometryCacheCell.Lines.Count > 0)
				{
					s_CellsForSegment.Add(geometryCacheCell);
				}
			}
			if (num6 == num7)
			{
				geometryCacheCell = m_Cells[num8 - num, num9];
				if (!skipEmpty || geometryCacheCell.Lines.Count > 0)
				{
					s_CellsForSegment.Add(geometryCacheCell);
				}
			}
			num3 += Mathf.Min(num6, num7);
		}
	}

	public void DrawGizmos()
	{
		if (m_Cells == null)
		{
			return;
		}
		GeometryCacheCell[,] cells = m_Cells;
		foreach (GeometryCacheCell geometryCacheCell in cells)
		{
			if (geometryCacheCell.Lines.Count == 0)
			{
				continue;
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(geometryCacheCell.Center.To3D(), new Vector3(3.8999999f, 0f, 3.8999999f));
			Gizmos.color = Color.magenta;
			foreach (LineSegment line in geometryCacheCell.Lines)
			{
				Gizmos.DrawLine(line.A.To3D(), line.B.To3D());
			}
		}
	}
}
