using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipHealthBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAbilityTargetSelectionUIHandler, ISubscriber
{
	public readonly UnitState UnitState;

	public readonly ReactiveProperty<int> MinDamage = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> MaxDamage = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<bool> CanDie = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<int> HitPointLeft = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> HitPointTemporary = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> HitPointTotalLeft = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> HitPointMax = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> HitPointTotalMax = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShieldMaxValue = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> Shield = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShieldMinDamage = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> ShieldMaxDamage = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_HasAbility = new ReactiveProperty<bool>();

	private AbilityData m_Ability;

	private MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	private MechanicEntityUIWrapper UnitUIWrapper => UnitState.Unit;

	public bool HideRealHealthInUI
	{
		get
		{
			if (Unit != null)
			{
				return Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI);
			}
			return false;
		}
	}

	public bool IsPLayerEnemy => UnitUIWrapper.IsPlayerEnemy;

	public OvertipHealthBlockVM(UnitState unitState)
	{
		UnitState = unitState;
		AddDisposable(EventBus.Subscribe(this));
		UpdateProperties(initial: true);
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateProperties();
		}));
		AddDisposable(m_HasAbility.CombineLatest(UnitState.IsMouseOverUnit, UnitState.IsAoETarget, (bool hasAbility, bool isHover, bool isAoE) => hasAbility && (isHover || (isAoE && !UnitState.IsStarshipAttack.Value))).Subscribe(CollectAbilityProperty));
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateProperties(bool initial = false)
	{
		if (Unit != null && (!Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI) || initial))
		{
			int maxHitPoints = UnitUIWrapper.Health.MaxHitPoints;
			HitPointMax.Value = maxHitPoints;
			HitPointTotalMax.Value = (HideRealHealthInUI ? maxHitPoints : (maxHitPoints + UnitUIWrapper.Health.TemporaryHitPoints));
			HitPointLeft.Value = (HideRealHealthInUI ? maxHitPoints : UnitUIWrapper.Health.HitPointsLeft);
			HitPointTemporary.Value = (HideRealHealthInUI ? maxHitPoints : UnitUIWrapper.Health.TemporaryHitPoints);
			HitPointTotalLeft.Value = (HideRealHealthInUI ? maxHitPoints : (UnitUIWrapper.Health.HitPointsLeft + UnitUIWrapper.Health.TemporaryHitPoints));
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_Ability = ability;
		m_HasAbility.Value = true;
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_Ability = null;
		m_HasAbility.Value = false;
	}

	private void CollectAbilityProperty(bool show)
	{
		if (!show || Unit.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			MinDamage.Value = 0;
			MaxDamage.Value = 0;
			CanDie.Value = false;
			ShieldMaxValue.Value = 0;
			Shield.Value = 0;
			ShieldMaxDamage.Value = 0;
			ShieldMinDamage.Value = 0;
			return;
		}
		int num;
		object obj;
		if (!UnitState.IsAoETarget.Value)
		{
			num = (m_Ability.CanTargetFromDesiredPosition(Unit) ? 1 : 0);
			if (num == 0)
			{
				obj = null;
				goto IL_00c4;
			}
		}
		else
		{
			num = 1;
		}
		obj = m_Ability.GetDamagePrediction(Unit, Game.Instance.VirtualPositionController.GetDesiredPosition(m_Ability.Caster));
		goto IL_00c4;
		IL_00c4:
		DamagePredictionData damagePredictionData = (DamagePredictionData)obj;
		HealPredictionData healPredictionData = ((num != 0) ? m_Ability.GetHealPrediction(Unit) : null);
		if (damagePredictionData != null)
		{
			MinDamage.Value = damagePredictionData.MinDamage;
			MaxDamage.Value = damagePredictionData.MaxDamage;
			CanDie.Value = damagePredictionData.MaxDamage >= HitPointTotalLeft.Value;
		}
		else if (healPredictionData != null)
		{
			MinDamage.Value = -healPredictionData.MinValue;
			MaxDamage.Value = -healPredictionData.MaxValue;
			CanDie.Value = false;
		}
		else
		{
			MinDamage.Value = 0;
			MaxDamage.Value = 0;
			CanDie.Value = false;
		}
		if (IsPLayerEnemy)
		{
			UpdateEnemyShields();
		}
	}

	private void UpdateEnemyShields()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ShieldMaxValue.Value = 0;
			Shield.Value = 0;
			ShieldMaxDamage.Value = 0;
			ShieldMinDamage.Value = 0;
			if (Unit is StarshipEntity starshipEntity && m_Ability.StarshipWeapon != null)
			{
				ShieldDamageData item = AbilityDataHelper.GetStarshipDamagePrediction(starshipEntity, starshipEntity.Position, m_Ability, m_Ability.StarshipWeapon).resultShields;
				ShieldMaxValue.Value = item.MaxShield;
				Shield.Value = item.CurrentShield;
				ShieldMaxDamage.Value = item.MaxDamage;
				ShieldMinDamage.Value = item.MinDamage;
			}
		}
	}
}
