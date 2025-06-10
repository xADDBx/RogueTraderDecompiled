using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[TypeId("4e19ee98b71c98b40ba235cfa715b760")]
public class BlueprintAbilityAreaEffect : BlueprintMechanicEntityFact, IAbilityAoEPatternProvider, IResourcesHolder
{
	[SerializeField]
	private bool m_AllowNonContextActions;

	public TargetType TargetType;

	public bool AffectEnemies;

	public bool AggroEnemies = true;

	public bool AffectDead;

	[SerializeField]
	private AreaEffectRestrictions m_AreaEffectRestrictions = AreaEffectRestrictions.None;

	public bool IsAllArea;

	public bool OnlyInCombat;

	public bool SavePersistentArea;

	public Texture2D PersistentAreaTexture2D;

	public CombatHudMaterialRemapAsset PersistentAreaMaterialRemap;

	public bool IsStrategistAbility;

	public bool NeedsTooltip;

	[SerializeField]
	[ShowIf("NeedsTooltip")]
	private BlueprintBuffReference m_BlueprintBuffForTooltip;

	[ShowIf("CanChooseStrategistTacticsAbilityType")]
	public StrategistTacticsAreaEffectType TacticsAreaEffectType;

	[SerializeField]
	[HideIf("IsAllArea")]
	private AoEPattern m_Pattern;

	[HideIf("IsAllArea")]
	public bool IgnoreLosWhenSpread;

	[HideIf("IsAllArea")]
	public bool IgnoreLevelDifferenceWhenSpread;

	public PrefabLink Fx;

	public PrefabLink FxOnEndAreaEffect;

	public bool ScrollCameraToAreaEffectWhenEnded;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintAbilityAreaEffectGroupReference[] m_AreaEffectGroups;

	private AreaEffectClusterComponent m_ClusterComponent;

	public BlueprintBuff BlueprintBuffForTooltip => m_BlueprintBuffForTooltip?.Get();

	public AoEPattern Pattern
	{
		get
		{
			if (!IsAllArea)
			{
				return m_Pattern;
			}
			return null;
		}
	}

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public ReferenceArrayProxy<BlueprintAbilityAreaEffectGroup> AreaEffectGroups
	{
		get
		{
			BlueprintReference<BlueprintAbilityAreaEffectGroup>[] areaEffectGroups = m_AreaEffectGroups;
			return areaEffectGroups;
		}
	}

	public bool CanTargetEnemies => TargetType != TargetType.Ally;

	public bool CanTargetAllies => TargetType != TargetType.Enemy;

	private bool CanChooseStrategistTacticsAbilityType => IsStrategistAbility;

	public override bool AllowContextActionsOnly => !m_AllowNonContextActions;

	public AreaEffectRestrictions AreaEffectRestrictions => m_AreaEffectRestrictions;

	public bool HasConcussionEffect => HasFlag(AreaEffectRestrictions.CanOnlyUseWeaponAbilities);

	public bool HasCantAttackEffect => HasFlag(AreaEffectRestrictions.CannotUseWeaponAbilities);

	public bool HasInertWarpEffect => HasFlag(AreaEffectRestrictions.CannotUsePsychicPowers);

	private bool SearchedForCusterComponent { get; set; }

	private AreaEffectClusterComponent ClusterComponent
	{
		get
		{
			if (m_ClusterComponent == null)
			{
				if (SearchedForCusterComponent)
				{
					return null;
				}
				IEnumerable<AreaEffectClusterComponent> source = base.ComponentsArray.OfType<AreaEffectClusterComponent>();
				m_ClusterComponent = source.FirstOrDefault();
				SearchedForCusterComponent = true;
				return m_ClusterComponent;
			}
			return m_ClusterComponent;
		}
	}

	bool IAbilityAoEPatternProvider.IsIgnoreLos => IgnoreLosWhenSpread;

	public bool UseMeleeLos => false;

	bool IAbilityAoEPatternProvider.IsIgnoreLevelDifference => IgnoreLevelDifferenceWhenSpread;

	int IAbilityAoEPatternProvider.PatternAngle => 0;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => true;

	public bool ExcludeUnwalkable => IsStrategistAbility;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType;

	private BlueprintComponent[] TryOverrideComponentsArray()
	{
		if (ClusterComponent != null)
		{
			return ClusterComponent.ClusterLogicBlueprint.ComponentsArray;
		}
		return base.ComponentsArray;
	}

	private bool IsUnitInAnotherAreaOfCluster(BaseUnitEntity unit, AreaEffectEntity areaEffect)
	{
		if (ClusterComponent != null)
		{
			return unit.IsCurrentlyInAnotherClusterArea(ClusterComponent.ClusterLogicBlueprint, areaEffect);
		}
		return false;
	}

	private void MarkUnitEnteredInAreaCluster(BaseUnitEntity unit, AreaEffectEntity areaEffect)
	{
		if (ClusterComponent != null)
		{
			PartUnitInAreaEffectCluster obj = unit.GetPartUnitInAreaEffectClusterOptional() ?? unit.GetOrCreate<PartUnitInAreaEffectCluster>();
			obj.AddClusterKey(ClusterComponent.ClusterLogicBlueprint);
			obj.AddEnteringAreaEffectToList(ClusterComponent.ClusterLogicBlueprint, areaEffect);
		}
	}

	public void HandleUnitEnter(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (IsUnitInAnotherAreaOfCluster(unit, areaEffect))
		{
			return;
		}
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleUnitEnter(context, areaEffect, unit);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
		MarkUnitEnteredInAreaCluster(unit, areaEffect);
	}

	public void HandleUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, IBaseUnitEntity unit)
	{
		HandleUnitExit(context, areaEffect, (BaseUnitEntity)unit);
	}

	public void HandleUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (IsUnitInAnotherAreaOfCluster(unit, areaEffect))
		{
			return;
		}
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleUnitExit(context, areaEffect, unit);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleUnitMove(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleUnitMove(context, areaEffect, unit);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleRound(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleTick(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectSpawnLogic)?.HandleAreaEffectSpawn(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (ScrollCameraToAreaEffectWhenEnded)
		{
			((ICameraFocusTarget)areaEffect).RetainCamera();
		}
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AbilityAreaEffectLogic)?.HandleEnd(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	private bool HasFlag(AreaEffectRestrictions flag)
	{
		return (AreaEffectRestrictions & flag) != 0;
	}

	public OrientedPatternData GetOrientedPattern(CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		return ((IAbilityAoEPatternProvider)this).GetOrientedPattern((IAbilityDataProviderForPattern)null, casterNode, targetNode, coveredTargetsOnly: false);
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	OrientedPatternData IAbilityAoEPatternProvider.GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly)
	{
		CustomGridNodeBase actualCastNode;
		return AoEPatternHelper.GetOrientedPattern(ability, ability.Caster, Pattern, this, casterNode, targetNode, castOnSameLevel: false, directional: false, coveredTargetsOnly, out actualCastNode);
	}
}
