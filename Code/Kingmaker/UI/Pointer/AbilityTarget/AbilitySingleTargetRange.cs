using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
		if (base.CanEnable() && Ability.Blueprint.Range != Kingmaker.UnitLogic.Abilities.Blueprints.AbilityRange.Unlimited)
		{
			return Ability.GetPatternSettings() == null;
		}
		return false;
	}

	protected override void SetRangeToWorldPosition(Vector3 castPosition)
	{
		PointerController clickEventsController = Game.Instance.ClickEventsController;
		TargetWrapper target = Game.Instance.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability, castPosition);
		Vector3 target2 = ((target != null) ? target.Point : Game.Instance.ClickEventsController.WorldPosition);
		Vector3 gridAdjustedPosition = AoEPatternHelper.GetGridAdjustedPosition(castPosition);
		Vector3 actualCastPosition = AoEPatternHelper.GetActualCastPosition(Ability.Caster, gridAdjustedPosition, target2, MinRangeCells, MaxRangeCells);
		float sqrMagnitude = (_cachedCasterPosition - gridAdjustedPosition).sqrMagnitude;
		float sqrMagnitude2 = (_cachedTargetPosition - actualCastPosition).sqrMagnitude;
		if ((double)sqrMagnitude > 1E-05 || (double)sqrMagnitude2 > 1E-05)
		{
			_cachedCasterPosition = gridAdjustedPosition;
			_cachedTargetPosition = actualCastPosition;
			int effectiveRange = ((Ability.Weapon != null) ? Mathf.FloorToInt((float)MaxRangeCells / 2f) : 0);
			bool hasFiringArc = Ability.RestrictedFiringArc != RestrictedFiringArc.None;
			Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
			bool ignoreRangesByDefault;
			OrientedPatternData patternData = GetPatternData(hasFiringArc, gridAdjustedPosition, currentUnitDirection, target, out ignoreRangesByDefault);
			NodeList nodes = Ability.Caster.GetOccupiedNodes(Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster));
			if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
			{
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
				abilityAreaHudInfo.pattern = patternData;
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
			UnitPredictionManager.Instance.Or(null)?.SetAbilityArea(gridAdjustedPosition, actualCastPosition, patternData);
			m_AbilityTargets.Clear();
			Ability.GatherAffectedTargetsData(patternData, castPosition, in m_AbilityTargets);
			EventBus.RaiseEvent(delegate(ICellAbilityHandler h)
			{
				h.HandleCellAbility(m_AbilityTargets);
			});
		}
	}

	private OrientedPatternData GetPatternData(bool hasFiringArc, Vector3 casterPosition, Vector3 casterDirection, TargetWrapper target, out bool ignoreRangesByDefault)
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
			List<CustomGridNodeBase> singleShotAffectedNodes = Ability.GetSingleShotAffectedNodes(target);
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
			ignoreRangesByDefault = true;
			return partAbilityPredictionForAreaEffect.GetAreaEffectPatternNotFromPatternCenter(Ability, target ?? ((TargetWrapper)_cachedTargetPosition)) ?? OrientedPatternData.Empty;
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
