using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class CombatHudSurfaceRenderer : MonoBehaviour
{
	public enum CommandsSet
	{
		SurfaceCombat,
		SpaceCombat
	}

	[Header("Components")]
	[SerializeField]
	private MeshRenderer m_FillMeshRenderer;

	[SerializeField]
	private MeshFilter m_FillMeshFilter;

	[SerializeField]
	private MeshRenderer m_OutlineMeshRenderer;

	[SerializeField]
	private MeshFilter m_OutlineMeshFilter;

	[Header("Asset")]
	[SerializeField]
	private CombatHudSurfaceRendererAsset m_SurfaceCombatAsset;

	[SerializeField]
	private CombatHudSurfaceRendererAsset m_SpaceCombatAsset;

	private SurfaceService m_Service;

	private SurfaceServiceRequestPool m_ServiceRequestPool;

	private CommandsSet m_CommandSet;

	private readonly MaterialBindingDataSource m_MaterialBindingDataSource = new MaterialBindingDataSource();

	private Material[] m_PendingOverrideMaterials;

	public IAreaSource MovementAreaSource { get; set; }

	public IAreaSource DeploymentPermittedAreaSource { get; set; }

	public IAreaSource DeploymentForbiddenAreaSource { get; set; }

	public IAreaSource ActiveUnitAreaSource { get; set; }

	public IAreaSource AttackOfOpportunityAreaSource { get; set; }

	public IAreaSource MinRangeAreaSource { get; set; }

	public IAreaSource MaxRangeAreaSource { get; set; }

	public IAreaSource EffectiveRangeAreaSource { get; set; }

	public IAreaSource PrimaryAreaSource { get; set; }

	public IAreaSource SecondaryAreaSource { get; set; }

	public List<AreaSourceData> AllyStratagemAreaDataList { get; } = new List<AreaSourceData>();


	public List<AreaSourceData> HostileStratagemAreaDataList { get; } = new List<AreaSourceData>();


	public IAreaSource SpaceCombatMovementAreaPhaseOne { get; set; }

	public IAreaSource SpaceCombatMovementAreaPhaseTwo { get; set; }

	public IAreaSource SpaceCombatMovementAreaPhaseThree { get; set; }

	public CombatHudCommandSetAsset AbilityCommandsOverride { get; set; }

	public HighlightData HighlightSpaceCombatMovementAreaPhaseThree { get; set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		m_ServiceRequestPool = new SurfaceServiceRequestPool();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		m_Service?.Dispose();
		m_Service = null;
		m_ServiceRequestPool.Dispose();
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		m_Service?.Update();
	}

	public void SetCommandsSet(CommandsSet commandsSet)
	{
		m_CommandSet = commandsSet;
	}

	public void SetOverrideMaterials([CanBeNull] Material[] overrideMaterials)
	{
		if (m_Service == null)
		{
			m_PendingOverrideMaterials = overrideMaterials;
		}
		else
		{
			m_Service.SetOverrideMaterials(overrideMaterials);
		}
	}

	public CombatHudSurfaceRendererAsset ResolveAsset()
	{
		if (m_CommandSet == CommandsSet.SpaceCombat)
		{
			return m_SpaceCombatAsset;
		}
		return m_SurfaceCombatAsset;
	}

	public void Display()
	{
		CustomGridGraph graph = CombatHudGraphDataSource.FindGraph();
		if (!EnsurePrerequisites(graph))
		{
			return;
		}
		CombatHudSurfaceRendererAsset combatHudSurfaceRendererAsset = ResolveAsset();
		m_Service.DiscardPendingRequest();
		m_MaterialBindingDataSource.Clear();
		m_MaterialBindingDataSource.SetHighlight(HighlightDataSource.SpaceCombatMovement3, HighlightSpaceCombatMovementAreaPhaseThree);
		SurfaceServiceRequest surfaceServiceRequest = m_ServiceRequestPool.Get();
		surfaceServiceRequest.Graph = graph;
		surfaceServiceRequest.OutlineSettings = combatHudSurfaceRendererAsset.outlineSettings;
		surfaceServiceRequest.FillSettings = combatHudSurfaceRendererAsset.fillSettings;
		surfaceServiceRequest.InsertMaterial(null, default(MaterialOverrides));
		if (MovementAreaSource != null || DeploymentPermittedAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(2u, MovementAreaSource ?? DeploymentPermittedAreaSource));
		}
		if (ActiveUnitAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(4u, ActiveUnitAreaSource));
		}
		if (AttackOfOpportunityAreaSource != null || DeploymentForbiddenAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(8u, AttackOfOpportunityAreaSource ?? DeploymentForbiddenAreaSource));
		}
		if (MinRangeAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(16u, MinRangeAreaSource));
		}
		if (MaxRangeAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(32u, MaxRangeAreaSource));
		}
		if (EffectiveRangeAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(64u, EffectiveRangeAreaSource));
		}
		if (PrimaryAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(128u, PrimaryAreaSource));
		}
		if (SecondaryAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(256u, SecondaryAreaSource));
		}
		if (SpaceCombatMovementAreaPhaseOne != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(8192u, SpaceCombatMovementAreaPhaseOne));
		}
		if (SpaceCombatMovementAreaPhaseTwo != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(16384u, SpaceCombatMovementAreaPhaseTwo));
		}
		if (SpaceCombatMovementAreaPhaseThree != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(32768u, SpaceCombatMovementAreaPhaseThree));
		}
		foreach (AreaSourceData allyStratagemAreaData in AllyStratagemAreaDataList)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(512u, allyStratagemAreaData.Source, 1, isStratagem: true));
		}
		foreach (AreaSourceData hostileStratagemAreaData in HostileStratagemAreaDataList)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(2048u, hostileStratagemAreaData.Source, 1, isStratagem: true));
		}
		bool num = DeploymentPermittedAreaSource != null;
		bool flag = MovementAreaSource != null;
		bool flag2 = SpaceCombatMovementAreaPhaseOne != null || SpaceCombatMovementAreaPhaseTwo != null || SpaceCombatMovementAreaPhaseThree != null;
		bool flag3 = MinRangeAreaSource != null || MaxRangeAreaSource != null || EffectiveRangeAreaSource != null;
		bool flag4 = PrimaryAreaSource != null || SecondaryAreaSource != null;
		CombatHudCommand[] array = (num ? combatHudSurfaceRendererAsset.deploymentCommands : (flag ? combatHudSurfaceRendererAsset.movementCommands : ((flag4 && flag3) ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternRangeCommands) : (flag3 ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityRangeCommands) : (flag4 ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternCommands) : ((!flag2) ? null : combatHudSurfaceRendererAsset.spaceMovementCommands))))));
		if (array != null)
		{
			CombatHudCommand[] array2 = array;
			foreach (CombatHudCommand combatHudCommand in array2)
			{
				combatHudCommand.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
		}
		int num2 = 0;
		if (AllyStratagemAreaDataList.Count > 0)
		{
			CombatHudCommand[] array2 = combatHudSurfaceRendererAsset.allyStratagemStartCommands;
			foreach (CombatHudCommand combatHudCommand2 in array2)
			{
				combatHudCommand2.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
			foreach (AreaSourceData allyStratagemAreaData2 in AllyStratagemAreaDataList)
			{
				array2 = combatHudSurfaceRendererAsset.allyStratagemLoopCommands;
				foreach (CombatHudCommand combatHudCommand3 in array2)
				{
					m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, allyStratagemAreaData2.IconTexture);
					m_MaterialBindingDataSource.SetMaterialRemap(allyStratagemAreaData2.MaterialRemapAsset);
					combatHudCommand3.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, num2);
				}
				num2++;
			}
			m_MaterialBindingDataSource.SetMaterialRemap(null);
			array2 = combatHudSurfaceRendererAsset.allyStratagemFinishCommands;
			foreach (CombatHudCommand combatHudCommand4 in array2)
			{
				combatHudCommand4.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
		}
		if (HostileStratagemAreaDataList.Count > 0)
		{
			CombatHudCommand[] array2 = combatHudSurfaceRendererAsset.hostileStratagemStartCommands;
			foreach (CombatHudCommand combatHudCommand5 in array2)
			{
				combatHudCommand5.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
			foreach (AreaSourceData hostileStratagemAreaData2 in HostileStratagemAreaDataList)
			{
				array2 = combatHudSurfaceRendererAsset.hostileStratagemLoopCommands;
				foreach (CombatHudCommand combatHudCommand6 in array2)
				{
					m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, hostileStratagemAreaData2.IconTexture);
					m_MaterialBindingDataSource.SetMaterialRemap(hostileStratagemAreaData2.MaterialRemapAsset);
					combatHudCommand6.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, num2);
				}
				num2++;
			}
			m_MaterialBindingDataSource.SetMaterialRemap(null);
			array2 = combatHudSurfaceRendererAsset.hostileStratagemFinishCommands;
			foreach (CombatHudCommand combatHudCommand7 in array2)
			{
				combatHudCommand7.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
		}
		m_Service.SetPendingRequest(surfaceServiceRequest);
	}

	private bool EnsurePrerequisites(CustomGridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (m_FillMeshFilter == null)
		{
			return false;
		}
		if (m_FillMeshRenderer == null)
		{
			return false;
		}
		if (m_OutlineMeshFilter == null)
		{
			return false;
		}
		if (m_OutlineMeshRenderer == null)
		{
			return false;
		}
		if (ResolveAsset() == null)
		{
			return false;
		}
		if (m_Service == null)
		{
			m_Service = new SurfaceService(m_FillMeshFilter, m_FillMeshRenderer, m_OutlineMeshFilter, m_OutlineMeshRenderer, m_ServiceRequestPool);
			if (m_PendingOverrideMaterials != null)
			{
				m_Service.SetOverrideMaterials(m_PendingOverrideMaterials);
				m_PendingOverrideMaterials = null;
			}
		}
		return true;
	}
}
