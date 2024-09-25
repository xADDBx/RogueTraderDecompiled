using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.AI;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility.Locator;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class PathfindingService : IService
{
	public class Options
	{
		public IPathModifier[] Modifiers;
	}

	public struct PostProcessData
	{
		public float ApproachRadius;

		public PostProcessData(float approachRadius)
		{
			ApproachRadius = approachRadius;
		}
	}

	private struct WarhammerPathChargeCacheEntry
	{
		public EntityRef Entity;

		public WarhammerPathCharge Path;

		public Vector3 Origin;

		public Vector3 Destination;

		public bool IgnoreBlockers;

		public EntityRef TargetEntity;
	}

	private readonly List<(Path path, Action<Path> callback, int delayToStep)> m_DelayedPaths = new List<(Path, Action<Path>, int)>();

	private static ServiceProxy<PathfindingService> s_InstanceProxy;

	private readonly WarhammerPathPlayerCache m_WarhammerPathPlayerCache = new WarhammerPathPlayerCache();

	private Queue<WarhammerPathChargeCacheEntry> m_ChargePathCache = new Queue<WarhammerPathChargeCacheEntry>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static PathfindingService Instance
	{
		get
		{
			if (s_InstanceProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new PathfindingService());
				s_InstanceProxy = Services.GetProxy<PathfindingService>();
			}
			return s_InstanceProxy.Instance;
		}
	}

	public void ForceCompleteAll()
	{
		PFLog.Pathfinding.Log($"Force complete all paths... x{m_DelayedPaths.Count}");
		int num = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		while (0 < m_DelayedPaths.Count)
		{
			List<(Path path, Action<Path> callback, int delayToStep)> delayedPaths = m_DelayedPaths;
			int num2 = delayedPaths[delayedPaths.Count - 1].delayToStep + 1;
			for (int i = num; i < num2; i++)
			{
				TickInternal(i);
			}
			num = num2;
		}
	}

	public void Tick()
	{
		int currentSystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		TickInternal(currentSystemStepIndex);
	}

	private void TickInternal(int currentSytemStep)
	{
		using (ProfileScope.New("PathindingService.Tick"))
		{
			int num = 0;
			for (int i = 0; i < m_DelayedPaths.Count; i++)
			{
				(Path, Action<Path>, int) tuple = m_DelayedPaths[i];
				if (tuple.Item3 > currentSytemStep)
				{
					break;
				}
				if (!tuple.Item1.IsDoneAndPostProcessed())
				{
					using (ProfileScope.New("BlockUntilCalculated"))
					{
						AstarPath.BlockUntilCalculated(tuple.Item1);
					}
				}
				try
				{
					using (ProfileScope.New("Invoke callback"))
					{
						tuple.Item2?.Invoke(tuple.Item1);
					}
				}
				catch (Exception exception)
				{
					PFLog.Pathfinding.ExceptionWithReport(exception, "Exception occured in delayed path callback");
				}
				if (tuple.Item2 != null)
				{
					tuple.Item1.Release(this);
				}
				num++;
			}
			m_DelayedPaths.RemoveRange(0, num);
		}
	}

	private static void PreProcess(Path path, Options options)
	{
		if (options.Modifiers == null)
		{
			return;
		}
		using (ProfileScope.New("PathfindingService.PreProcess"))
		{
			IPathModifier[] modifiers = options.Modifiers;
			for (int i = 0; i < modifiers.Length; i++)
			{
				modifiers[i].PreProcess(path);
			}
		}
	}

	private static Vector3[] FindLineSphereIntersections(Vector3 linePoint0, Vector3 linePoint1, Vector3 circleCenter, float circleRadius)
	{
		float x = circleCenter.x;
		float y = circleCenter.y;
		float z = circleCenter.z;
		float x2 = linePoint0.x;
		float y2 = linePoint0.y;
		float z2 = linePoint0.z;
		float num = linePoint1.x - x2;
		float num2 = linePoint1.y - y2;
		float num3 = linePoint1.z - z2;
		float num4 = num * num + num2 * num2 + num3 * num3;
		if (Mathf.Approximately(num4, 0f))
		{
			return Array.Empty<Vector3>();
		}
		float num5 = 2f * (x2 * num + y2 * num2 + z2 * num3 - num * x - num2 * y - num3 * z);
		float num6 = x2 * x2 - 2f * x2 * x + x * x + y2 * y2 - 2f * y2 * y + y * y + z2 * z2 - 2f * z2 * z + z * z - circleRadius * circleRadius;
		float num7 = num5 * num5 - 4f * num4 * num6;
		float num8 = (0f - num5 - Mathf.Sqrt(num7)) / (2f * num4);
		Vector3 vector = new Vector3(linePoint0.x * (1f - num8) + num8 * linePoint1.x, linePoint0.y * (1f - num8) + num8 * linePoint1.y, linePoint0.z * (1f - num8) + num8 * linePoint1.z);
		float num9 = (0f - num5 + Mathf.Sqrt(num7)) / (2f * num4);
		Vector3 vector2 = new Vector3(linePoint0.x * (1f - num9) + num9 * linePoint1.x, linePoint0.y * (1f - num9) + num9 * linePoint1.y, linePoint0.z * (1f - num9) + num9 * linePoint1.z);
		if (num7 < 0f)
		{
			return Array.Empty<Vector3>();
		}
		if ((num8 > 1f || num8 < 0f) && (num9 > 1f || num9 < 0f))
		{
			return Array.Empty<Vector3>();
		}
		if (!(num8 > 1f) && !(num8 < 0f) && (num9 > 1f || num9 < 0f))
		{
			return new Vector3[1] { vector };
		}
		if ((num8 > 1f || num8 < 0f) && !(num9 > 1f) && !(num9 < 0f))
		{
			return new Vector3[1] { vector2 };
		}
		if (num7 != 0f)
		{
			return new Vector3[2] { vector, vector2 };
		}
		return new Vector3[1] { vector };
	}

	private static Vector3 GetNewEndPoint(WarhammerXPath xPath, float approachRadius)
	{
		Vector3[] array = Array.Empty<Vector3>();
		if (xPath.vectorPath.Count > 2)
		{
			List<Vector3> vectorPath = xPath.vectorPath;
			Vector3 linePoint = vectorPath[vectorPath.Count - 2];
			List<Vector3> vectorPath2 = xPath.vectorPath;
			Vector3[] array2 = FindLineSphereIntersections(linePoint, vectorPath2[vectorPath2.Count - 1], xPath.originalEndPoint, approachRadius);
			if (array2.Length != 0)
			{
				array = array2;
			}
		}
		else
		{
			Vector3 originalStartPoint = xPath.originalStartPoint;
			List<Vector3> vectorPath3 = xPath.vectorPath;
			array = FindLineSphereIntersections(originalStartPoint, vectorPath3[vectorPath3.Count - 1], xPath.originalEndPoint, approachRadius);
		}
		if (array.Length == 1)
		{
			return array[0];
		}
		if (array.Length == 2)
		{
			List<Vector3> vectorPath4 = xPath.vectorPath;
			float magnitude = (vectorPath4[vectorPath4.Count - 2] - array[0]).magnitude;
			List<Vector3> vectorPath5 = xPath.vectorPath;
			if (!(magnitude <= (vectorPath5[vectorPath5.Count - 2] - array[1]).magnitude))
			{
				return array[1];
			}
			return array[0];
		}
		List<Vector3> vectorPath6 = xPath.vectorPath;
		return vectorPath6[vectorPath6.Count - 1];
	}

	private static void PostProcess(Path path, PostProcessData? data, Options options)
	{
		if (options.Modifiers != null)
		{
			using (ProfileScope.New("PathfindingService.PostProcess"))
			{
				IPathModifier[] modifiers = options.Modifiers;
				for (int i = 0; i < modifiers.Length; i++)
				{
					modifiers[i].Apply(path);
				}
			}
		}
		if (data.HasValue && !Game.Instance.TurnController.TurnBasedModeActive && path is WarhammerXPath warhammerXPath)
		{
			Vector3 value = (warhammerXPath.endPoint = GetNewEndPoint(warhammerXPath, data.Value.ApproachRadius));
			List<Vector3> vectorPath = warhammerXPath.vectorPath;
			vectorPath[vectorPath.Count - 1] = value;
		}
	}

	public void FindPath<TPath>(TPath path, PostProcessData? data, Options options, Action<ForcedPath> callback) where TPath : Path
	{
		if (callback != null)
		{
			path.Claim(this);
		}
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		path.enabledTags = 1;
		path.callback = delegate(Path p)
		{
			OnForcedPathCompleteInternal<TPath>(p as TPath, data, options, callback);
		};
		PreProcess(path, options);
		AstarPath.StartPath(path);
	}

	public void FindPathWithType<TPath>(TPath path, PostProcessData? data, Options options, Action<TPath> callback) where TPath : Path
	{
		if (callback != null)
		{
			path.Claim(this);
		}
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		path.enabledTags = 1;
		path.callback = delegate(Path p)
		{
			OnPathCompleteInternal(p, data, options, callback);
		};
		PreProcess(path, options);
		AstarPath.StartPath(path);
	}

	private void OnPathCompleteInternal<TPath>(Path path, PostProcessData? data, Options options, Action<TPath> callback) where TPath : Path
	{
		if (!path.error)
		{
			PostProcess(path, data, options);
		}
		if (callback != null)
		{
			try
			{
				callback(path as TPath);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
			path.Release(this);
		}
	}

	private void OnForcedPathCompleteInternal<TPath>(Path path, PostProcessData? data, Options options, Action<ForcedPath> callback) where TPath : Path
	{
		if (!path.error)
		{
			PostProcess(path, data, options);
		}
		if (callback != null)
		{
			ForcedPath obj = (path.error ? ForcedPath.ErrorPath : ForcedPath.Construct(path));
			try
			{
				callback(obj);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
			path.Release(this);
		}
	}

	private static void FindPath_Blocking(Path path, Options options)
	{
		path.enabledTags = 1;
		path.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		PreProcess(path, options);
		AstarPath.StartPath(path);
		AstarPath.BlockUntilCalculated(path);
		PostProcess(path, null, options);
	}

	private ABPath ConstructPath(UnitMovementAgentBase agent, Vector3 destination, float approachRadius)
	{
		if (approachRadius > 0.35f)
		{
			WarhammerXPath warhammerXPath = WarhammerXPath.Construct(agent.Position, destination);
			warhammerXPath.nnConstraint.constrainArea = true;
			warhammerXPath.endingCondition = new LosPathCondition(warhammerXPath, approachRadius - 0.2f, checkLos: false);
			warhammerXPath.LinkTraversalProvider = agent.NodeLinkTraverser;
			return warhammerXPath;
		}
		WarhammerABPath warhammerABPath = WarhammerABPath.Construct(agent.Position, destination);
		warhammerABPath.nnConstraint.constrainArea = true;
		warhammerABPath.LinkTraversalProvider = agent.NodeLinkTraverser;
		return warhammerABPath;
	}

	public ABPath FindPathRT(UnitMovementAgentBase agent, Vector3 destination, float approachRadius, [NotNull] Action<ForcedPath> callback)
	{
		return FindPathRT_Delayed(agent, destination, approachRadius, 1, callback);
	}

	public Task<ForcedPath> FindPathRTAsync(UnitMovementAgentBase agent, Vector3 destination, float approachRadius)
	{
		TaskCompletionSource<ForcedPath> completionSource = new TaskCompletionSource<ForcedPath>();
		FindPathRT(agent, destination, approachRadius, delegate(ForcedPath path)
		{
			completionSource.SetResult(path);
		});
		return completionSource.Task;
	}

	public ABPath FindPathRT_Delayed(UnitMovementAgentBase agent, Vector3 destination, float approachRadius, int delaySteps, [NotNull] Action<ForcedPath> callback)
	{
		ABPath aBPath = ConstructPath(agent, destination, approachRadius);
		aBPath.Claim(this);
		FindPathWithType(aBPath, new PostProcessData(approachRadius), agent.RealTimeOptions, null);
		AddDelayedPath((path: aBPath, callback: delegate(Path p)
		{
			ForcedPath obj = ForcedPath.Construct(p);
			try
			{
				callback(obj);
			}
			catch (Exception ex)
			{
				PFLog.Pathfinding.Exception(ex);
			}
		}, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return aBPath;
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed(UnitMovementAgentBase agent, Vector3 destination, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed(UnitMovementAgentBase agent, MechanicEntity targetEntity, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), null, targetEntity, ignoreThreateningAreaCost: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public PathDisposable<WarhammerPathPlayer> FindPathTB_Delayed(UnitMovementAgentBase agent, TargetWrapper target, bool limitRangeByActionPoints, int delaySteps, object owner)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), target.Point, target.Entity, ignoreThreateningAreaCost: false);
		FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathPlayer, callback: null, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + delaySteps));
		return PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, owner);
	}

	public ForcedPath FindPathRT_Blocking(UnitMovementAgentBase agent, Vector3 destination, float approachRadius)
	{
		ABPath aBPath = ConstructPath(agent, destination, approachRadius);
		aBPath.Claim(this);
		FindPath_Blocking(aBPath, agent.RealTimeOptions);
		if (aBPath.error)
		{
			aBPath.Release(this);
			return ForcedPath.ErrorPath;
		}
		ForcedPath result = ForcedPath.Construct(aBPath);
		aBPath.Release(this);
		return result;
	}

	private WarhammerPathPlayer ConstructWhPlayerPath(UnitMovementAgentBase agent, Vector3 start, float maxLength, Vector3? destination, MechanicEntity targetEntity, bool ignoreThreateningAreaCost, bool ignoreBlockers = false)
	{
		CustomGridNode targetNode = ((targetEntity != null) ? null : (destination.HasValue ? ObstacleAnalyzer.GetNearestNodeXZUnwalkable(destination.Value) : null));
		ICollection<GraphNode> collection2;
		if (!ignoreThreateningAreaCost)
		{
			ICollection<GraphNode> collection = UnitMovementAgentBase.CacheThreateningAreaCells(agent.Unit.Data);
			collection2 = collection;
		}
		else
		{
			ICollection<GraphNode> collection = Array.Empty<GraphNode>();
			collection2 = collection;
		}
		ICollection<GraphNode> threateningAreaCells = collection2;
		Dictionary<GraphNode, float> overrideCosts = NodeTraverseCostHelper.GetOverrideCosts(agent.Unit.Data);
		WarhammerPathPlayerMetricCostProvider traversalCostProvider = new WarhammerPathPlayerMetricCostProvider(agent.Unit.Data, maxLength, targetNode, targetEntity, threateningAreaCells, overrideCosts);
		WarhammerPathPlayerMetric initialLength = new WarhammerPathPlayerMetric(agent.Unit.Data.GetCombatStateOptional()?.LastDiagonalCount ?? 0, 0f);
		WarhammerPathPlayer warhammerPathPlayer = WarhammerPathPlayer.Construct(start, ignoreBlockers ? BlockMode.Ignore : agent.Unit.BlockMode, initialLength, traversalCostProvider);
		warhammerPathPlayer.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		warhammerPathPlayer.traversalProvider = agent.TraversalProvider;
		warhammerPathPlayer.LinkTraversalProvider = agent.NodeLinkTraverser;
		return warhammerPathPlayer;
	}

	private WarhammerPathCharge ConstructWhChargePath(UnitMovementAgentBase agent, Vector3 start, Vector3 destination, bool ignoreBlockers, [CanBeNull] MechanicEntity targetEntity)
	{
		BaseUnitEntity unit = (agent.Unit.Data as BaseUnitEntity) ?? throw new Exception("Only an entity based on BaseUnitEntity can use ConstructWhChargePath.");
		CustomGridNodeBase nearestNodeXZUnwalkable = start.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = destination.GetNearestNodeXZUnwalkable();
		WarhammerPathChargeMetricCostProvider traversalCostProvider = new WarhammerPathChargeMetricCostProvider(unit, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2, targetEntity);
		WarhammerPathCharge obj = WarhammerPathCharge.Construct(initialLength: new WarhammerPathChargeMetric(0f, 0), start: start, blockMode: ignoreBlockers ? BlockMode.Ignore : agent.Unit.BlockMode, traversalCostProvider: traversalCostProvider);
		obj.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		obj.traversalProvider = agent.TraversalProvider;
		obj.persistentPath = true;
		return obj;
	}

	private WarhammerPathAi ConstructWarhammerPathAi(UnitMovementAgentBase agent, Vector3 start, WarhammerPathAiMetric initialLength, ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell> traversalCostProvider)
	{
		WarhammerPathAi warhammerPathAi = WarhammerPathAi.Construct(start, agent.Unit.BlockMode, initialLength, traversalCostProvider);
		warhammerPathAi.nnConstraint = new ConstraintWithRespectToTraversalProvider(agent.Blocker);
		warhammerPathAi.traversalProvider = agent.TraversalProvider;
		warhammerPathAi.LinkTraversalProvider = agent.NodeLinkTraverser;
		return warhammerPathAi;
	}

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgentBase agent, Vector3 destination, bool limitRangeByActionPoints = true, bool ignoreThreateningAreaCost = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgentBase agent, MechanicEntity targetEntity, bool limitRangeByActionPoints = true)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), null, targetEntity, ignoreThreateningAreaCost: false);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgentBase agent, Vector3 origin, Vector3 destination, bool limitRangeByActionPoints = true, bool ignoreThreateningAreaCost = false, bool ignoreBlockers = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, origin, limitRangeByActionPoints ? ((int)(agent.Unit.Data.GetCombatStateOptional()?.ActionPointsBlue ?? (-1f))) : (-1), destination, null, ignoreThreateningAreaCost, ignoreBlockers);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathPlayer FindPathTB_Blocking_Cached(UnitMovementAgentBase agent, Vector3 origin, Vector3 destination, float maxLength, bool ignoreThreateningAreaCost = false, bool ignoreBlockers = false)
	{
		return m_WarhammerPathPlayerCache.FindPathTB_Blocking(agent, origin, destination, maxLength, ignoreThreateningAreaCost, ignoreBlockers);
	}

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgentBase agent, Vector3 origin, Vector3 destination, float maxLength, bool ignoreThreateningAreaCost = false, bool ignoreBlockers = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, origin, maxLength, destination, null, ignoreThreateningAreaCost, ignoreBlockers);
		using (ProfileScope.New("FindPathTB"))
		{
			FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
			return warhammerPathPlayer;
		}
	}

	public WarhammerPathCharge FindPathChargeTB_Blocking(UnitMovementAgentBase agent, Vector3 origin, Vector3 destination, bool ignoreBlockers, [CanBeNull] MechanicEntity targetEntity)
	{
		WarhammerPathCharge warhammerPathCharge = ConstructWhChargePath(agent, origin, destination, ignoreBlockers, targetEntity);
		foreach (WarhammerPathChargeCacheEntry item in m_ChargePathCache)
		{
			if (item.Entity.Id == agent.Unit?.UniqueId && item.Origin == origin && item.Destination == destination && item.IgnoreBlockers == ignoreBlockers)
			{
				EntityRef targetEntity2 = item.TargetEntity;
				if ((targetEntity2.IsEmpty && targetEntity == null) || item.TargetEntity.Id == targetEntity?.UniqueId)
				{
					return item.Path;
				}
			}
		}
		using (ProfileScope.New("FindPathChargeTB"))
		{
			FindPath_Blocking(warhammerPathCharge, agent.TurnBasedOptions);
			if (m_ChargePathCache.Count >= 5)
			{
				PathPool.Pool(m_ChargePathCache.Dequeue().Path);
			}
			m_ChargePathCache.Enqueue(new WarhammerPathChargeCacheEntry
			{
				Entity = agent.Unit.Data.Ref,
				Path = warhammerPathCharge,
				Origin = origin,
				Destination = destination,
				IgnoreBlockers = ignoreBlockers,
				TargetEntity = (((EntityRef?)targetEntity?.Ref) ?? default(EntityRef))
			});
			return warhammerPathCharge;
		}
	}

	public Task<PathDisposable<WarhammerPathPlayer>> FindPathTB_Task(UnitMovementAgentBase agent, Vector3 destination, int maxTiles, object pathOwner)
	{
		WarhammerPathPlayer warhammerPathPlayer = null;
		try
		{
			warhammerPathPlayer = ConstructWhPlayerPath(agent, agent.Unit.Data.Position, maxTiles, destination, null, ignoreThreateningAreaCost: false);
			TaskCompletionSource<PathDisposable<WarhammerPathPlayer>> tcs = new TaskCompletionSource<PathDisposable<WarhammerPathPlayer>>();
			FindPathWithType(warhammerPathPlayer, null, agent.TurnBasedOptions, delegate(WarhammerPathPlayer path)
			{
				tcs.SetResult(PathDisposable<WarhammerPathPlayer>.Get(path, pathOwner));
			});
			return tcs.Task;
		}
		catch (Exception ex)
		{
			warhammerPathPlayer?.Claim(this);
			warhammerPathPlayer?.Release(this);
			PFLog.Default.Exception(ex, "Exception during FindPathTbAsync");
			return null;
		}
	}

	public Dictionary<GraphNode, WarhammerPathPlayerCell> FindAllReachableTiles_Blocking(UnitMovementAgentBase agent, Vector3 start, float maxLength, bool ignoreThreateningAreaCost = false)
	{
		WarhammerPathPlayer warhammerPathPlayer = ConstructWhPlayerPath(agent, start, maxLength, null, null, ignoreThreateningAreaCost);
		using (ProfileScope.New("AFindPathTB"))
		{
			using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
			{
				FindPath_Blocking(warhammerPathPlayer, agent.TurnBasedOptions);
				return warhammerPathPlayer.GetAllNodesAndReset();
			}
		}
	}

	public Task<Dictionary<GraphNode, WarhammerPathAiCell>> FindAllReachableTiles_Delayed_Task(UnitMovementAgentBase agent, Vector3 start, int maxTiles, Dictionary<GraphNode, AiBrainHelper.ThreatsInfo> threateningAreaCells)
	{
		TaskCompletionSource<Dictionary<GraphNode, WarhammerPathAiCell>> tcs = new TaskCompletionSource<Dictionary<GraphNode, WarhammerPathAiCell>>();
		if (!UnitMovementAgentBase.AllAgents.Contains(agent))
		{
			PFLog.Default.Error($"Cant find reachable tiles for disabled UnitMovementAgentBase: {agent}");
			tcs.SetResult(null);
			return tcs.Task;
		}
		WarhammerPathAiMetricCostProvider traversalCostProvider = new WarhammerPathAiMetricCostProvider(agent.Unit.Data, maxTiles, threateningAreaCells);
		WarhammerPathAiMetric initialLength = new WarhammerPathAiMetric(agent.Unit.Data.GetCombatStateOptional()?.LastDiagonalCount ?? 0, 0f, 0f, 0, 0, 0);
		WarhammerPathAi warhammerPathAi = ConstructWarhammerPathAi(agent, start, initialLength, traversalCostProvider);
		warhammerPathAi.Claim(this);
		Action<Path> item = delegate(Path p)
		{
			Dictionary<GraphNode, WarhammerPathAiCell> result = ((p != null && !p.error && p is WarhammerPathAi warhammerPathAi2) ? warhammerPathAi2.GetAllNodesAndReset() : null);
			tcs.SetResult(result);
		};
		FindPathWithType(warhammerPathAi, null, agent.TurnBasedOptions, null);
		AddDelayedPath((path: warhammerPathAi, callback: item, delayToStep: Game.Instance.RealTimeController.CurrentSystemStepIndex + 1));
		return tcs.Task;
	}

	private void AddDelayedPath((Path path, Action<Path> callback, int delayToStep) value)
	{
		int index = 0;
		int num = m_DelayedPaths.Count - 1;
		while (0 <= num)
		{
			if (m_DelayedPaths[num].delayToStep <= value.delayToStep)
			{
				index = num + 1;
				break;
			}
			num--;
		}
		m_DelayedPaths.Insert(index, value);
	}
}
