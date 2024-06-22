using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.PathRenderer;

public class UnitPathManager : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IHideUIWhileActionCameraHandler, IAreaHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetRoleSetHandler, IAbilityTargetHoverUIHandler, IAbilityOwnerTargetSelectionHandler, IAbilityTargetSelectionUIHandler, IUnitMovableAreaHandler, INetPingPosition
{
	private class PathData
	{
		public Path path;

		public float PathCost;
	}

	private class PingData
	{
		public IDisposable PingDelay { get; set; }
	}

	public static UnitPathManager Instance;

	[Header("Path Rendering")]
	[SerializeField]
	private CombatHudPathRenderer m_PathRenderer;

	[Header("PathDecal")]
	[SerializeField]
	private GameObject m_PathEndDecal;

	[SerializeField]
	private Color m_PathEndDecalColor;

	private GameObject m_CreatedPathEndDecal;

	[Header("PredictDecal")]
	[SerializeField]
	private PointerCellDecal m_PointerCellDecal;

	[SerializeField]
	private PointerCellDecal m_PointerCellDecalSpace;

	private PointerCellDecal m_CreatedPointerCellDecal;

	[Header("PingDecal")]
	[SerializeField]
	private GameObject m_PingDecal;

	[SerializeField]
	private GameObject m_SpacePingDecal;

	private readonly GameObject[] m_CreatedPingDecals = new GameObject[6];

	[Header("Suicide")]
	[SerializeField]
	private GameObject m_SuicideDecal;

	[SerializeField]
	private Color m_SuicideDecalColor;

	[Header("SpacePathDecal")]
	[SerializeField]
	private GameObject m_SpacePathEndDecal;

	[SerializeField]
	private Color m_SpacePathEndDecalColor;

	private GameObject m_CreatedSuicideDecal;

	private CameraRig m_CameraRig;

	private bool m_TbActive;

	private bool m_IsPlayerTurn;

	private bool m_ShouldHide;

	private bool m_TooFarForUnit;

	private bool m_DeploymentPhase;

	public CustomGridNode CurrentNode;

	private Vector3? m_CurrentDecalPosition;

	private int m_DecalScale = 1;

	private Vector3 m_DecalOffset = Vector3.zero;

	private Vector3 m_SizeOffset = Vector3.zero;

	private readonly Dictionary<AbstractUnitEntity, PathData> m_MemorizedPaths = new Dictionary<AbstractUnitEntity, PathData>();

	private Task m_UpdateTask = Task.CompletedTask;

	private CancellationTokenSource m_UpdateTaskCancelToken;

	private Vector3? m_LastShownPosition;

	private AbstractUnitEntity m_UnitCached;

	private Path m_PathCached;

	private float[] m_ApCostPerEveryCellCached;

	private bool m_AbilityHover;

	private readonly Dictionary<NetPlayer, PingData> m_PlayerPingData = new Dictionary<NetPlayer, PingData>();

	public List<Vector3> PointerCellDecalCornersPositions => m_CreatedPointerCellDecal.Or(null)?.CornersPositions ?? new List<Vector3> { PointerWorldCorrectedPosition };

	private Vector3 PointerWorldCorrectedPosition
	{
		get
		{
			if (Game.Instance.ClickEventsController == null)
			{
				return Vector3.zero;
			}
			return Game.Instance.ClickEventsController.WorldPosition - m_DecalOffset;
		}
	}

	[CanBeNull]
	private BaseUnitEntity SelectedUnit => Game.Instance.SelectionCharacter.SelectedUnit.Value;

