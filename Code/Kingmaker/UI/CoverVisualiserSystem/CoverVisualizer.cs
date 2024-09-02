using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.CoverVisualiserSystem;

public class CoverVisualizer : MonoBehaviour, IUnitMovableAreaHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, IInterruptTurnStartHandler, IRoundStartHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler
{
	private const int CoverGridExtents = 2;

	private const int CoverGridSize = 5;

	public CoverCellController coverCell;

	private CoverCellController[] m_CoverMeshControllers;

	private int m_LastNodeId;

	private Vector3 m_PrevPos;

	private bool m_IsCtrlHold;

	private List<GraphNode> m_MovableAreaNodes;

	private bool m_HasVisibleCovers;

	private bool m_IsPlayerTurn;

	private bool m_IsDeploymentPhase;

	private readonly IntRect m_CurrentNodeSizeRect = new IntRect(0, 0, 1, 1);

	private readonly Dictionary<int, HashSet<DestructibleEntity>> m_DestructiblesCache = new Dictionary<int, HashSet<DestructibleEntity>>();

	public static CoverVisualizer Instance { get; private set; }

	private void Awake()
	{
		m_CoverMeshControllers = (from v in Enumerable.Repeat(0, 25)
			select Object.Instantiate(coverCell, base.transform)).ToArray();
		m_CoverMeshControllers[m_CoverMeshControllers.Length / 2].IsCentral = true;
		UpdatePlayerTurn();
		m_IsDeploymentPhase = Game.Instance.TurnController.IsPreparationTurn && Game.Instance.TurnController.IsDeploymentAllowed;
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			UpdateAllCoverMeshesActive(active: false);
		}
	}

	private void OnEnable()
	{
		Instance = this;
		EventBus.Subscribe(this);
		UpdateDestructiblesCache();
	}

	private void OnDisable()
	{
		Instance = null;
		EventBus.Unsubscribe(this);
		ClearDestructiblesCache();
	}

	private void Update()
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			UpdateAllCoverMeshesActive(active: false);
			return;
		}
		PointerController clickEventsController = Game.Instance.ClickEventsController;
		if (clickEventsController == null)
		{
			UpdateAllCoverMeshesActive(active: false);
			return;
		}
		bool flag = false;
		bool flag2 = KeyboardAccess.IsCtrlHold();
		if (m_IsCtrlHold != flag2)
		{
			m_IsCtrlHold = flag2;
			flag = true;
		}
		Vector3 worldPosition = clickEventsController.WorldPosition;
		if (!flag && (worldPosition - m_PrevPos).magnitude < 0.1f)
		{
			return;
		}
		m_PrevPos = worldPosition;
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(worldPosition).node;
		if (customGridNodeBase != null && (customGridNodeBase.NodeIndex != m_LastNodeId || flag))
		{
			m_LastNodeId = customGridNodeBase.NodeIndex;
			if (!IsNodeCoverVisible(customGridNodeBase))
			{
				HideCoverMeshes();
			}
			else
			{
				UpdateCoverMeshes(customGridNodeBase);
			}
		}
	}

	private void UpdateCoverMeshes(CustomGridNodeBase currentNode)
	{
		CustomGridGraph customGridGraph = (CustomGridGraph)currentNode.Graph;
		int xCoordinateInGrid = currentNode.XCoordinateInGrid;
		int zCoordinateInGrid = currentNode.ZCoordinateInGrid;
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				CustomGridNodeBase node = customGridGraph.GetNode(xCoordinateInGrid + j - 2, zCoordinateInGrid + i - 2);
				CoverCellController coverCellController = m_CoverMeshControllers[i * 5 + j];
				if (node != null && IsNodeCoverVisible(node) && node.Walkable)
				{
					BaseUnitEntity unit = node.GetUnit();
					if (unit == null || !unit.Features.ProvidesFullCover)
					{
						coverCellController.gameObject.SetActive(value: true);
						coverCellController.transform.position = (Vector3)node.position;
						m_HasVisibleCovers = true;
						coverCellController.ChangeCoverIndicator(LosCalculations.GetCellCoverStatus(node, 2), LosCalculations.GetCellCoverStatus(node, 1), LosCalculations.GetCellCoverStatus(node, 0), LosCalculations.GetCellCoverStatus(node, 3));
						continue;
					}
				}
				coverCellController.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateDestructiblesCache()
	{
		ClearDestructiblesCache();
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			foreach (CustomGridNodeBase occupiedNode in destructibleEntity.GetOccupiedNodes())
			{
				if (!m_DestructiblesCache.TryGetValue(occupiedNode.NodeIndex, out var value))
				{
					HashSet<DestructibleEntity> hashSet2 = (m_DestructiblesCache[occupiedNode.NodeIndex] = new HashSet<DestructibleEntity>());
					value = hashSet2;
				}
				value.Add(destructibleEntity);
			}
		}
	}

	private void ClearDestructiblesCache()
	{
		m_DestructiblesCache.Clear();
	}

	private bool IsNodeCoverVisible(CustomGridNodeBase node)
	{
		bool flag = m_MovableAreaNodes?.Contains(node) ?? false;
		bool flag2 = Game.Instance.CursorController.SelectedAbility != null;
		bool num = m_IsPlayerTurn && (flag || m_IsCtrlHold) && !flag2;
		bool flag3 = Game.Instance.CurrentMode == GameModeType.Cutscene;
		if (num || m_IsDeploymentPhase)
		{
			return !flag3;
		}
		return false;
	}

	private void HideCoverMeshes()
	{
		if (m_HasVisibleCovers)
		{
			UpdateAllCoverMeshesActive(active: false);
			m_HasVisibleCovers = false;
		}
	}

	private void UpdateAllCoverMeshesActive(bool active)
	{
		CoverCellController[] coverMeshControllers = m_CoverMeshControllers;
		for (int i = 0; i < coverMeshControllers.Length; i++)
		{
			coverMeshControllers[i].gameObject.SetActive(active);
		}
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		m_MovableAreaNodes = nodes;
	}

	public void HandleRemoveUnitMovableArea()
	{
		m_MovableAreaNodes = null;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdatePlayerTurn();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdatePlayerTurn();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdatePlayerTurn();
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdatePlayerTurn();
	}

	private void UpdatePlayerTurn()
	{
		m_IsPlayerTurn = Game.Instance.TurnController.IsPlayerTurn;
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdatePlayerTurn();
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		m_IsDeploymentPhase = canDeploy;
		UpdatePlayerTurn();
	}

	public void HandleEndPreparationTurn()
	{
		m_IsDeploymentPhase = false;
		UpdatePlayerTurn();
	}
}
