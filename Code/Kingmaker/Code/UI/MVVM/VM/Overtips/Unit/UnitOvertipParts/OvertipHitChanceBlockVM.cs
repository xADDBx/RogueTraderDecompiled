using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipHitChanceBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICellAbilityHandler, ISubscriber
{
	public readonly UnitState UnitState;

	public readonly ReactiveProperty<bool> HasHit = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCaster = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> HitChance = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<bool> HitAlways = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> InitialHitChance = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<int> BurstIndex = new ReactiveProperty<int>(0);

	public readonly AutoDisposingReactiveCollection<HitChanceEntityVM> BurstHitChancesCollection = new AutoDisposingReactiveCollection<HitChanceEntityVM>();

	public readonly ReactiveProperty<int> MinDamage = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> MaxDamage = new ReactiveProperty<int>(0);

	public bool CanDie;

	public readonly ReactiveProperty<bool> CanPush = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> DodgeChance = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> ParryChance = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CoverChance = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> EvasionChance = new ReactiveProperty<float>(0f);

	public readonly BoolReactiveProperty IsVisibleTrigger = new BoolReactiveProperty();

	private ReactiveProperty<AbilityTargetUIData> m_AbilityTargetUIData = new ReactiveProperty<AbilityTargetUIData>();

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	private MechanicEntityUIWrapper UnitUIWrapper => UnitState.Unit;

	public OvertipHitChanceBlockVM(UnitState unitState)
	{
		UnitState = unitState;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(UnitState.IsInCombat.CombineLatest(UnitState.IsVisibleForPlayer, UnitState.IsDeadOrUnconsciousIsDead, UnitState.Ability, UnitState.IsMouseOverUnit, UnitState.IsAoETarget, (bool isInCombat, bool isVisibleForPlayer, bool isDead, AbilityData ability, bool isHover, bool isAoETarget) => isInCombat && isVisibleForPlayer && !isDead && ability != null && (isHover || (isAoETarget && !UnitState.IsStarshipAttack.Value))).ObserveLastValueOnLateUpdate().Subscribe(delegate(bool value)
		{
			IsVisibleTrigger.Value = value;
		}));
		AddDisposable(IsVisibleTrigger.CombineLatest(m_AbilityTargetUIData, (bool visible, AbilityTargetUIData uiData) => visible).ObserveLastValueOnLateUpdate().Subscribe(OnVisibilityChanged));
		if (unitState.Unit.MechanicEntity.CanBeAttackedDirectly)
		{
			AddDisposable(UnitState.Ability.Subscribe(delegate
			{
				UpdateSingleSelectedAbilityProperty();
			}));
		}
	}

	protected override void DisposeImplementation()
	{
		ClearProperties();
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		AbilityTargetUIData abilityTargetUIData = abilityTargets.FirstOrDefault((AbilityTargetUIData t) => t.Target == Unit);
		if (!(abilityTargetUIData == default(AbilityTargetUIData)) && !m_AbilityTargetUIData.Value.Equals(abilityTargetUIData))
		{
			m_AbilityTargetUIData.SetValueAndForceNotify(abilityTargetUIData);
		}
	}

	private void UpdateSingleSelectedAbilityProperty()
	{
		if (UnitState.IsInCombat.Value && !UnitState.IsDeadOrUnconsciousIsDead.Value && !(UnitState.Ability.Value == null) && UnitState.Ability.Value.TargetAnchor == AbilityTargetAnchor.Unit && !UnitState.Ability.Value.IsChainLighting())
		{
			AbilityTargetUIData orCreate = AbilityTargetUIDataCache.Instance.GetOrCreate(UnitState.Ability.Value, Unit, Game.Instance.VirtualPositionController.GetDesiredPosition(UnitState.Ability.Value.Caster));
			m_AbilityTargetUIData.SetValueAndForceNotify(orCreate);
		}
	}

	private void OnVisibilityChanged(bool state)
	{
		if (!state)
		{
			ClearProperties();
		}
		else
		{
			UpdateProperties();
		}
	}

	private void UpdateProperties()
	{
		if (UnitState.Ability.Value == null)
		{
			ClearProperties();
			return;
		}
		ClearBurstHitChances();
		TargetWrapper targetForDesiredPosition = Game.Instance.SelectedAbilityHandler.GetTargetForDesiredPosition(Unit.View.gameObject, Game.Instance.ClickEventsController.WorldPosition);
		bool flag = targetForDesiredPosition != null && UnitState.Ability.Value.CanTargetFromDesiredPosition(targetForDesiredPosition) && CanAoETarget(UnitState.Ability.Value.Blueprint.AoETargets);
		HasHit.Value = flag;
		IsCaster.Value = UnitState.IsCaster.Value;
		if (!flag)
		{
			return;
		}
		HitAlways.Value = m_AbilityTargetUIData.Value.HitAlways;
		CanDie = m_AbilityTargetUIData.Value.MaxDamage >= UnitUIWrapper.Health.HitPointsLeft + UnitUIWrapper.Health.TemporaryHitPoints;
		BurstIndex.Value = m_AbilityTargetUIData.Value.BurstIndex;
		HitChance.Value = m_AbilityTargetUIData.Value.HitWithAvoidanceChance;
		InitialHitChance.Value = m_AbilityTargetUIData.Value.InitialHitChance;
		MinDamage.Value = m_AbilityTargetUIData.Value.MinDamage;
		MaxDamage.Value = m_AbilityTargetUIData.Value.MaxDamage;
		DodgeChance.Value = m_AbilityTargetUIData.Value.DodgeChance;
		ParryChance.Value = m_AbilityTargetUIData.Value.ParryChance;
		CoverChance.Value = m_AbilityTargetUIData.Value.CoverChance;
		EvasionChance.Value = m_AbilityTargetUIData.Value.EvasionChance;
		CanPush.Value = m_AbilityTargetUIData.Value.CanPush;
		if (m_AbilityTargetUIData.Value.BurstHitChances != null)
		{
			for (int i = 0; i < m_AbilityTargetUIData.Value.BurstHitChances.Count; i++)
			{
				HitChanceEntityVM hitChanceEntityVM = new HitChanceEntityVM(i, m_AbilityTargetUIData.Value.BurstHitChances[i], i == m_AbilityTargetUIData.Value.BurstHitChances.Count - 1);
				AddDisposable(hitChanceEntityVM);
				BurstHitChancesCollection.Add(hitChanceEntityVM);
			}
		}
	}

	private void ClearProperties()
	{
		if (HasHit.Value)
		{
			HitAlways.Value = false;
			HitChance.Value = 0f;
			InitialHitChance.Value = 0f;
			BurstIndex.Value = 0;
			CanDie = false;
			MinDamage.Value = 0;
			MaxDamage.Value = 0;
			DodgeChance.Value = 0f;
			ParryChance.Value = 0f;
			CoverChance.Value = 0f;
			EvasionChance.Value = 0f;
			CanPush.Value = false;
			HasHit.Value = false;
			ClearBurstHitChances();
		}
	}

	private void ClearBurstHitChances()
	{
		BurstHitChancesCollection.Clear();
	}

	private bool CanAoETarget(TargetType targetType)
	{
		switch (targetType)
		{
		case TargetType.Enemy:
			return UnitUIWrapper.IsPlayerEnemy;
		case TargetType.Ally:
			if (!UnitUIWrapper.IsPlayer)
			{
				return UnitUIWrapper.IsPlayerFaction;
			}
			return true;
		case TargetType.Any:
			return true;
		default:
			return false;
		}
	}
}
