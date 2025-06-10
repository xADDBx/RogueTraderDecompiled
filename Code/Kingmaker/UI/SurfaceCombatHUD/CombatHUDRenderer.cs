using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.UniRx;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UI.SurfaceCombatHUD;

public class CombatHUDRenderer : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitMovableAreaHandler, ISubscriber<IMechanicEntity>, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>, IAreaHandler, INetRoleSetHandler
{
	public struct AbilityAreaHudInfo
	{
		public OrientedPatternData pattern;

		public IntRect casterRect;

		public int minRange;

		public int maxRange;

		public int effectiveRange;

		public bool ignoreRangesByDefault;

		public bool ignorePatternPrimaryAreaByDefault;

		public CombatHudCommandSetAsset combatHudCommandsOverride;
	}

	private enum AreaDisplayMode
	{
		Default,
		Movement,
		Ability
	}

	private sealed class StratagemAreaCollection
	{
		private struct Item
		{
			public CombatHubCollectionAreaSource Source;

			public Texture2D Icon;

			public CombatHudMaterialRemapAsset MaterialRemapAsset;
		}

		private readonly ObjectPool<CombatHubCollectionAreaSource> m_SourcesPool;

		private readonly Dictionary<AreaEffectEntity, Item> m_Map = new Dictionary<AreaEffectEntity, Item>();

		public StratagemAreaCollection(ObjectPool<CombatHubCollectionAreaSource> sourcesPool)
		{
			m_SourcesPool = sourcesPool;
		}

		public void GetAreaDataList(List<AreaSourceData> results)
		{
			results.Clear();
			foreach (Item value in m_Map.Values)
			{
				results.Add(new AreaSourceData
				{
					Source = value.Source,
					IconTexture = value.Icon,
					MaterialRemapAsset = value.MaterialRemapAsset
				});
			}
		}

		public void SetupArea(AreaEffectEntity areaEffectEntity, in NodeList patternNodes)
		{
			CombatHubCollectionAreaSource combatHubCollectionAreaSource;
			if (m_Map.TryGetValue(areaEffectEntity, out var value))
			{
				combatHubCollectionAreaSource = value.Source;
				combatHubCollectionAreaSource.Clear();
			}
			else
			{
				combatHubCollectionAreaSource = m_SourcesPool.Get();
			}
			foreach (CustomGridNodeBase patternNode in patternNodes)
			{
				combatHubCollectionAreaSource.Add(patternNode);
			}
			m_Map[areaEffectEntity] = new Item
			{
				Source = combatHubCollectionAreaSource,
				Icon = areaEffectEntity.Blueprint?.PersistentAreaTexture2D,
				MaterialRemapAsset = areaEffectEntity.Blueprint?.PersistentAreaMaterialRemap
			};
		}

		public bool CleanupArea(AreaEffectEntity areaEffectEntity)
		{
			if (m_Map.Remove(areaEffectEntity, out var value))
			{
				m_SourcesPool.Release(value.Source);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			foreach (Item value in m_Map.Values)
			{
				m_SourcesPool.Release(value.Source);
			}
			m_Map.Clear();
		}
	}

	private const int AbilityMaxAllowedRange = 200;

	private const float kHighlightSpaceMovementAreaDuration = 2f;

	[Header("Surface Renderer")]
	[SerializeField]
	private CombatHudSurfaceRenderer m_SurfaceRenderer;

	[SerializeField]
	private float m_DelayForResetToMoveGrid = 0.3f;

	private bool m_TurnBasedModeActive;

	private bool m_MovementAreaDisplayEnabled;

	private bool m_SpaceCombatMovementAreaDisplayEnabled;

	private bool m_AbilityAreaDisplayEnabled;

	private BaseUnitEntity m_ActiveUnit;

	private bool m_ForceDrawThreatArea;

