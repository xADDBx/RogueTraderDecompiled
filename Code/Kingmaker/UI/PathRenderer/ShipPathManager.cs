using System.Collections.Generic;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.SurfaceCombatHUD;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.UI.PathRenderer;

public class ShipPathManager : MonoBehaviour
{
	public enum MovementPhase
	{
		One,
		Two,
		Three
	}

	private struct PathNodeMarkerEntity
	{
		public readonly GameObject GameObject;

		public readonly int XCoordinateInGrid;

		public readonly int ZCoordinateInGrid;

		public readonly Renderer Renderer;

		public readonly MovementPhase MovementPhase;

		public PathNodeMarkerEntity(GameObject gameObject, int xCoordinateInGrid, int zCoordinateInGrid, Renderer renderer, MovementPhase movementPhase)
		{
			GameObject = gameObject;
			XCoordinateInGrid = xCoordinateInGrid;
			ZCoordinateInGrid = zCoordinateInGrid;
			Renderer = renderer;
			MovementPhase = movementPhase;
		}
	}

	public GameObject PathNodeMarker;

	public GameObject VantagePointMarker;

	public Material PassThroughCellMaterial;

	public Material SpeedDownCellMaterial;

	public Material SteadySpeedCellMaterial;

	public Material SpeedUpCellMaterial;

	public Material SpeedUp2CellMaterial;

	[Range(0f, 255f)]
	public int PassThroughCellBaseAlpha;

	[Range(0f, 255f)]
	public int SpeedDownCellBaseAlpha;

	[Range(0f, 255f)]
	public int SteadySpeedCellBaseAlpha;

	[Range(0f, 1f)]
	public float NeighbourCellAlphaMultiplier;

	private readonly List<PathNodeMarkerEntity> m_PathNodeMarkers = new List<PathNodeMarkerEntity>();

	private readonly List<GameObject> m_VantagePointMarkers = new List<GameObject>();

	private readonly List<CustomGridNodeBase> m_MovementAreaPhaseOneNodes = new List<CustomGridNodeBase>();

	private readonly List<CustomGridNodeBase> m_MovementAreaPhaseTwoNodes = new List<CustomGridNodeBase>();

	private readonly List<CustomGridNodeBase> m_MovementAreaPhaseThreeNodes = new List<CustomGridNodeBase>();

	private readonly Quaternion[] m_DirectionToRotationMap = new Quaternion[8]
	{
		Quaternion.LookRotation(Vector3.back, Vector3.up),
		Quaternion.LookRotation(Vector3.right, Vector3.up),
		Quaternion.LookRotation(Vector3.forward, Vector3.up),
		Quaternion.LookRotation(Vector3.left, Vector3.up),
		Quaternion.LookRotation(Vector3.back + Vector3.right, Vector3.up),
		Quaternion.LookRotation(Vector3.right + Vector3.forward, Vector3.up),
		Quaternion.LookRotation(Vector3.forward + Vector3.left, Vector3.up),
		Quaternion.LookRotation(Vector3.left + Vector3.back, Vector3.up)
	};

	public static ShipPathManager Instance { get; private set; }

	private bool AnyAbilitySelected => Game.Instance.CursorController.SelectedAbility != null;

