using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public class AbilityPatternRange : AbilityRange, IShowAoEAffectedUIHandler, ISubscriber
{
	private readonly List<GameObject> m_CellMarkers = new List<GameObject>();

	public GameObject CellMarker;

	private CustomGridNodeBase m_CachedCasterNode;

	private CustomGridNodeBase m_CachedTargetNode;

	private readonly List<AbilityTargetUIData> m_AbilityTargets = new List<AbilityTargetUIData>();

	private int MinRangeCells => Ability.MinRangeCells;

	private int MaxRangeCells => Ability.RangeCells;

	private IAbilityAoEPatternProvider PatternProvider => Ability.GetPatternSettings();

	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return PatternProvider != null;
		}
		return false;
	}

	protected override void SetRangeToWorldPosition(Vector3 desiredCastPosition, bool ignoreCache = false)
	{
		foreach (GameObject cellMarker in m_CellMarkers)
		{
			cellMarker.SetActive(value: false);
		}
		PointerController clickEventsController = Game.Instance.ClickEventsController;
		TargetWrapper target = Game.Instance.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, Ability, desiredCastPosition);
		Vector3 vector = ((target != null) ? target.Point : clickEventsController.WorldPosition);
		CustomGridNodeBase node;
		bool flag = Ability.TryGetCasterNodeForDistanceCalculation(out node, forUi: true);
		if (!flag)
		{
			node = Ability.GetBestShootingPosition(desiredCastPosition.GetNearestNodeXZUnwalkable(), vector);
		}
		CustomGridNodeBase customGridNodeBase = AoEPatternHelper.GetActualCastNode(Ability.Caster, node, vector, MinRangeCells, MaxRangeCells);
		if (Game.Instance.IsSpaceCombat)
		{
			customGridNodeBase = AoEPatternHelper.GetGridNode(AdjustTargetWithAngleRestriction(customGridNodeBase.Vector3Position));
		}
		if (!(m_CachedCasterNode != node || m_CachedTargetNode != customGridNodeBase || ignoreCache))
		{
			return;
		}
		OrientedPatternData orientedPattern;
		using (ProfileScope.New("GetOrientedPattern"))
		{
			orientedPattern = PatternProvider.GetOrientedPattern(Ability, node, customGridNodeBase);
		}
		m_CachedCasterNode = node;
		m_CachedTargetNode = customGridNodeBase;
		m_AbilityTargets.Clear();
		using (ProfileScope.New("GatherAffectedTargetsData"))
		{
			Ability.GatherAffectedTargetsData(orientedPattern, node.Vector3Position, target, in m_AbilityTargets);
		}
		int effectiveRange = ((Ability.Weapon != null) ? (MaxRangeCells / 2 + 1) : 0);
		Vector3 position = (flag ? node.Vector3Position : desiredCastPosition);
		NodeList nodes = Ability.Caster.GetOccupiedNodes(position);
		if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
		{
			bool flag2 = Ability.RestrictedFiringArc != RestrictedFiringArc.None;
			bool flag3 = Ability.Blueprint.ComponentsArray.HasItem((BlueprintComponent c) => c is WarhammerAbilityAttackDelivery warhammerAbilityAttackDelivery && warhammerAbilityAttackDelivery.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst);
			CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
			abilityAreaHudInfo.pattern = orientedPattern;
			abilityAreaHudInfo.casterRect = result;
			abilityAreaHudInfo.minRange = MinRangeCells;
			abilityAreaHudInfo.maxRange = MaxRangeCells;
			abilityAreaHudInfo.effectiveRange = effectiveRange;
			abilityAreaHudInfo.ignoreRangesByDefault = flag2 || !flag3;
			abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = false;
			abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
			CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
			CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
		}
		Vector3 casterPosition = (flag ? desiredCastPosition : node.Vector3Position);
		ObjectExtensions.Or(UnitPredictionManager.Instance, null)?.SetAbilityPositions(casterPosition, customGridNodeBase.Vector3Position);
		EventBus.RaiseEvent(delegate(ICellAbilityHandler h)
		{
			h.HandleCellAbility(m_AbilityTargets);
		});
	}

	private Vector3 AdjustTargetWithAngleRestriction(Vector3 target)
	{
		RestrictedFiringArc restrictedFiringArc = Ability.RestrictedFiringArc;
		if (restrictedFiringArc == RestrictedFiringArc.None)
		{
			return target;
		}
		Vector3 restrictedFiringArcDirection = GetRestrictedFiringArcDirection(Ability.Caster.View.ViewTransform.forward, restrictedFiringArc);
		int num = ((restrictedFiringArc == RestrictedFiringArc.Dorsal) ? 270 : 90);
		RestrictedFiringAreaComponent component = Ability.Blueprint.GetComponent<RestrictedFiringAreaComponent>();
		if (component != null)
		{
			num = Math.Min(num, component.RestrictedAngleDegrees);
		}
		num = Math.Max(num - PatternProvider.PatternAngle, 0);
		Vector3 position = Ability.Caster.View.ViewTransform.position;
		Vector3 to = target - position;
		int num2 = (int)Vector3.SignedAngle(restrictedFiringArcDirection, to, Vector3.up);
		if (num2 > num / 2)
		{
			target = position + Quaternion.AngleAxis(num / 2, Vector3.up) * restrictedFiringArcDirection * to.magnitude;
		}
		if (num2 < -num / 2)
		{
			target = position + Quaternion.AngleAxis(-num / 2, Vector3.up) * restrictedFiringArcDirection * to.magnitude;
		}
		return target;
	}

	private Vector3 GetRestrictedFiringArcDirection(Vector3 dir, RestrictedFiringArc arc)
	{
		switch (arc)
		{
		case RestrictedFiringArc.Port:
			dir = Quaternion.AngleAxis(-90f, Vector3.up) * dir;
			break;
		case RestrictedFiringArc.Starboard:
			dir = Quaternion.AngleAxis(90f, Vector3.up) * dir;
			break;
		case RestrictedFiringArc.Aft:
			dir = Quaternion.AngleAxis(180f, Vector3.up) * dir;
			break;
		}
		return CustomGraphHelper.AdjustDirection(dir);
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		foreach (GameObject cellMarker in m_CellMarkers)
		{
			cellMarker.SetActive(value: false);
		}
		m_CachedCasterNode = null;
		m_CachedTargetNode = null;
		CombatHUDRenderer.Instance.RemoveAbilityAreaHUD();
	}
}
