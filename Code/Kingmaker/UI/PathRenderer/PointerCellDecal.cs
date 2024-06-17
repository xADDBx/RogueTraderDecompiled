using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.UI.PathRenderer;

public class PointerCellDecal : MonoBehaviour
{
	public enum TargetType
	{
		Ground,
		Unit
	}

	public enum ActionType
	{
		Move,
		Attack,
		Unable
	}

	[Serializable]
	private class CellDecalDecorator
	{
		[SerializeField]
		private List<CellDecalSetEntry> m_DecorationSet;

		private CellDecalSetEntry GetMaterialEntry(ActionType actionType)
		{
			return m_DecorationSet.FirstOrDefault((CellDecalSetEntry s) => s.ActionType == actionType);
		}

		public MeshRenderer ApplyActionTypeToMeshRenderer(ActionType actionType, MeshRenderer meshRenderer)
		{
			meshRenderer.material = GetMaterialEntry(actionType).Material;
			return meshRenderer;
		}

		public List<MeshRenderer> ApplyActionTypeToMeshRenderer(ActionType actionType, List<MeshRenderer> meshRendererList)
		{
			Material material = GetMaterialEntry(actionType).Material;
			meshRendererList.ForEach(delegate(MeshRenderer mr)
			{
				mr.material = material;
			});
			return meshRendererList;
		}
	}

	[Serializable]
	private struct CellDecalSetEntry
	{
		public ActionType ActionType;

		public Material Material;
	}

	[Header("Path End Marker")]
	[SerializeField]
	private GameObject m_PathEnd;

	[SerializeField]
	private MeshRenderer m_PathEndMeshRenderer;

	[SerializeField]
	private CellDecalDecorator m_PathEndDecorator;

	[Header("CellDecal")]
	[Header("GroundBase")]
	[SerializeField]
	private GameObject m_Cell;

	[SerializeField]
	private MeshRenderer m_CellMeshRenderer;

	[SerializeField]
	private CellDecalDecorator m_CellDecorator;

	[Header("GroundSides")]
	[SerializeField]
	private GameObject m_Sides;

	[SerializeField]
	private List<MeshRenderer> m_SidesMeshRenderer;

	[SerializeField]
	private CellDecalDecorator m_SidesDecorator;

	[Header("Walls")]
	[SerializeField]
	private GameObject m_Walls;

	[SerializeField]
	private List<MeshRenderer> m_WallsMeshRenderer;

	[SerializeField]
	private CellDecalDecorator m_WallsDecorator;

	[Header("Corners")]
	[SerializeField]
	private List<Transform> m_Corners;

	private TargetType m_TargetType;

	private ActionType m_ActionType;

	private int m_Scale;

	public List<Vector3> CornersPositions => m_Corners.Select((Transform c) => c.position).ToList();

	private void Awake()
	{
		SetVisible(visible: false);
		SetTargetType(TargetType.Ground);
	}

	public void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	public void SetTargetType(TargetType targetType)
	{
		if (m_TargetType != targetType)
		{
			m_Sides.SetActive(targetType == TargetType.Ground);
			m_Walls.SetActive(targetType == TargetType.Unit);
			m_TargetType = targetType;
		}
	}

	public void SetActionType(ActionType actionType)
	{
		if (m_ActionType != actionType)
		{
			m_PathEndDecorator.ApplyActionTypeToMeshRenderer(actionType, m_PathEndMeshRenderer);
			m_CellDecorator.ApplyActionTypeToMeshRenderer(actionType, m_CellMeshRenderer);
			m_SidesDecorator.ApplyActionTypeToMeshRenderer(actionType, m_SidesMeshRenderer);
			m_WallsDecorator.ApplyActionTypeToMeshRenderer(actionType, m_WallsMeshRenderer);
			m_ActionType = actionType;
		}
	}

	public void ShowPathEndMarker(bool show)
	{
		m_PathEnd.SetActive(show);
	}

	private void OnDestroy()
	{
		SetActionType(ActionType.Move);
	}
}
