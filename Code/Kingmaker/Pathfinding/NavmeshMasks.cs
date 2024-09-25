using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[ExecuteInEditMode]
public class NavmeshMasks : MonoBehaviour
{
	private const int TerrainObstacle = 1;

	public const int ForbidArea = 2;

	public const int AllowArea = 4;

	[SerializeField]
	public float CellSize = 0.2f;

	[SerializeField]
	public string[] ForbidNavmeshMasks = new string[1] { "mountains" };

	[SerializeField]
	public string[] IgnoredMeshes;

	[HideInInspector]
	[SerializeField]
	[Range(0f, 1f)]
	public float MaskAlpha = 0.5f;

	[SerializeField]
	[HideInInspector]
	public Bounds Bounds;

	public bool UpdateMasksWhenCollidersMove;

	private bool? m_WasUpdateMasksWhenCollidersMove;

	private Dictionary<object, Bounds> m_PrevColliderBounds = new Dictionary<object, Bounds>();

	private Rect m_UpdateRect = new Rect
	{
		xMin = float.MaxValue,
		yMin = float.MaxValue,
		xMax = float.MinValue,
		yMax = float.MinValue
	};

	private double m_LastMoveTime;

	private bool m_ColliderTransformChanged;

	[SerializeField]
	private Texture2D m_Data;

	public int MaxX => Mathf.CeilToInt(Bounds.size.x / CellSize);

	public int MaxZ => Mathf.CeilToInt(Bounds.size.z / CellSize);

	public NavmeshMasksGeneration ConvertData()
	{
		if (m_Data == null)
		{
			return default(NavmeshMasksGeneration);
		}
		return new NavmeshMasksGeneration(m_Data.GetPixels(), m_Data.height, m_Data.width);
	}

	private void OnEnable()
	{
		if (AstarData.active?.graphs != null && AstarData.active.graphs.Length != 0 && AstarData.active.graphs[0] is CustomGridGraph customGridGraph)
		{
			customGridGraph.MasksChanged = true;
		}
	}

	private void OnDisable()
	{
		if (AstarData.active?.graphs != null && AstarData.active.graphs.Length != 0 && AstarData.active.graphs[0] is CustomGridGraph customGridGraph)
		{
			customGridGraph.MasksChanged = true;
		}
	}
}
