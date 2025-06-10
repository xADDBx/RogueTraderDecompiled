using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public class AbilitySingleTargetRange : AbilityRange, IShowAoEAffectedUIHandler, ISubscriber
{
	private Vector3 _cachedCasterPosition;

	private Vector3 _cachedTargetPosition;

	private readonly List<AbilityTargetUIData> m_AbilityTargets = new List<AbilityTargetUIData>();

	private int MinRangeCells => Ability.MinRangeCells;

	private int MaxRangeCells => Ability.RangeCells;

	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return Ability.GetPatternSettings() == null;
		}
		return false;
	}

	protected override void SetRangeToWorldPosition(Vector3 desiredCastPosition, bool ignoreCache = false)
	{
		PointerController clickEventsController = Game.Instance.ClickEventsController;
		TargetWrapper target = Game.Instance.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability, desiredCastPosition);
		Vector3 vector = ((target != null) ? target.Point : Game.Instance.ClickEventsController.WorldPosition);
		MechanicEntity caster;
		bool flag = Ability.TryGetCasterForDistanceCalculation(out caster, forUi: true);
		if (caster == null)
		{
			caster = Ability.Caster;
		}
		CustomGridNodeBase customGridNodeBase = (flag ? caster.CurrentUnwalkableNode : Ability.GetBestShootingPosition(desiredCastPosition.GetNearestNodeXZUnwalkable(), vector));
		Vector3 gridAdjustedPosition = AoEPatternHelper.GetGridAdjustedPosition(flag ? customGridNodeBase.Vector3Position : desiredCastPosition);
		Vector3 actualCastPosition = AoEPatternHelper.GetActualCastPosition(caster, gridAdjustedPosition, vector, MinRangeCells, MaxRangeCells);
		float sqrMagnitude = (_cachedCasterPosition - gridAdjustedPosition).sqrMagnitude;
		float sqrMagnitude2 = (_cachedTargetPosition - actualCastPosition).sqrMagnitude;
		if (!((double)sqrMagnitude > 1E-05) && !((double)sqrMagnitude2 > 1E-05))
		{
			return;
		}
		_cachedCasterPosition = gridAdjustedPosition;
		_cachedTargetPosition = actualCastPosition;
		int effectiveRange = ((Ability.Weapon != null) ? Mathf.FloorToInt((float)MaxRangeCells / 2f) : 0);
		bool hasFiringArc = Ability.RestrictedFiringArc != RestrictedFiringArc.None;
		Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
		bool ignoreRangesByDefault = false;
		OrientedPatternData pattern = ((target != null && Ability.CanRedirectFromTarget(target)) ? Ability.RedirectPattern.GetOriented(customGridNodeBase, target.NearestNode, currentUnitDirection) : GetPatternData(hasFiringArc, gridAdjustedPosition, currentUnitDirection, target, out ignoreRangesByDefault, customGridNodeBase));
		NodeList nodes = caster.GetOccupiedNodes(flag ? customGridNodeBase.Vector3Position : desiredCastPosition);
		if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
		{
			CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
			abilityAreaHudInfo.pattern = pattern;
			abilityAreaHudInfo.casterRect = result;
			abilityAreaHudInfo.minRange = MinRangeCells;
			abilityAreaHudInfo.maxRange = MaxRangeCells;
			abilityAreaHudInfo.effectiveRange = effectiveRange;
			abilityAreaHudInfo.ignoreRangesByDefault = ignoreRangesByDefault;
			abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = Ability.IsStarshipAttack;
			abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
			CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
			CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
		}
		UnitPredictionManager.Instance.Or(null)?.SetAbilityPositions(flag ? desiredCastPosition : customGridNodeBase.Vector3Position, actualCastPosition);
		bool flag2 = false;
		m_AbilityTargets.Clear();
		if (target != null && !Ability.IsVariable)
		{
			if (Ability.CanRedirectFromTarget(target))
			{
				flag2 = true;
				foreach (MechanicEntity item in Ability.CalculateRedirectTargets(target))
				{
					m_AbilityTargets.Add(new AbilityTargetUIData(Ability, item, gridAdjustedPosition, isAbilityRedirected: true));
				}
			}
			else
			{
				Ability.GatherAffectedTargetsData(pattern, desiredCastPosition, target, in m_AbilityTargets);
			}
		}
		if (!flag2 && pattern.IsEmpty)
		{
			MechanicEntity targetEntity = target?.Entity;
			if (targetEntity != null && !m_AbilityTargets.HasItem((AbilityTargetUIData i) => i.Target == targetEntity))
			{
				m_AbilityTargets.Add(new AbilityTargetUIData(Ability, targetEntity, gridAdjustedPosition));
			}
		}
		EventBus.RaiseEvent(delegate(ICellAbilityHandler h)
		{
			h.HandleCellAbility(m_AbilityTargets);
		});
	}

	private OrientedPatternData GetPatternData(bool hasFiringArc, Vector3 casterPosition, Vector3 casterDirection, TargetWrapper target, out bool ignoreRangesByDefault, CustomGridNodeBase overrideCasterNode = null)
	{
		ignoreRangesByDefault = false;
		if (hasFiringArc)
		{
			HashSet<CustomGridNodeBase> restrictedFiringArcNodes = Ability.GetRestrictedFiringArcNodes(casterPosition.GetNearestNodeXZUnwalkable(), CustomGraphHelper.GuessDirection(casterDirection));
			ignoreRangesByDefault = true;
			return new OrientedPatternData(restrictedFiringArcNodes, restrictedFiringArcNodes.FirstOrDefault());
		}
		if (Ability.IsSingleShot)
		{
			ReadonlyList<CustomGridNodeBase> singleShotAffectedNodes = Ability.GetSingleShotAffectedNodes(target);
			return new OrientedPatternData(singleShotAffectedNodes, singleShotAffectedNodes.FirstOrDefault());
		}
		if (Ability.IsChainLighting())
		{
			HashSet<CustomGridNodeBase> chainLightingTargets = Ability.GetChainLightingTargets(target);
			return new OrientedPatternData(chainLightingTargets, chainLightingTargets.FirstOrDefault());
		}
		PartAbilityPredictionForAreaEffect partAbilityPredictionForAreaEffect = Ability.TryGetPatternDataFromAreaEffect();
		if (partAbilityPredictionForAreaEffect != null)
		{
			OrientedPatternData? areaEffectPatternNotFromPatternCenter = partAbilityPredictionForAreaEffect.GetAreaEffectPatternNotFromPatternCenter(Ability, target ?? ((TargetWrapper)_cachedTargetPosition), overrideCasterNode);
			ignoreRangesByDefault = areaEffectPatternNotFromPatternCenter.HasValue;
			return areaEffectPatternNotFromPatternCenter ?? OrientedPatternData.Empty;
		}
		return OrientedPatternData.Empty;
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		_cachedCasterPosition = Vector3.zero;
		_cachedTargetPosition = Vector3.zero;
		CombatHUDRenderer.Instance.RemoveAbilityAreaHUD();
	}
}