	public void SetPathMarkers(StarshipEntity starship, Path newPath)
	{
		ClearPathMarkersInternal();
		ClearMovementAreaNodes();
		if (!(newPath is ShipPath shipPath))
		{
			return;
		}
		IntRect sizeRect = starship.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		Vector3 sizePositionOffset = SizePathfindingHelper.GetSizePositionOffset(sizeRect);
		int width = sizeRect.Width;
		foreach (KeyValuePair<GraphNode, ShipPath.PathCell> item in shipPath.Result)
		{
			if (item.Value.CanStand && item.Key != shipPath.startNode)
			{
				MovementPhase movementPhase = CalculateMovementPhase(item.Value.Length, shipPath.MaxLength, starship.CombatState.ActionPointsBlueSpentThisTurn, starship.Navigation);
				GameObject gameObject = Object.Instantiate(PathNodeMarker, base.transform);
				gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				Transform obj = gameObject.transform;
				obj.position = (Vector3)item.Key.position + sizePositionOffset;
				obj.rotation = ConvertDirection(item.Value.Direction);
				obj.localScale *= (float)width;
				Renderer componentInChildren = gameObject.GetComponentInChildren<Renderer>();
				if (componentInChildren != null)
				{
					componentInChildren.sharedMaterial = GetMarkerMaterial(movementPhase);
				}
				if (item.Key is CustomGridNodeBase customGridNodeBase)
				{
					GetMovementAreaNodeContainer(movementPhase).Add(customGridNodeBase);
					gameObject.SetActive(value: false);
					m_PathNodeMarkers.Add(new PathNodeMarkerEntity(gameObject, customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid, componentInChildren, movementPhase));
				}
			}
		}
		UpdateVantagePointsMarkers(starship, shipPath);
		CombatHUDRenderer.Instance.SetSpaceCombatMovementArea(m_MovementAreaPhaseOneNodes, m_MovementAreaPhaseTwoNodes, m_MovementAreaPhaseThreeNodes);
	}

	public void SetPathMarkers(StarshipEntity starship, Dictionary<GraphNode, CustomPathNode> path, GameObject defaultMarker)
	{
		ClearPathMarkersInternal();
		ClearMovementAreaNodes();
		IntRect sizeRect = starship.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		Vector3 sizePositionOffset = SizePathfindingHelper.GetSizePositionOffset(sizeRect);
		int width = sizeRect.Width;
		foreach (KeyValuePair<GraphNode, CustomPathNode> item in path)
		{
			item.Deconstruct(out var key, out var value);
			GraphNode graphNode = key;
			CustomPathNode customPathNode = value;
			GameObject gameObject = Object.Instantiate(customPathNode.Marker ?? defaultMarker, Instance.transform);
			gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			Transform obj = gameObject.transform;
			obj.position = graphNode.Vector3Position + sizePositionOffset;
			obj.rotation = ConvertDirection(customPathNode.Direction);
			obj.localScale *= (float)width;
			if (graphNode is CustomGridNodeBase customGridNodeBase)
			{
				gameObject.SetActive(value: false);
				m_PathNodeMarkers.Add(new PathNodeMarkerEntity(gameObject, customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid, null, MovementPhase.Three));
			}
		}
		CombatHUDRenderer.Instance.ClearSpaceCombatMovementArea();
	}

	public void ClearPathMarkers()
	{
		ClearPathMarkersInternal();
		ClearMovementAreaNodes();
		CombatHUDRenderer.Instance.ClearSpaceCombatMovementArea();
	}