	private readonly CombatHubCollectionAreaSource m_MovementArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_SpaceCombatMovementAreaPhaseOne = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_SpaceCombatMovementAreaPhaseTwo = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_SpaceCombatMovementAreaPhaseThree = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_ActiveUnitArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_AttackOfOpportunityArea = new CombatHubCollectionAreaSource();

	private readonly RingAreaSource m_MinRangeArea = new RingAreaSource();

	private readonly RingAreaSource m_MaxRangeArea = new RingAreaSource();

	private readonly CombatHubCollectionAreaSource m_PrimaryAoeArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_SecondaryAoeArea = new CombatHubCollectionAreaSource();

	private readonly RingAreaSource m_WeaponEffectiveRangeArea = new RingAreaSource();

	private StratagemAreaCollection m_AllyStratagemAreas;

	private StratagemAreaCollection m_HostileStratagemAreas;

	private bool m_MovementAreaValid;

	private bool m_ActiveUnitAreaValid;

	private bool m_AttackOfOpportunityAreaValid;

	private bool m_MinRangeAreaValid;

	private bool m_MaxRangeAreaValid;

	private bool m_PrimaryAoeAreaValid;

	private bool m_SecondaryAoeAreaValid;

	private bool m_WeaponEffectiveRangeAreaValid;

	private bool m_PendingRefresh;

	private float m_HighlightSpaceMovementAreaStartTime;

	private CombatHudCommandSetAsset m_AbilityCommandsOverride;

	private readonly ObjectPool<CombatHubCollectionAreaSource> m_CollectionAreaSourcePool = new ObjectPool<CombatHubCollectionAreaSource>(() => new CombatHubCollectionAreaSource(), null, delegate(CombatHubCollectionAreaSource source)
	{
		source.Clear();
	});

	private IDisposable m_ShowDefaultAreas;

	private IDisposable m_ShowShowMovementAreas;

	public static CombatHUDRenderer Instance { get; private set; }

	public bool ForceDrawThreatArea
	{
		get
		{
			return m_ForceDrawThreatArea;
		}
		set
		{
			if (m_ForceDrawThreatArea != value)
			{
				m_ForceDrawThreatArea = value;
				if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
				{
					ShowMovementAreas();
				}
			}
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Instance = this;
		m_AllyStratagemAreas = new StratagemAreaCollection(m_CollectionAreaSourcePool);
		m_HostileStratagemAreas = new StratagemAreaCollection(m_CollectionAreaSourcePool);
		EventBus.Subscribe(this);
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instance = this;
		EventBus.Unsubscribe(this);
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		if (m_PendingRefresh)
		{
			m_PendingRefresh = false;
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	private void OnGraphsUpdated(AstarPath script)
	{
		m_PendingRefresh = true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		bool valueOrDefault = (Game.Instance.State?.LoadedAreaState?.Blueprint?.IsShipArea).GetValueOrDefault();
		m_SurfaceRenderer.SetCommandsSet(valueOrDefault ? CombatHudSurfaceRenderer.CommandsSet.SpaceCombat : CombatHudSurfaceRenderer.CommandsSet.SurfaceCombat);
		m_AllyStratagemAreas.Clear();
		m_HostileStratagemAreas.Clear();
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			NodeList patternNodes = areaEffect.GetPatternCoveredNodes();
			if (!patternNodes.IsEmpty)
			{
				MechanicEntity maybeCaster = areaEffect.Context.MaybeCaster;
				if (maybeCaster == null || maybeCaster.IsPlayerFaction)
				{
					m_AllyStratagemAreas.SetupArea(areaEffect, in patternNodes);
				}
				else
				{
					m_HostileStratagemAreas.SetupArea(areaEffect, in patternNodes);
				}
			}
		}
		EventBus.RaiseEvent(delegate(IUpdateShieldPatternBeforeAreaLoadHandler h)
		{
			h.HandleUpdateShieldPatternBeforeAreaLoad();
		});
		m_PendingRefresh = true;
	}

	void IAreaEffectHandler.HandleAreaEffectSpawned()
	{
		if (!(EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity))
		{
			return;
		}
		NodeList patternNodes = areaEffectEntity.GetPatternCoveredNodes();
		if (!patternNodes.IsEmpty)
		{
			MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
			if (maybeCaster == null || maybeCaster.IsPlayerFaction)
			{
				m_AllyStratagemAreas.SetupArea(areaEffectEntity, in patternNodes);
			}
			else
			{
				m_HostileStratagemAreas.SetupArea(areaEffectEntity, in patternNodes);
			}
			m_PendingRefresh = true;
		}
	}

	void IAreaEffectHandler.HandleAreaEffectDestroyed()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity)
		{
			MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
			if (maybeCaster == null || maybeCaster.IsPlayerFaction)
			{
				m_PendingRefresh |= m_AllyStratagemAreas.CleanupArea(areaEffectEntity);
			}
			else
			{
				m_PendingRefresh |= m_HostileStratagemAreas.CleanupArea(areaEffectEntity);
			}
		}
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		m_MovementAreaDisplayEnabled = true;
		m_ActiveUnit = EventInvokerExtensions.BaseUnitEntity;
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
		{
			ShowMovementAreas(nodes);
		}
	}