	private bool ShowPointerCellDecal
	{
		get
		{
			if (m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase))
			{
				return !m_ShouldHide;
			}
			return false;
		}
	}

	public float MemorizedPathCost => m_MemorizedPaths.Values.FirstOrDefault()?.PathCost ?? 0f;

	private void Awake()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void Start()
	{
		m_CameraRig = UnityEngine.Object.FindObjectOfType<CameraRig>();
	}

	private void OnEnable()
	{
		if (!(Instance != null))
		{
			Instance = this;
			EventBus.Subscribe(this);
		}
	}

	private void OnDisable()
	{
		Instance = null;
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
		UnityEngine.Object.Destroy(m_CreatedPathEndDecal);
		UnityEngine.Object.Destroy(m_CreatedSuicideDecal);
		UnityEngine.Object.Destroy(m_CreatedPointerCellDecal.gameObject);
		m_CreatedPingDecals.ForEach(UnityEngine.Object.Destroy);
		m_UpdateTaskCancelToken.Cancel();
	}

	public void OnAreaDidLoad()
	{
		m_ShouldHide = false;
		List<Color> coopPlayersPingsColors = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors;
		if (Game.Instance.IsSpaceCombat)
		{
			m_CreatedPathEndDecal = CreateDecal(m_SpacePathEndDecal, m_SpacePathEndDecalColor);
			for (int i = 0; i < m_CreatedPingDecals.Length; i++)
			{
				m_CreatedPingDecals[i] = CreateDecal(m_SpacePingDecal, coopPlayersPingsColors[i]);
			}
		}
		else
		{
			m_CreatedPathEndDecal = CreateDecal(m_PathEndDecal, m_PathEndDecalColor);
			for (int j = 0; j < m_CreatedPingDecals.Length; j++)
			{
				m_CreatedPingDecals[j] = CreateDecal(m_PingDecal, coopPlayersPingsColors[j]);
			}
		}
		m_CreatedSuicideDecal = CreateDecal(m_SuicideDecal, m_SuicideDecalColor);
		m_CreatedPointerCellDecal = UnityEngine.Object.Instantiate((Game.Instance.CurrentMode == GameModeType.SpaceCombat) ? m_PointerCellDecalSpace : m_PointerCellDecal, base.transform, worldPositionStays: true);
		m_UpdateTaskCancelToken.Cancel();
	}

	private void Update()
	{
		OnUpdate();
		if (m_UpdateTask.IsCompleted)
		{
			m_UpdateTaskCancelToken = new CancellationTokenSource();
			m_UpdateTask = OnUpdateAsync(m_UpdateTaskCancelToken.Token);
		}
	}

	private void OnUpdate()
	{
		if (!m_CreatedPointerCellDecal)
		{
			return;
		}
		m_CreatedPointerCellDecal.SetVisible(ShowPointerCellDecal);
		if (ShowPointerCellDecal)
		{
			CameraRig cameraRig = m_CameraRig.Or(null);
			if ((object)cameraRig == null || !cameraRig.RotationByMouse)
			{
				UpdateDecalScale();
				UpdateSizeOffset();
				UpdateCurrentNode();
				UpdatePredict();
			}
		}
	}

	private void UpdateDecalScale()
	{
		m_DecalScale = GetCellScaleByUnit(SelectedUnit);
	}

	private int GetCellScaleByUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return 1;
		}
		IntRect sizeRect = unit.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		if (!(Game.Instance.CursorController.SelectedAbility != null))
		{
			return sizeRect.Width;
		}
		return 1;
	}

	private void UpdateSizeOffset()
	{
		m_SizeOffset = GetCellOffsetForUnit(SelectedUnit);
		m_DecalOffset = ((m_DecalScale == 1) ? new Vector3(0f, 0f, 0f) : m_SizeOffset);
	}

	public Vector3 GetCellOffsetForUnit(MechanicEntity unit)
	{
		if (unit == null)
		{
			return Vector3.zero;
		}
		if (CurrentNode == null)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		if (unit.Size == Size.Cruiser_2x4)
		{
			float num = GraphParamsMechanicsCache.GridCellSize / 2f;
			if (PointerWorldCorrectedPosition.x >= CurrentNode.Vector3Position.x)
			{
				zero.x += num;
			}
			else
			{
				zero.x -= num;
			}
			if (PointerWorldCorrectedPosition.z >= CurrentNode.Vector3Position.z)
			{
				zero.z += num;
			}
			else
			{
				zero.z -= num;
			}
		}
		else if (unit.Size.IsBigAndEvenUnit())
		{
			float num2 = GraphParamsMechanicsCache.GridCellSize / 2f;
			zero.x += num2;
			zero.z += num2;
		}
		return zero;
	}

	private void UpdateCurrentNode()
	{
		CurrentNode = (CustomGridNode)PointerWorldCorrectedPosition.GetNearestNodeXZ();
	}

	private async Task OnUpdateAsync(CancellationToken token)
	{
		await DrawPathToPoint(m_CurrentDecalPosition, token);
	}

	private void UpdatePredict()
	{
		if (CurrentNode == null)
		{
			return;
		}
		if (InvalidNode(CurrentNode, PointerWorldCorrectedPosition))
		{
			UpdateInvalid(PointerWorldCorrectedPosition);
		}
		else if (SelectedUnit == null)
		{
			MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
			if (currentUnit != null && currentUnit.IsDirectlyControllable)
			{
				UpdateInactiveState();
			}
		}
		else
		{
			m_CurrentDecalPosition = CurrentNode.Vector3Position + m_DecalOffset;
			SetDecalPosition(m_CreatedPointerCellDecal.transform, CurrentNode, m_CurrentDecalPosition);
			UpdatePredictState(CurrentNode);
		}
	}

	private bool InvalidNode(CustomGridNode node, Vector3 worldPosition)
	{
		if (node == null)
		{
			return true;
		}
		if (!node.ContainsPoint(worldPosition))
		{
			return true;
		}
		if (Game.Instance.CursorController.SelectedAbility == null && WarhammerBlockManager.Instance.NodeContainsAny(node))
		{
			BaseUnitEntity selectedUnit = SelectedUnit;
			if (selectedUnit == null)
			{
				return true;
			}
			NodeList occupiedNodes = selectedUnit.GetOccupiedNodes();
			CustomGridNodeBase nearestNodeXZUnwalkable = selectedUnit.Position.GetNearestNodeXZUnwalkable();
			if (nearestNodeXZUnwalkable == null)
			{
				return true;
			}
			Vector2Int coordinatesInGrid = nearestNodeXZUnwalkable.CoordinatesInGrid;
			Vector2Int coordinatesInGrid2 = node.CoordinatesInGrid;
			if (occupiedNodes.Contains(node))
			{
				return coordinatesInGrid2 == coordinatesInGrid;
			}
			return true;
		}
		return false;
	}

	private void UpdateInvalid(Vector3 worldPosition)
	{
		m_CreatedPointerCellDecal.transform.position = worldPosition;
		m_CurrentDecalPosition = null;
		if (Game.Instance.CursorController.SelectedAbility == null)
		{
			Game.Instance.CursorController.SetCursor(CursorType.Restricted);
			Game.Instance.CursorController.ClearComponents();
		}
		UpdatePredictState(null);
	}

	private void UpdateInactiveState()
	{
		m_CurrentDecalPosition = CurrentNode.Vector3Position + m_DecalOffset;
		SetDecalPosition(m_CreatedPointerCellDecal.transform, CurrentNode, m_CurrentDecalPosition);
		bool flag = m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase) && !m_ShouldHide;
		m_CreatedPointerCellDecal.SetVisible(flag);
		if (flag)
		{
			m_CreatedPointerCellDecal.SetTargetType(PointerCellDecal.TargetType.Ground);
			UpdateMoveMarker();
			m_CreatedPointerCellDecal.SetActionType(PointerCellDecal.ActionType.Unable);
			UnitPredictionManager.Instance.Or(null)?.ResetVirtualHoverPosition();
		}
	}

	private void UpdatePredictState(GraphNode node)
	{
		bool flag = m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase) && !m_ShouldHide;
		m_CreatedPointerCellDecal.SetVisible(flag);
		if (!flag)
		{
			return;
		}
		BaseUnitEntity selectedUnit = SelectedUnit;
		if (selectedUnit == null)
		{
			UpdateMoveMarker();
			return;
		}
		bool flag2 = false;
		Vector2Int coordinatesInGrid = selectedUnit.Position.GetNearestNodeXZUnwalkable().CoordinatesInGrid;
		Vector2Int? vector2Int = (node as CustomGridNode)?.CoordinatesInGrid;
		if (node == null || coordinatesInGrid == vector2Int)
		{
			flag2 = true;
		}
		bool flag3 = (m_DeploymentPhase ? (flag2 || !ClickSurfaceDeploymentHandler.CanDeployUnit(node, selectedUnit.SizeRect)) : (flag2 || m_TooFarForUnit || !WarhammerBlockManager.Instance.CanUnitStandOnNode(selectedUnit, node as CustomGridNode)));
		AbilityData selectedAbility = Game.Instance.CursorController.SelectedAbility;
		bool flag4 = selectedAbility != null && selectedAbility.TargetAnchor == AbilityTargetAnchor.Unit;
		m_CreatedPointerCellDecal.SetTargetType(flag4 ? PointerCellDecal.TargetType.Unit : PointerCellDecal.TargetType.Ground);
		PointerCellDecal.ActionType actionType;
		if (selectedAbility == null)
		{
			actionType = (flag3 ? PointerCellDecal.ActionType.Unable : PointerCellDecal.ActionType.Move);
			List<Vector3> list = m_MemorizedPaths.Values.FirstOrDefault()?.path?.vectorPath;
			bool state = false;
			if (list != null)
			{
				Vector3 vector = list[list.Count - 1];
				state = node != null && vector == node.Vector3Position;
			}
			Game.Instance.CursorController.SetMoveCursor(state, flag3);
		}
		else
		{
			PointerController clickEventsController = Game.Instance.ClickEventsController;
			TargetWrapper targetForDesiredPosition = Game.Instance.SelectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, clickEventsController.WorldPosition);
			actionType = ((targetForDesiredPosition != null && selectedAbility.CanTargetFromDesiredPosition(targetForDesiredPosition)) ? PointerCellDecal.ActionType.Attack : PointerCellDecal.ActionType.Unable);
		}
		UpdateMoveMarker();
		m_CreatedPointerCellDecal.SetActionType(actionType);
		if (flag3)
		{
			UnitPredictionManager.Instance?.ResetVirtualHoverPosition();
			return;
		}
		try
		{
			UISounds.Instance.Sounds.Combat.CombatGridHover.Play();
		}
		catch (Exception ex)
		{
			PFLog.Audio.Exception(ex);
		}
		UnitPredictionManager.Instance?.SetVirtualHoverPosition(node.Vector3Position);
	}

	private void UpdateMoveMarker()
	{
		m_CreatedPointerCellDecal.ShowPathEndMarker((m_PathRenderer.PathShown && Game.Instance.CursorController.SelectedAbility == null && !Game.Instance.VirtualPositionController.HasVirtualPosition) || Game.Instance.TurnController.IsPreparationTurn);
	}

	private async Task DrawPathToPoint(Vector3? position, CancellationToken token)
	{
		if (!ShowPointerCellDecal)
		{
			ClearActivePath();
		}
		else
		{
			if ((m_CameraRig.Or(null)?.RotationByMouse ?? false) || m_DeploymentPhase || !m_MemorizedPaths.Empty())
			{
				return;
			}
			BaseUnitEntity unit = SelectedUnit;
			if (unit == null || unit.CombatState.ActionPointsBlue == 0f)
			{
				ClearActivePath();
			}
			else if (unit.IsCastingAbility())
			{
				ClearActivePath();
			}
			else if (!position.HasValue)
			{
				ClearActivePath();
			}
			else
			{
				if (position == m_LastShownPosition)
				{
					return;
				}
				m_LastShownPosition = position;
				if (!TurnController.IsInTurnBasedCombat())
				{
					ClearActivePath();
				}
				else
				{
					if (Game.Instance.CurrentMode == GameModeType.SpaceCombat)
					{
						return;
					}
					if (!Game.Instance.TurnController.IsPlayerTurn || Game.Instance.CursorController.SelectedAbility != null)
					{
						ClearActivePath();
					}
					else
					{
						if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(unit, position.Value.GetNearestNodeXZUnwalkable()) || !unit.IsMyNetRole())
						{
							return;
						}
						Vector3 unitSizeOffset = m_SizeOffset;
						UnitMovementAgentBase unitMovementAgent = unit.View.MovementAgent;
						using PathDisposable<WarhammerPathPlayer> pathDisposable = await PathfindingService.Instance.FindPathTB_Task(unitMovementAgent, position.Value - unitSizeOffset, -1, this);
						token.ThrowIfCancellationRequested();
						WarhammerPathPlayer warhammerPathPlayer = pathDisposable?.Path;
						List<GraphNode> list = ((warhammerPathPlayer != null && !warhammerPathPlayer.error) ? warhammerPathPlayer.path : null);
						if (list == null || list.Count < 2)
						{
							ClearActivePath();
							return;
						}
						float cost = warhammerPathPlayer.CalculatedPath[^1].Length;
						m_TooFarForUnit = cost > unit.CombatState.ActionPointsBlue;
						SetupActivePath(list, m_TooFarForUnit, unitSizeOffset, unitMovementAgent);
						UpdateMoveMarker();
						EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
						{
							h.HandleCurrentNodeChanged(cost);
						}, isCheckRuntime: true);
					}
				}
			}
		}
	}

	public void AddPath(BaseUnitEntity unit, Path path, float apPerCell, float actionPointsBlue, bool oddDiagonal, float[] apCostPerEveryCell = null)
	{
		m_UnitCached = unit;
		m_PathCached = path;
		m_ApCostPerEveryCellCached = apCostPerEveryCell;
		PathData pathData = CreatePathData(unit, path);
		IntRect sizeRect = unit.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		Vector3 sizePositionOffset = SizePathfindingHelper.GetSizePositionOffset(sizeRect);
		float num = actionPointsBlue;
		int num2 = (oddDiagonal ? 1 : 0);
		for (int i = 1; i < path.vectorPath.Count; i++)
		{
			Vector2 a = path.vectorPath[i - 1].To2D();
			Vector2 b = path.vectorPath[i].To2D();
			bool num3 = a.IsDiagonal(b);
			if (num3)
			{
				num2++;
			}
			int num4 = ((!num3 || num2 % 2 != 0) ? 1 : 2);
			float num5 = ((apCostPerEveryCell != null && i < apCostPerEveryCell.Length) ? apCostPerEveryCell[i] : (apPerCell * (float)num4));
			num -= num5;
		}
		pathData.PathCost = actionPointsBlue - num;
		List<Vector3> vectorPath = path.vectorPath;
		Vector3 vector = vectorPath[vectorPath.Count - 1];
		CustomGridNodeBase node = (CustomGridNodeBase)AstarPath.active.GetNearest(vector).node;
		BaseUnitEntity unit2 = node.GetUnit();
		GameObject decal = ((unit2 != null && !unit2.IsDead && !unit2.IsInPlayerParty) ? m_CreatedSuicideDecal : m_CreatedPathEndDecal);
		DrawDecalAt(decal, node, vector + sizePositionOffset);
		UpdatePredictState(node);
		UpdatePathRenderer();
		EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
		{
			h.HandlePathAdded(path, pathData.PathCost);
		}, isCheckRuntime: true);
	}

	public void RemoveAllPaths()
	{
		foreach (AbstractUnitEntity item in new List<AbstractUnitEntity>(m_MemorizedPaths.Keys.ToList()))
		{
			RemovePathInternal(item);
		}
		UpdatePathRenderer();
	}

	public void RemovePath(AbstractUnitEntity unit)
	{
		if (RemovePathInternal(unit))
		{
			UpdatePathRenderer();
		}
	}

	private bool RemovePathInternal(AbstractUnitEntity unit)
	{
		if (m_MemorizedPaths.Remove(unit))
		{
			SetDecalVisibility(m_CreatedPathEndDecal, isVisible: false);
			SetDecalVisibility(m_CreatedSuicideDecal, isVisible: false);
			m_CreatedPointerCellDecal.SetVisible(visible: false);
			EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
			{
				h.HandlePathRemoved();
			}, isCheckRuntime: true);
			m_UpdateTaskCancelToken.Cancel();
			if (!m_AbilityHover)
			{
				m_UnitCached = null;
				m_PathCached = null;
				m_ApCostPerEveryCellCached = null;
			}
			return true;
		}
		return false;
	}

	public Path GetPath(AbstractUnitEntity unit)
	{
		if (unit != null && m_MemorizedPaths.TryGetValue(unit, out var value))
		{
			return value.path;
		}
		return null;
	}

	public void DrawDecalAt(GameObject decal, GraphNode node, Vector3? overridePosition = null)
	{
		if (!(decal == null))
		{
			SetDecalVisibility(decal, isVisible: true);
			SetDecalPosition(decal.transform, node, overridePosition);
		}
	}

	private GameObject CreateDecal(GameObject prefab, Color color, bool active = false)
	{
		if (prefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, base.transform, worldPositionStays: true);
		SetDecalColor(gameObject, color);
		SetDecalVisibility(gameObject, active);
		return gameObject;
	}

	private void SetDecalPosition(Transform decalTransform, GraphNode node, Vector3? overridePosition = null)
	{
		CustomGridGraph obj = (CustomGridGraph)node.Graph;
		Vector3 position = overridePosition ?? node.Vector3Position;
		obj.collision.CheckHeight(position, out var hit, out var _, 200f);
		decalTransform.position = position;
		Vector3 localScale = decalTransform.localScale;
		localScale.x = 1.35f * (float)m_DecalScale;
		localScale.z = 1.35f * (float)m_DecalScale;
		decalTransform.localScale = localScale;
		if (hit.normal != Vector3.zero)
		{
			decalTransform.rotation = Quaternion.LookRotation(decalTransform.forward, hit.normal.normalized);
		}
	}

	private void SetDecalColor(GameObject decal, Color color)
	{
		decal.GetComponentInChildren<Renderer>().material.color = color;
	}

	private void SetDecalVisibility(GameObject decal, bool isVisible)
	{
		if (decal != null)
		{
			decal.SetActive(isVisible);
		}
	}

	private PathData CreatePathData(BaseUnitEntity unit, Path path)
	{
		if (!m_MemorizedPaths.TryGetValue(unit, out var value))
		{
			value = new PathData();
			m_MemorizedPaths.Add(unit, value);
		}
		value.path = path;
		return value;
	}

	private void UpdatePathRenderer()
	{
		if (m_PathRenderer == null)
		{
			return;
		}
		if (true & m_TbActive)
		{
			foreach (KeyValuePair<AbstractUnitEntity, PathData> memorizedPath in m_MemorizedPaths)
			{
				AbstractUnitEntity key = memorizedPath.Key;
				if (key != null)
				{
					List<GraphNode> list = memorizedPath.Value?.path?.path;
					if (list != null && list.Count >= 2)
					{
						SetupActivePath(list, pathUnableStatus: false, m_SizeOffset, key.MovementAgent);
						return;
					}
				}
			}
		}
		ClearActivePath();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
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
		m_UnitCached = null;
		m_PathCached = null;
		m_ApCostPerEveryCellCached = null;
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnBasedModeHandle(isTurnBased: true);
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		m_TbActive = isTurnBased;
		if (m_CreatedPointerCellDecal != null)
		{
			m_CreatedPointerCellDecal.SetVisible(m_TbActive && !Game.Instance.TurnController.IsPreparationTurn);
		}
		if (!m_TbActive)
		{
			RemoveAllPaths();
		}
		UpdatePlayerTurn();
	}

	public void HandleHideUI()
	{
		m_ShouldHide = true;
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			m_ShouldHide = false;
		}, 2.5f);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		m_DeploymentPhase = canDeploy;
		UpdatePlayerTurn();
	}

	public void HandleEndPreparationTurn()
	{
		m_DeploymentPhase = false;
		UpdatePlayerTurn();
	}

	public void HandleRoleSet(string entityId)
	{
		UpdatePathRenderer();
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		m_AbilityHover = hover;
		if (!hover)
		{
			m_UpdateTaskCancelToken.Cancel();
		}
		if (ability.TargetAnchor == AbilityTargetAnchor.Owner)
		{
			if (hover)
			{
				UnitHelper.ClearPrediction();
			}
			else if (m_UnitCached is BaseUnitEntity unit && m_PathCached != null)
			{
				UnitHelper.DrawMovePrediction(unit, m_PathCached, m_ApCostPerEveryCellCached);
			}
		}
	}

	public void HandleOwnerAbilitySelected(AbilityData ability)
	{
		m_UnitCached = null;
		m_PathCached = null;
		m_ApCostPerEveryCellCached = null;
		m_LastShownPosition = null;
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		m_UpdateTaskCancelToken.Cancel();
	}

	public void HandleRemoveUnitMovableArea()
	{
		m_UpdateTaskCancelToken.Cancel();
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap || Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			return;
		}
		int playerIndex = player.Index - 1;
		if (m_PlayerPingData.ContainsKey(player))
		{
			m_PlayerPingData[player].PingDelay?.Dispose();
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingPositionMarker(m_CreatedPingDecals[playerIndex]);
			});
		}
		else
		{
			m_PlayerPingData[player] = new PingData();
		}
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(position).node;
		if (TurnController.IsInTurnBasedCombat())
		{
			position = customGridNodeBase.Vector3Position;
		}
		PingData pingData = m_PlayerPingData[player];
		DrawDecalAt(m_CreatedPingDecals[playerIndex], customGridNodeBase, position);
		EventBus.RaiseEvent(delegate(INetPingPosition h)
		{
			h.HandlePingPositionSound(m_CreatedPingDecals[playerIndex]);
		});
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingPositionMarker(m_CreatedPingDecals[playerIndex]);
		});
		pingData.PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			SetDecalVisibility(m_CreatedPingDecals[playerIndex], isVisible: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingPositionMarker(m_CreatedPingDecals[playerIndex]);
			});
		}, 7.5f);
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
	}

	private void ClearActivePath()
	{
		m_PathRenderer.Show(null, null, pathUnableStatus: false, default(Vector3));
		m_LastShownPosition = null;
	}

	private void SetupActivePath(List<GraphNode> nodes, bool pathUnableStatus, Vector3 meshOffset, UnitMovementAgentBase movementAgent)
	{
		m_PathRenderer.Show(nodes, (movementAgent != null) ? movementAgent.transform : null, pathUnableStatus, meshOffset);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_LastShownPosition = null;
	}
}
