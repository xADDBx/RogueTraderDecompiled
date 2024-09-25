using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipNavigation : StarshipPart, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>, IAreaHandler, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipNavigation>, IEntityPartOwner
	{
		PartStarshipNavigation Navigation { get; }
	}

	public enum SpeedModeType
	{
		Normal,
		Deccelerating,
		LowSpeed,
		FullStop
	}

	private struct StarshipPathParameters
	{
		private int inertia;

		private int maxLength;

		private int lastStraightMoveLength;

		private int lastDiagonalCount;

		private int direction;

		private int nodeIndex;

		private ShipPath.TurnAngleType[] turnAngles;

		public StarshipPathParameters(PartStarshipNavigation navigation)
		{
			inertia = navigation.PartStarship.Inertia;
			maxLength = (int)navigation.CombatState.ActionPointsBlue;
			lastStraightMoveLength = navigation.CombatState.LastStraightMoveLength;
			lastDiagonalCount = navigation.CombatState.LastDiagonalCount;
			direction = CustomGraphHelper.GuessDirection(navigation.Owner.Forward);
			nodeIndex = AstarPath.active.GetNearest(navigation.Owner.Position).node.NodeIndex;
			turnAngles = navigation.turnAngles;
		}

		public override bool Equals(object obj)
		{
			if (obj is StarshipPathParameters parameters)
			{
				return Equals(parameters);
			}
			return false;
		}

		public bool Equals(StarshipPathParameters parameters)
		{
			if (inertia == parameters.inertia && maxLength == parameters.maxLength && lastStraightMoveLength == parameters.lastStraightMoveLength && lastDiagonalCount == parameters.lastDiagonalCount && direction == parameters.direction && nodeIndex == parameters.nodeIndex)
			{
				return EqualityComparer<ShipPath.TurnAngleType[]>.Default.Equals(turnAngles, parameters.turnAngles);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(inertia, maxLength, lastStraightMoveLength, lastDiagonalCount, direction, nodeIndex, turnAngles);
		}

		public static bool operator ==(StarshipPathParameters a, StarshipPathParameters b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(StarshipPathParameters a, StarshipPathParameters b)
		{
			return !a.Equals(b);
		}
	}

	private struct ReachableTilesArgs
	{
		public int Inertia;

		public int MovementPoints;

		public int LastStraightMoveLength;

		public int LastDiagonalCount;

		public int Direction;
	}

	private StarshipPathParameters m_LastPathParameters;

	[JsonProperty]
	private SpeedModeType m_SpeedMode;

	private ShipPath m_ReachableTiles;

	private WarhammerSingleNodeBlocker nodeBlocker;

	private WarhammerTraversalProvider traversalProvider;

	private ShipPath.TurnAngleType[] turnAngles;

	private static readonly int[] OppositeDirections = new int[8] { 2, 3, 0, 1, 6, 7, 4, 5 };

	public SpeedModeType SpeedMode
	{
		get
		{
			return m_SpeedMode;
		}
		set
		{
			if (m_SpeedMode != value)
			{
				int currentSpeed = CurrentSpeed;
				m_SpeedMode = value;
				int currentSpeed2 = CurrentSpeed;
				float bluePoint = Math.Max(CombatState.ActionPointsBlue + (float)currentSpeed2 - (float)currentSpeed, 0f);
				CombatState.SetBluePoint(bluePoint);
			}
		}
	}

	public bool CanTurn90Degrees => base.Owner.Facts.HasComponent<StarshipMovementVariation>((EntityFact x) => x.Components.Find((EntityFactComponent y) => y.SourceBlueprintComponent is StarshipMovementVariation) != null);

	public bool IsSuicideAttacker => base.Owner.Facts.HasComponent<SuicideAttacker>((EntityFact x) => x.Components.Find((EntityFactComponent y) => y.SourceBlueprintComponent is SuicideAttacker) != null);

	public bool IsSoftUnit => base.Owner.Blueprint.IsSoftUnit;

	private PartStarship PartStarship => base.Owner.GetRequired<PartStarship>();

	private PartUnitCombatState CombatState => base.Owner.GetRequired<PartUnitCombatState>();

	private UnitMovementAgentBase MovementAgent => base.Owner.View?.MovementAgent;

	public int CurrentSpeed => SpeedMode switch
	{
		SpeedModeType.Deccelerating => (int)CombatState.WarhammerInitialAPBlue * 2 / 3, 
		SpeedModeType.LowSpeed => ((int)CombatState.WarhammerInitialAPBlue + 1) / 2, 
		SpeedModeType.FullStop => 0, 
		_ => CombatState.WarhammerInitialAPBlue, 
	};

	private BlueprintItemPlasmaDrives MaybePlasmaDrives => base.Owner.Hull.HullSlots.PlasmaDrives.MaybeItem?.Blueprint as BlueprintItemPlasmaDrives;

	public int FinishingTilesCount
	{
		get
		{
			if (SpeedMode != 0 && SpeedMode != SpeedModeType.Deccelerating)
			{
				return 1;
			}
			return MaybePlasmaDrives?.FinishPhase ?? 3;
		}
	}

	public int PushPhaseTilesCount
	{
		get
		{
			if (SpeedMode != 0)
			{
				return -1;
			}
			return MaybePlasmaDrives?.PushPhase ?? (-1);
		}
	}

	public bool IsEndingMovementPhase => CombatState.ActionPointsBlue < (float)FinishingTilesCount;

	public bool IsAccelerationMovementPhase => CombatState.ActionPointsBlueSpentThisTurn <= (float)PushPhaseTilesCount;

	public HashSet<ShipPath.DirectionalPathNode> RawReachableTiles => ReachableTiles?.RawResult ?? null;

	public bool HasAnotherPlaceToStand => (ReachableTiles?.Result?.Count((KeyValuePair<GraphNode, ShipPath.PathCell> x) => x.Value.CanStand)).GetValueOrDefault() > 1;

	public ShipPath ReachableTiles
	{
		get
		{
			return m_ReachableTiles;
		}
		private set
		{
			m_ReachableTiles?.Release(this);
			m_ReachableTiles = value;
			m_ReachableTiles?.Claim(this);
		}
	}

	[Cheat(Name = "set_starship_speed_mode")]
	public static void SetSpeedMode(string cond)
	{
		if (Enum.TryParse<SpeedModeType>(cond, out var result))
		{
			Game.Instance.Player.PlayerShip.Navigation.SpeedMode = result;
		}
	}

	public bool CanStand(GraphNode node, int direction)
	{
		return traversalProvider.CanTraverseEndNode(node, direction);
	}

	public void OverrideTurnAngles(ShipPath.TurnAngleType[] turnAngles)
	{
		this.turnAngles = turnAngles;
	}

	public void ResetCustomOverrides()
	{
		turnAngles = null;
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (SpeedMode == SpeedModeType.FullStop)
		{
			SpeedMode = SpeedModeType.LowSpeed;
		}
		else if (SpeedMode == SpeedModeType.Deccelerating)
		{
			SpeedMode = SpeedModeType.LowSpeed;
		}
		else if (SpeedMode == SpeedModeType.LowSpeed)
		{
			SpeedMode = SpeedModeType.Normal;
		}
	}

	protected override void OnViewDidAttach()
	{
		nodeBlocker = MovementAgent.Blocker;
		traversalProvider = new WarhammerTraversalProvider(nodeBlocker, base.Owner.SizeRect, base.Owner.Faction.IsPlayerEnemy, IsSoftUnit);
	}

	protected override void OnViewWillDetach()
	{
		ReachableTiles = null;
	}

	public bool ShouldUpdateReachableTiles()
	{
		StarshipPathParameters starshipPathParameters = new StarshipPathParameters(this);
		return m_LastPathParameters != starshipPathParameters;
	}

	public void UpdateReachableTiles(OnPathDelegate callback, bool unitMaxMovePoints = false, bool isDeadEnd = false)
	{
		if (!MovementAgent.IsReallyMoving)
		{
			ReachableTilesArgs reachableTilesArgs = default(ReachableTilesArgs);
			reachableTilesArgs.Inertia = PartStarship.Inertia;
			reachableTilesArgs.MovementPoints = (unitMaxMovePoints ? ((int)CombatState.ActionPointsBlueMax) : ((int)CombatState.ActionPointsBlue));
			reachableTilesArgs.LastStraightMoveLength = ((!unitMaxMovePoints) ? CombatState.LastStraightMoveLength : 0);
			reachableTilesArgs.LastDiagonalCount = CombatState.LastDiagonalCount;
			reachableTilesArgs.Direction = CustomGraphHelper.GuessDirection(base.Owner.Forward);
			ReachableTilesArgs data = reachableTilesArgs;
			if (isDeadEnd)
			{
				data.Inertia = 0;
				data.MovementPoints = 1;
			}
			ReachableTiles = FindReachableTiles(callback, data);
			m_LastPathParameters = new StarshipPathParameters(this);
		}
	}

	public void UpdateReachableTiles_Blocking()
	{
		UpdateReachableTiles(delegate
		{
		});
		AstarPath.BlockUntilCalculated(ReachableTiles);
	}

	public PathDisposable<ShipPath> FindReachableTiles_Blocking(object pathOwner, bool unitMaxMovePoints = false)
	{
		ReachableTilesArgs reachableTilesArgs = default(ReachableTilesArgs);
		reachableTilesArgs.Inertia = PartStarship.Inertia;
		reachableTilesArgs.MovementPoints = (unitMaxMovePoints ? ((int)CombatState.ActionPointsBlueMax) : ((int)CombatState.ActionPointsBlue));
		reachableTilesArgs.LastStraightMoveLength = ((!unitMaxMovePoints) ? CombatState.LastStraightMoveLength : 0);
		reachableTilesArgs.LastDiagonalCount = CombatState.LastDiagonalCount;
		reachableTilesArgs.Direction = CustomGraphHelper.GuessDirection(base.Owner.Forward);
		ReachableTilesArgs data = reachableTilesArgs;
		ShipPath path = FindReachableTiles(delegate
		{
		}, data);
		PathDisposable<ShipPath> result = PathDisposable<ShipPath>.Get(path, pathOwner);
		AstarPath.BlockUntilCalculated(path);
		return result;
	}

	private ShipPath FindReachableTiles(OnPathDelegate callback, ReachableTilesArgs data)
	{
		ShipPath path = ShipPath.Construct(base.Owner.Position, data.Direction, data.LastStraightMoveLength, data.LastDiagonalCount, data.Inertia, data.MovementPoints, base.Owner.SizeRect, turnAngles);
		path.nnConstraint = new ConstraintWithRespectToTraversalProvider(nodeBlocker);
		path.traversalProvider = traversalProvider;
		path.persistentPath = true;
		path.callback = Callback;
		PathfindingService.Instance.FindPathWithType(path, null, MovementAgent.TurnBasedOptions, delegate(ShipPath p)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else
			{
				Callback(p);
			}
		});
		m_LastPathParameters = new StarshipPathParameters(this);
		return path;
		void Callback(Path p)
		{
			try
			{
				path.Claim(p);
				PostProcessReachableTiles(p);
				callback(p);
			}
			finally
			{
				p.Release(p);
			}
		}
	}

	public ForcedPath FindPath(Vector3 destination)
	{
		if (ReachableTiles == null)
		{
			using PathDisposable<ShipPath> pathDisposable = FindReachableTiles_Blocking(base.Owner);
			ReachableTiles = pathDisposable.Path;
		}
		CustomGridNodeBase currentNode = (CustomGridNodeBase)AstarPath.active.GetNearest(destination).node;
		CustomGridNodeBase startNode = (CustomGridNodeBase)AstarPath.active.GetNearest(base.Owner.Position).node;
		currentNode = GetNodeInMetagrid(startNode, base.Owner.SizeRect, currentNode);
		if (!ReachableTiles.Result.TryGetValue(currentNode, out var value))
		{
			return null;
		}
		if (!value.CanStand && !IsSuicideAttacker)
		{
			return null;
		}
		int length = value.Length;
		List<Vector3> list = new List<Vector3>();
		List<GraphNode> list2 = new List<GraphNode>();
		int num = value.Direction;
		if (length > 0)
		{
			while (true)
			{
				list.Add(currentNode.Vector3Position);
				list2.Add(currentNode);
				int num2 = value.ParentDirections[num];
				if (num2 < 0)
				{
					break;
				}
				currentNode = GetNodeAlongDirection(currentNode, OppositeDirections[num]);
				if (currentNode == null || !ReachableTiles.Result.TryGetValue(currentNode, out value))
				{
					break;
				}
				num = num2;
			}
		}
		list.Reverse();
		list2.Reverse();
		return ForcedPath.Construct(list, list2);
	}

	private CustomGridNodeBase GetNodeAlongDirection(CustomGridNodeBase sourceNode, int direction)
	{
		CustomGridNodeBase customGridNodeBase = sourceNode;
		for (int i = 0; i < base.Owner.SizeRect.Width; i++)
		{
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(direction);
		}
		return customGridNodeBase;
	}

	private CustomGridNodeBase GetNodeInMetagrid(CustomGridNodeBase startNode, IntRect sizeRect, CustomGridNodeBase currentNode)
	{
		int width = sizeRect.Width;
		CustomGridGraph obj = (CustomGridGraph)currentNode.Graph;
		int xmin = sizeRect.xmin;
		int ymin = sizeRect.ymin;
		startNode = obj.GetNode(startNode.XCoordinateInGrid + xmin, startNode.ZCoordinateInGrid + ymin);
		int num = -startNode.XCoordinateInGrid % width;
		int num2 = -startNode.ZCoordinateInGrid % width;
		currentNode = obj.GetNode((currentNode.XCoordinateInGrid + num) / width * width - num - xmin, (currentNode.ZCoordinateInGrid + num2) / width * width - num2 - ymin);
		return currentNode;
	}

	public Dictionary<GraphNode, ShipPath.PathCell> GetEndNodes()
	{
		Dictionary<GraphNode, ShipPath.PathCell> dictionary = new Dictionary<GraphNode, ShipPath.PathCell>();
		foreach (var (key, value) in ReachableTiles.Result)
		{
			if (ReachableTiles.MaxLength - value.Length < FinishingTilesCount)
			{
				dictionary.Add(key, value);
			}
		}
		return dictionary;
	}

	private void PostProcessReachableTiles(Path path)
	{
		ShipPath shipPath = (ShipPath)path;
		if (IsSoftUnit)
		{
			return;
		}
		foreach (GraphNode item in shipPath.Result.Keys.ToList())
		{
			ShipPath.PathCell value = shipPath.Result[item];
			int direction = value.Direction;
			if (base.Owner.SizeRect.Width == 1 && base.Owner.SizeRect.Height == 2 && direction > 3 && CheckFrigateCrossing(item, direction))
			{
				value.CanStand = false;
				shipPath.Result[item] = value;
			}
		}
	}

	private bool CheckFrigateCrossing(GraphNode node, int direction)
	{
		var (item, item2) = GetCrossedNodesToCheck(node, direction);
		foreach (BaseUnitEntity item3 in Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity u) => u is StarshipEntity && u.Size == Size.Frigate_1x2).ToList())
		{
			if (item3.MovementAgent.Blocker != nodeBlocker)
			{
				NodeList occupiedNodes = item3.GetOccupiedNodes();
				if (occupiedNodes.Contains(item) && occupiedNodes.Contains(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private (CustomGridNodeBase, CustomGridNodeBase) GetCrossedNodesToCheck(GraphNode node, int direction)
	{
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)node;
		CustomGridNodeBase neighbourAlongDirection = customGridNodeBase.GetNeighbourAlongDirection(CustomGraphHelper.OppositeDirections[direction]);
		CustomGridGraph customGridGraph = node.Graph as CustomGridGraph;
		int num = neighbourAlongDirection.XCoordinateInGrid - customGridNodeBase.XCoordinateInGrid;
		int num2 = neighbourAlongDirection.ZCoordinateInGrid - customGridNodeBase.ZCoordinateInGrid;
		CustomGridNodeBase node2 = customGridGraph.GetNode(customGridNodeBase.XCoordinateInGrid + num, customGridNodeBase.ZCoordinateInGrid);
		CustomGridNodeBase node3 = customGridGraph.GetNode(customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid + num2);
		return (node2, node3);
	}

	public void ClearLastPathParameters()
	{
		m_LastPathParameters = default(StarshipPathParameters);
	}

	public void OnAreaBeginUnloading()
	{
		m_LastPathParameters = default(StarshipPathParameters);
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_SpeedMode);
		return result;
	}
}