	public void HandleRemoveUnitMovableArea()
	{
		ClearShowTasks();
		if (m_MovementAreaDisplayEnabled)
		{
			m_MovementAreaDisplayEnabled = false;
			if (EvaluateAreaDisplayMode() == AreaDisplayMode.Default)
			{
				ShowDefaultAreas();
			}
		}
	}

	public void HighlightSpaceMovementAreaThirdPhase()
	{
		m_HighlightSpaceMovementAreaStartTime = Time.time;
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
		{
			ShowMovementAreas();
		}
	}

	public void SetSpaceCombatMovementArea(List<CustomGridNodeBase> movementAreaPhaseOneNodes, List<CustomGridNodeBase> movementAreaPhaseTwoNodes, List<CustomGridNodeBase> movementAreaPhaseThreeNod)
	{
		m_SpaceCombatMovementAreaDisplayEnabled = true;
		m_ActiveUnit = null;
		m_SpaceCombatMovementAreaPhaseOne.Clear();
		m_SpaceCombatMovementAreaPhaseTwo.Clear();
		m_SpaceCombatMovementAreaPhaseThree.Clear();
		m_SpaceCombatMovementAreaPhaseOne.AddRange(movementAreaPhaseOneNodes);
		m_SpaceCombatMovementAreaPhaseTwo.AddRange(movementAreaPhaseTwoNodes);
		m_SpaceCombatMovementAreaPhaseThree.AddRange(movementAreaPhaseThreeNod);
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
		{
			ShowMovementAreas();
		}
	}

	public void ClearSpaceCombatMovementArea()
	{
		m_SpaceCombatMovementAreaDisplayEnabled = false;
		m_SpaceCombatMovementAreaPhaseOne.Clear();
		m_SpaceCombatMovementAreaPhaseTwo.Clear();
		m_SpaceCombatMovementAreaPhaseThree.Clear();
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Default)
		{
			ShowDefaultAreas();
		}
	}

