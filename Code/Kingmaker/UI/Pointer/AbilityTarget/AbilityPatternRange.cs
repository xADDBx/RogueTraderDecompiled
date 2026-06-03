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

	private OrientedPatternData m_SavedPattern = OrientedPatternData.Empty;

	private OrientedPatternData m_CurrentPattern = OrientedPatternData.Empty;

	private readonly List<AbilityTargetUIData> m_AbilityTargets = new List<AbilityTargetUIData>();

	private readonly List<AbilityTargetUIData> m_SavedAbilityTargets = new List<AbilityTargetUIData>();

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

	public override void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		base.HandleAbilityTargetSelectionStart(ability);
		if (Ability.ShouldKeepPreviousAoEPatternOnUi())
		{
			m_SavedPattern = m_CurrentPattern;
			m_CurrentPattern = OrientedPatternData.Empty;
			for (int i = 0; i < m_AbilityTargets.Count; i++)
			{
				if (i < m_SavedAbilityTargets.Count)
				{
					m_SavedAbilityTargets[i] = m_AbilityTargets[i].CopyWithOverride(ability);
				}
				else
				{
					m_SavedAbilityTargets.Add(m_AbilityTargets[i].CopyWithOverride(ability));
				}
			}
		}
		else
		{
			ClearSavedAbilityValues();
		}
	}

	public override void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		base.HandleAbilityTargetSelectionEnd(ability);
		ClearSavedAbilityValues();
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
		CustomGridNodeBase node2;
		CustomGridNodeBase customGridNodeBase = (Ability.TryGetCastNodeOverride(node, out node2) ? node2 : node);
		CustomGridNodeBase customGridNodeBase2 = AoEPatternHelper.GetActualCastNode(Ability.Caster, customGridNodeBase, vector, MinRangeCells, MaxRangeCells);
		if (Game.Instance.IsSpaceCombat)
		{
			customGridNodeBase2 = AoEPatternHelper.GetGridNode(AdjustTargetWithAngleRestriction(customGridNodeBase2.Vector3Position));
		}
		if (!(m_CachedCasterNode != customGridNodeBase || m_CachedTargetNode != customGridNodeBase2 || ignoreCache))
		{
			return;
		}
		OrientedPatternData pattern;
		using (ProfileScope.New("GetOrientedPattern"))
		{
			pattern = PatternProvider.GetOrientedPattern(Ability, customGridNodeBase, customGridNodeBase2);
		}
		if (!m_SavedPattern.IsEmpty)
		{
			using (ProfileScope.New("CombineCurrentPatternWithSaved"))
			{
				m_CurrentPattern = CombineCurrentPatternWithSaved(in pattern);
			}
		}
		else
		{
			m_CurrentPattern = pattern;
		}
		m_CachedCasterNode = customGridNodeBase;
		m_CachedTargetNode = customGridNodeBase2;
		m_AbilityTargets.Clear();
		using (ProfileScope.New("GatherAffectedTargetsData"))
		{
			Ability.GatherAffectedTargetsData(pattern, node.Vector3Position, target, in m_AbilityTargets);
			CombineAbilityTargetsWithSaved(m_AbilityTargets);
		}
		int effectiveRange = ((Ability.Weapon != null) ? (MaxRangeCells / 2 + 1) : 0);
		Vector3 position = (flag ? node.Vector3Position : desiredCastPosition);
		NodeList nodes = Ability.Caster.GetOccupiedNodes(position);
		if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
		{
			bool flag2 = Ability.RestrictedFiringArc != RestrictedFiringArc.None;
			bool flag3 = Ability.Blueprint.ComponentsArray.HasItem((BlueprintComponent c) => c is WarhammerAbilityAttackDelivery warhammerAbilityAttackDelivery && warhammerAbilityAttackDelivery.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst);
			CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
			abilityAreaHudInfo.pattern = m_CurrentPattern;
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
		ObjectExtensions.Or(UnitPredictionManager.Instance, null)?.SetAbilityPositions(casterPosition, customGridNodeBase2.Vector3Position);
		EventBus.RaiseEvent(delegate(ICellAbilityHandler h)
		{
			h.HandleCellAbility(m_AbilityTargets);
		});
	}

	private OrientedPatternData CombineCurrentPatternWithSaved(in OrientedPatternData pattern)
	{
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		foreach (CustomGridNodeBase node in m_SavedPattern.Nodes)
		{
			hashSet.Add(node);
		}
		foreach (CustomGridNodeBase node2 in pattern.Nodes)
		{
			hashSet.Add(node2);
		}
		return new OrientedPatternData(hashSet, m_SavedPattern.ApplicationNode);
	}

	private void CombineAbilityTargetsWithSaved(List<AbilityTargetUIData> abilityTargets)
	{
		foreach (AbilityTargetUIData savedAbilityTarget in m_SavedAbilityTargets)
		{
			bool flag = false;
			for (int i = 0; i < abilityTargets.Count; i++)
			{
				AbilityTargetUIData abilityTargetUIData = abilityTargets[i];
				if (abilityTargetUIData.Target == savedAbilityTarget.Target)
				{
					int index = i;
					int? minDamageOverride = savedAbilityTarget.MinDamage + abilityTargetUIData.MinDamage;
					int? maxDamageOverride = savedAbilityTarget.MaxDamage + abilityTargetUIData.MaxDamage;
					abilityTargets[index] = savedAbilityTarget.CopyWithOverride(null, null, null, null, null, null, minDamageOverride, maxDamageOverride);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				abilityTargets.Add(savedAbilityTarget);
			}
		}
	}

	private void ClearSavedAbilityValues()
	{
		m_CurrentPattern = OrientedPatternData.Empty;
		m_SavedPattern = OrientedPatternData.Empty;
		m_SavedAbilityTargets.Clear();
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