	public void ClearPathMarkersInternal()
	{
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			Object.Destroy(pathNodeMarker.GameObject);
		}
		m_PathNodeMarkers.Clear();
		HideVantagePointsMarkers();
	}

	public void UpdatePathNodeMarkers(CustomGridNodeBase currentNode)
	{
		DisablePathNodeMarkers();
		if (AnyAbilitySelected)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			if (pathNodeMarker.XCoordinateInGrid == currentNode.XCoordinateInGrid && pathNodeMarker.ZCoordinateInGrid == currentNode.ZCoordinateInGrid)
			{
				num = pathNodeMarker.XCoordinateInGrid;
				num2 = pathNodeMarker.ZCoordinateInGrid;
				pathNodeMarker.GameObject.SetActive(value: true);
				SetPathNodeMarkerColor(pathNodeMarker, isNeighbour: false);
				break;
			}
		}
		if (num < 0 && num2 < 0)
		{
			return;
		}
		foreach (PathNodeMarkerEntity pathNodeMarker2 in m_PathNodeMarkers)
		{
			int xCoordinateInGrid = pathNodeMarker2.XCoordinateInGrid;
			int zCoordinateInGrid = pathNodeMarker2.ZCoordinateInGrid;
			if (xCoordinateInGrid != num || zCoordinateInGrid != num2)
			{
				bool num3 = num - 1 <= xCoordinateInGrid && xCoordinateInGrid <= num + 1;
				bool flag = num2 - 1 <= zCoordinateInGrid && zCoordinateInGrid <= num2 + 1;
				if (num3 && flag)
				{
					pathNodeMarker2.GameObject.SetActive(value: true);
					SetPathNodeMarkerColor(pathNodeMarker2, isNeighbour: true);
				}
			}
		}
	}

	public void DisablePathNodeMarkers()
	{
		foreach (PathNodeMarkerEntity pathNodeMarker in m_PathNodeMarkers)
		{
			pathNodeMarker.GameObject.SetActive(value: false);
		}
	}

	private void SetPathNodeMarkerColor(PathNodeMarkerEntity pathNodeMarker, bool isNeighbour)
	{
		Renderer renderer = pathNodeMarker.Renderer;
		if (!(renderer == null))
		{
			Color color = renderer.material.color;
			int num = Mathf.CeilToInt((float)GetMarkerMaterialAlpha(pathNodeMarker.MovementPhase) * (isNeighbour ? NeighbourCellAlphaMultiplier : 1f));
			renderer.material.color = new Color(color.r, color.g, color.b, (float)num / 255f);
		}
	}

	private MovementPhase CalculateMovementPhase(int length, int maxLength, float spentBlue, PartStarshipNavigation navigation)
	{
		if (spentBlue + (float)length <= (float)navigation.PushPhaseTilesCount)
		{
			return MovementPhase.One;
		}
		if (maxLength - length >= navigation.FinishingTilesCount)
		{
			return MovementPhase.Two;
		}
		return MovementPhase.Three;
	}

	private void ClearMovementAreaNodes()
	{
		m_MovementAreaPhaseOneNodes.Clear();
		m_MovementAreaPhaseTwoNodes.Clear();
		m_MovementAreaPhaseThreeNodes.Clear();
	}

	private List<CustomGridNodeBase> GetMovementAreaNodeContainer(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => m_MovementAreaPhaseOneNodes, 
			MovementPhase.Two => m_MovementAreaPhaseTwoNodes, 
			MovementPhase.Three => m_MovementAreaPhaseThreeNodes, 
			_ => m_MovementAreaPhaseThreeNodes, 
		};
	}

	private Material GetMarkerMaterial(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => SpeedDownCellMaterial, 
			MovementPhase.Two => PassThroughCellMaterial, 
			MovementPhase.Three => SteadySpeedCellMaterial, 
			_ => SteadySpeedCellMaterial, 
		};
	}

	private int GetMarkerMaterialAlpha(MovementPhase phase)
	{
		return phase switch
		{
			MovementPhase.One => SpeedDownCellBaseAlpha, 
			MovementPhase.Two => PassThroughCellBaseAlpha, 
			MovementPhase.Three => SteadySpeedCellBaseAlpha, 
			_ => SteadySpeedCellBaseAlpha, 
		};
	}

	public void UpdateVantagePointsMarkers(StarshipEntity starship, ShipPath shipPath)
	{
		PartVantagePoints optional = starship.GetOptional<PartVantagePoints>();
		int width = starship.SizeRect.Width;
		HideVantagePointsMarkers();
		foreach (Vector3 vantagePoint in optional.VantagePoints)
		{
			GraphNode node = AstarPath.active.GetNearest(vantagePoint).node;
			if (shipPath.Result.TryGetValue(node, out var value) && value.CanStand)
			{
				GameObject gameObject = Object.Instantiate(VantagePointMarker, base.transform);
				gameObject.transform.position = vantagePoint;
				gameObject.transform.localScale *= (float)width;
				gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				m_VantagePointMarkers.Add(gameObject);
			}
		}
	}

	private void HideVantagePointsMarkers()
	{
		foreach (GameObject vantagePointMarker in m_VantagePointMarkers)
		{
			Object.Destroy(vantagePointMarker);
		}
		m_VantagePointMarkers.Clear();
	}

	private Quaternion ConvertDirection(int direction)
	{
		if (direction >= 0 && direction < m_DirectionToRotationMap.Length)
		{
			return m_DirectionToRotationMap[direction];
		}
		return Quaternion.identity;
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Instance = null;
	}
}