	public void SetAbilityAreaHUD(AbilityAreaHudInfo abilityAreaHudInfo)
	{
		m_AbilityAreaDisplayEnabled = true;
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Ability)
		{
			ShowAbilityAreas(abilityAreaHudInfo);
		}
	}

	public void RemoveAbilityAreaHUD()
	{
		if (!m_AbilityAreaDisplayEnabled)
		{
			return;
		}
		ClearShowTasks();
		m_AbilityAreaDisplayEnabled = false;
		switch (EvaluateAreaDisplayMode())
		{
		case AreaDisplayMode.Default:
			m_ShowDefaultAreas = DelayedInvoker.InvokeInTime(delegate
			{
				ShowDefaultAreas();
			}, m_DelayForResetToMoveGrid);
			break;
		case AreaDisplayMode.Movement:
			m_ShowShowMovementAreas = DelayedInvoker.InvokeInTime(delegate
			{
				ShowMovementAreas();
			}, m_DelayForResetToMoveGrid);
			break;
		}
	}

	private void ClearShowTasks()
	{
		m_ShowDefaultAreas?.Dispose();
		m_ShowDefaultAreas = null;
		m_ShowShowMovementAreas?.Dispose();
		m_ShowShowMovementAreas = null;
	}

	private void ShowDefaultAreas()
	{
		ClearAreas();
		UpdateSurfaceRenderer();
	}

	private void ShowMovementAreas(List<GraphNode> movementNodes = null)
	{
		ClearAreas();
		try
		{
			PopulateActiveUnitArea();
			PopulateAttackOfOpportunityArea();
			PopulateMovementArea(movementNodes);
			UpdateSurfaceRenderer();
		}
		catch
		{
			ClearAreas();
			UpdateSurfaceRenderer();
			throw;
		}
	}

	private void ShowAbilityAreas(AbilityAreaHudInfo abilityAreaHudInfo)
	{
		ClearAreas();
		try
		{
			bool flag;
			bool flag2;
			bool buildPrimaryArea;
			if (abilityAreaHudInfo.combatHudCommandsOverride != null)
			{
				CombatHudAreas usedAreas = abilityAreaHudInfo.combatHudCommandsOverride.GetUsedAreas();
				flag = (usedAreas & (CombatHudAreas.AbilityMinRange | CombatHudAreas.AbilityMaxRange | CombatHudAreas.AbilityEffectiveRange)) != 0;
				flag2 = (usedAreas & (CombatHudAreas.AbilityPrimary | CombatHudAreas.AbilitySecondary)) != 0;
				buildPrimaryArea = (usedAreas & CombatHudAreas.AbilityPrimary) != 0;
			}
			else
			{
				flag = !abilityAreaHudInfo.ignoreRangesByDefault;
				flag2 = true;
				buildPrimaryArea = !abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault;
			}
			m_AbilityCommandsOverride = abilityAreaHudInfo.combatHudCommandsOverride;
			PopulateActiveUnitArea();
			if (flag)
			{
				PopulateAbilityRangeAreas(abilityAreaHudInfo.casterRect, abilityAreaHudInfo.minRange, abilityAreaHudInfo.maxRange, abilityAreaHudInfo.effectiveRange);
			}
			if (flag2)
			{
				PopulateAbilityPatternAreas(abilityAreaHudInfo.pattern, buildPrimaryArea);
			}
			UpdateSurfaceRenderer();
		}
		catch
		{
			ClearAreas();
			UpdateSurfaceRenderer();
			throw;
		}
	}

	private void ClearAreas()
	{
		ClearShowTasks();
		m_MovementArea.Clear();
		m_ActiveUnitArea.Clear();
		m_AttackOfOpportunityArea.Clear();
		m_MinRangeArea.Clear();
		m_MaxRangeArea.Clear();
		m_PrimaryAoeArea.Clear();
		m_SecondaryAoeArea.Clear();
		m_WeaponEffectiveRangeArea.Clear();
		m_MovementAreaValid = false;
		m_ActiveUnitAreaValid = false;
		m_AttackOfOpportunityAreaValid = false;
		m_MinRangeAreaValid = false;
		m_MaxRangeAreaValid = false;
		m_PrimaryAoeAreaValid = false;
		m_SecondaryAoeAreaValid = false;
		m_WeaponEffectiveRangeAreaValid = false;
		m_AbilityCommandsOverride = null;
	}

	private void PopulateActiveUnitArea()
	{
		BaseUnitEntity activeUnit = m_ActiveUnit;
		if (activeUnit != null && !activeUnit.IsDisposed && m_ActiveUnit.IsMyNetRole())
		{
			m_ActiveUnitAreaValid = true;
			m_ActiveUnitArea.AddRange(m_ActiveUnit.GetOccupiedNodes());
		}
	}

	private void PopulateAttackOfOpportunityArea()
	{
		if (IsDeploymentPhaseActive())
		{
			m_AttackOfOpportunityAreaValid = true;
			List<GraphNode> deploymentForbiddenArea = Game.Instance.UnitMovableAreaController.DeploymentForbiddenArea;
			if (deploymentForbiddenArea != null)
			{
				m_AttackOfOpportunityArea.AddRange(deploymentForbiddenArea);
			}
		}
		else
		{
			if (m_ActiveUnit == null || !m_ActiveUnit.IsMyNetRole())
			{
				return;
			}
			m_AttackOfOpportunityAreaValid = true;
			foreach (BaseUnitEntity item in m_ForceDrawThreatArea ? m_ActiveUnit.CombatGroup.Memory.Enemies.Select((UnitGroupMemory.UnitInfo i) => i.Unit) : m_ActiveUnit.GetEngagedByUnits())
			{
				if (item.State.CanAct && !item.IsInvisible)
				{
					HashSet<GraphNode> threateningArea = item.GetThreateningArea();
					if (threateningArea != null)
					{
						m_AttackOfOpportunityArea.AddRange(threateningArea);
					}
				}
			}
		}
	}

	private void PopulateMovementArea(List<GraphNode> movementNodes)
	{
		if (m_SpaceCombatMovementAreaDisplayEnabled || m_ActiveUnit == null || !m_ActiveUnit.IsMyNetRole())
		{
			return;
		}
		m_MovementAreaValid = true;
		if (movementNodes == null && Game.Instance.UnitMovableAreaController.DeploymentPhase && Game.Instance.UnitMovableAreaController.CurrentUnit == m_ActiveUnit)
		{
			movementNodes = Game.Instance.UnitMovableAreaController.CurrentUnitMovableArea;
		}
		if (movementNodes == null)
		{
			int num = ((!m_ActiveUnit.HasMechanicFeature(MechanicsFeatureType.CantMove)) ? ((int)m_ActiveUnit.CombatState.ActionPointsBlue) : 0);
			if (num > 0)
			{
				movementNodes = PathfindingService.Instance.FindAllReachableTiles_Blocking(m_ActiveUnit.View.MovementAgent, m_ActiveUnit.Position, num).Keys.ToList();
			}
		}
		if (movementNodes != null)
		{
			ExtendMovementAreaByUnitSize(movementNodes, m_ActiveUnit.SizeRect);
			m_MovementArea.AddRange(movementNodes);
		}
	}

	private void ExtendMovementAreaByUnitSize(List<GraphNode> movementNodes, IntRect sizeRect)
	{
		Dictionary<Vector2Int, CustomGridNode> value;
		using (CollectionPool<Dictionary<Vector2Int, CustomGridNode>, KeyValuePair<Vector2Int, CustomGridNode>>.Get(out value))
		{
			List<Vector2Int> value2;
			using (CollectionPool<List<Vector2Int>, Vector2Int>.Get(out value2))
			{
				List<Vector2Int> value3;
				using (CollectionPool<List<Vector2Int>, Vector2Int>.Get(out value3))
				{
					for (int i = 0; i < movementNodes.Count; i++)
					{
						if (movementNodes[i] is CustomGridNode customGridNode)
						{
							Vector2Int key = new Vector2Int(customGridNode.XCoordinateInGrid, customGridNode.ZCoordinateInGrid);
							value.Add(key, customGridNode);
						}
					}
					foreach (Vector2Int key2 in value.Keys)
					{
						if (!value.ContainsKey(key2 + Vector2Int.up))
						{
							value2.Add(key2);
						}
						if (!value.ContainsKey(key2 + Vector2Int.right))
						{
							value3.Add(key2);
						}
					}
					for (int j = 0; j < sizeRect.Height - 1; j++)
					{
						for (int num = value2.Count - 1; num >= 0; num--)
						{
							CustomGridNodeBase neighbourAlongDirection = value[value2[num]].GetNeighbourAlongDirection(2);
							if (neighbourAlongDirection != null)
							{
								value2.RemoveAt(num);
								Vector2Int vector2Int = new Vector2Int(neighbourAlongDirection.XCoordinateInGrid, neighbourAlongDirection.ZCoordinateInGrid);
								value[vector2Int] = neighbourAlongDirection as CustomGridNode;
								value2.Add(vector2Int);
								movementNodes.Add(neighbourAlongDirection);
								if (neighbourAlongDirection.GetNeighbourAlongDirection(1) != null && !value3.Contains(vector2Int))
								{
									value3.Add(vector2Int);
								}
							}
						}
					}
					for (int k = 0; k < sizeRect.Width - 1; k++)
					{
						for (int num2 = value3.Count - 1; num2 >= 0; num2--)
						{
							CustomGridNodeBase neighbourAlongDirection2 = value[value3[num2]].GetNeighbourAlongDirection(1);
							if (neighbourAlongDirection2 != null)
							{
								value3.RemoveAt(num2);
								Vector2Int vector2Int2 = new Vector2Int(neighbourAlongDirection2.XCoordinateInGrid, neighbourAlongDirection2.ZCoordinateInGrid);
								value[vector2Int2] = neighbourAlongDirection2 as CustomGridNode;
								value3.Add(vector2Int2);
								movementNodes.Add(neighbourAlongDirection2);
							}
						}
					}
				}
			}
		}
	}

	private void PopulateAbilityRangeAreas(IntRect casterRect, int minRange, int maxRange, int effectiveRange)
	{
		if (maxRange > 0 && maxRange <= 200)
		{
			if (minRange > 0 && minRange < maxRange && effectiveRange > minRange && effectiveRange < maxRange)
			{
				m_MinRangeAreaValid = true;
				m_WeaponEffectiveRangeAreaValid = true;
				m_MaxRangeAreaValid = true;
				m_MinRangeArea.Setup(casterRect, -1, minRange);
				m_WeaponEffectiveRangeArea.Setup(casterRect, minRange, effectiveRange);
				m_MaxRangeArea.Setup(casterRect, effectiveRange, maxRange);
			}
			else if (minRange > 0 && minRange < maxRange)
			{
				m_MinRangeAreaValid = true;
				m_MaxRangeAreaValid = true;
				m_MinRangeArea.Setup(casterRect, -1, minRange);
				m_MaxRangeArea.Setup(casterRect, minRange, maxRange);
			}
			else if (effectiveRange > 0 && effectiveRange < maxRange)
			{
				m_WeaponEffectiveRangeAreaValid = true;
				m_MaxRangeAreaValid = true;
				m_WeaponEffectiveRangeArea.Setup(casterRect, -1, effectiveRange);
				m_MaxRangeArea.Setup(casterRect, effectiveRange, maxRange);
			}
			else
			{
				m_MaxRangeAreaValid = true;
				m_MaxRangeArea.Setup(casterRect, -1, maxRange);
			}
		}
	}

	private void PopulateAbilityPatternAreas(OrientedPatternData pattern, bool buildPrimaryArea)
	{
		if (pattern.ApplicationNode == null)
		{
			return;
		}
		if (buildPrimaryArea)
		{
			m_PrimaryAoeAreaValid = true;
			m_SecondaryAoeAreaValid = true;
			OrientedPatternData.NodesWithExtraDataEnumerator enumerator = pattern.NodesWithExtraData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				(CustomGridNodeBase, PatternCellData) current = enumerator.Current;
				if (current.Item1.position == pattern.ApplicationNode.position || current.Item2.MainCell)
				{
					m_PrimaryAoeArea.Add(current.Item1);
				}
				m_SecondaryAoeArea.Add(current.Item1);
			}
			return;
		}
		m_PrimaryAoeAreaValid = false;
		m_SecondaryAoeAreaValid = true;
		foreach (CustomGridNodeBase node in pattern.Nodes)
		{
			m_SecondaryAoeArea.Add(node);
		}
	}

	private void UpdateSurfaceRenderer()
	{
		if (!(m_SurfaceRenderer == null))
		{
			if (IsDeploymentPhaseActive())
			{
				m_SurfaceRenderer.MovementAreaSource = null;
				m_SurfaceRenderer.AttackOfOpportunityAreaSource = null;
				m_SurfaceRenderer.DeploymentPermittedAreaSource = (m_MovementAreaValid ? m_MovementArea : null);
				m_SurfaceRenderer.DeploymentForbiddenAreaSource = (m_AttackOfOpportunityAreaValid ? m_AttackOfOpportunityArea : null);
			}
			else
			{
				m_SurfaceRenderer.MovementAreaSource = (m_MovementAreaValid ? m_MovementArea : null);
				m_SurfaceRenderer.AttackOfOpportunityAreaSource = (m_AttackOfOpportunityAreaValid ? m_AttackOfOpportunityArea : null);
				m_SurfaceRenderer.DeploymentPermittedAreaSource = null;
				m_SurfaceRenderer.DeploymentForbiddenAreaSource = null;
			}
			m_SurfaceRenderer.ActiveUnitAreaSource = (m_ActiveUnitAreaValid ? m_ActiveUnitArea : null);
			m_SurfaceRenderer.MinRangeAreaSource = (m_MinRangeAreaValid ? m_MinRangeArea : null);
			m_SurfaceRenderer.MaxRangeAreaSource = (m_MaxRangeAreaValid ? m_MaxRangeArea : null);
			m_SurfaceRenderer.PrimaryAreaSource = (m_PrimaryAoeAreaValid ? m_PrimaryAoeArea : null);
			m_SurfaceRenderer.SecondaryAreaSource = (m_SecondaryAoeAreaValid ? m_SecondaryAoeArea : null);
			m_SurfaceRenderer.EffectiveRangeAreaSource = (m_WeaponEffectiveRangeAreaValid ? m_WeaponEffectiveRangeArea : null);
			m_SurfaceRenderer.AbilityCommandsOverride = m_AbilityCommandsOverride;
			if (m_SpaceCombatMovementAreaDisplayEnabled)
			{
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseOne = m_SpaceCombatMovementAreaPhaseOne;
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseTwo = m_SpaceCombatMovementAreaPhaseTwo;
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseThree = m_SpaceCombatMovementAreaPhaseThree;
				m_SurfaceRenderer.HighlightSpaceCombatMovementAreaPhaseThree = new HighlightData(m_HighlightSpaceMovementAreaStartTime, 2f);
			}
			else
			{
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseOne = null;
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseTwo = null;
				m_SurfaceRenderer.SpaceCombatMovementAreaPhaseThree = null;
			}
			m_AllyStratagemAreas.GetAreaDataList(m_SurfaceRenderer.AllyStratagemAreaDataList);
			m_HostileStratagemAreas.GetAreaDataList(m_SurfaceRenderer.HostileStratagemAreaDataList);
			m_SurfaceRenderer.Display();
		}
	}

	private AreaDisplayMode EvaluateAreaDisplayMode()
	{
		if (m_AbilityAreaDisplayEnabled)
		{
			return AreaDisplayMode.Ability;
		}
		if (m_MovementAreaDisplayEnabled || m_SpaceCombatMovementAreaDisplayEnabled)
		{
			return AreaDisplayMode.Movement;
		}
		return AreaDisplayMode.Default;
	}

	public void HandleTurnBasedModeResumed()
	{
		HandleTurnBasedModeSwitched(isTurnBased: true);
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		if (m_TurnBasedModeActive != isTurnBased)
		{
			m_TurnBasedModeActive = isTurnBased;
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (m_ActiveUnit != null && m_ActiveUnit.UniqueId == entityId)
		{
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	private bool IsDeploymentPhaseActive()
	{
		return Game.Instance.TurnController.IsPreparationTurn;
	}
}
