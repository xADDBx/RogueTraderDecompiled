using Code.Visual.Animation;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechadendrites;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

public class UnitAnimationController : BaseUnitController, IControllerStart, IController
{
	public void OnStart()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (!allUnit.IsSleeping)
			{
				TickOnUnit(allUnit);
				AbstractUnitEntityView abstractUnitEntityView = allUnit.View.Or(null);
				if ((object)abstractUnitEntityView != null)
				{
					abstractUnitEntityView.AnimationManager.Or(null)?.CustomUpdate(allUnit.Random.Range(0.5f, 1.5f));
				}
			}
		}
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitAnimationManager unitAnimationManager = unit.View.Or(null)?.AnimationManager;
		if ((object)unitAnimationManager != null)
		{
			TickManagerOnUnit(unitAnimationManager, unit, isMechadendrite: false);
		}
		UnitPartMechadendrites optional = unit.GetOptional<UnitPartMechadendrites>();
		if (optional == null)
		{
			return;
		}
		foreach (MechadendriteSettings value in optional.Mechadendrites.Values)
		{
			UnitAnimationManager animationManager = value.AnimationManager;
			if ((object)animationManager != null)
			{
				TickManagerOnUnit(animationManager, unit, isMechadendrite: true);
			}
		}
	}

	private static void TickManagerOnUnit(UnitAnimationManager manager, AbstractUnitEntity unit, bool isMechadendrite)
	{
		using (ProfileScope.New("Tick Animator", manager))
		{
			using (ProfileScope.New("Set Variables", manager))
			{
				PartUnitState stateOptional = unit.GetStateOptional();
				AbstractUnitCommand current = unit.Commands.Current;
				manager.WalkSpeedType = ((stateOptional != null && stateOptional.IsCharging) ? WalkSpeedType.Run : (current?.MovementType ?? WalkSpeedType.Walk));
				PartUnitStealth optional = unit.GetOptional<PartUnitStealth>();
				if (manager.WalkSpeedType == WalkSpeedType.Walk && optional != null && optional.Active && !optional.FullSpeed)
				{
					manager.WalkSpeedType = WalkSpeedType.Crouch;
				}
				float currentSpeedMps = unit.Movable.CurrentSpeedMps;
				if (manager.WalkSpeedType == WalkSpeedType.Walk && currentSpeedMps < 0.95f * unit.Blueprint.Speed.Meters / 2.5f)
				{
					manager.WalkSpeedType = WalkSpeedType.Walk;
				}
				manager.IsWaitingForIncomingAttackOfOpportunity = unit.IsWaitingForIncomingAttackOfOpportunity();
				manager.Speed = 1f;
				manager.IsStopping = unit.View.MovementAgent.IsStopping;
				UnitEntityView unitEntityView = unit.View as UnitEntityView;
				manager.IsInCombat = unit.IsInCombat || (unitEntityView != null && (unitEntityView.HandsEquipment?.InCombat ?? false));
				manager.IsMechadendrite = isMechadendrite;
				if (unit.GetOptional<UnitPartMechadendrites>() != null)
				{
					if (isMechadendrite)
					{
						WeaponAnimationStyle activeOffHandWeaponStyle = (manager.ActiveMainHandWeaponStyle = ((manager.IsInCombat && unitEntityView != null && unitEntityView.HandsEquipment != null && unitEntityView.HandsEquipment.Sets.Count > 0) ? ((unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: false }) ? unitEntityView.HandsEquipment.ActiveMainHandWeaponStyle : ((unitEntityView.HandsEquipment.GetSelectedWeaponSet().OffHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: false }) ? unitEntityView.HandsEquipment.ActiveOffHandWeaponStyle : WeaponAnimationStyle.None)) : WeaponAnimationStyle.None));
						manager.ActiveOffHandWeaponStyle = activeOffHandWeaponStyle;
					}
					else
					{
						WeaponAnimationStyle activeOffHandWeaponStyle2 = (manager.ActiveMainHandWeaponStyle = ((manager.IsInCombat && unitEntityView != null && unitEntityView.HandsEquipment != null && unitEntityView.HandsEquipment.Sets.Count > 0) ? ((unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: not false }) ? unitEntityView.HandsEquipment.ActiveMainHandWeaponStyle : ((unitEntityView.HandsEquipment.GetSelectedWeaponSet().OffHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: not false }) ? unitEntityView.HandsEquipment.ActiveOffHandWeaponStyle : WeaponAnimationStyle.None)) : WeaponAnimationStyle.None));
						manager.ActiveOffHandWeaponStyle = activeOffHandWeaponStyle2;
					}
				}
				else
				{
					manager.ActiveMainHandWeaponStyle = ((manager.IsInCombat && unitEntityView != null && unitEntityView.HandsEquipment != null) ? unitEntityView.HandsEquipment.ActiveMainHandWeaponStyle : WeaponAnimationStyle.None);
					manager.ActiveOffHandWeaponStyle = ((manager.IsInCombat && unitEntityView != null && unitEntityView.HandsEquipment != null) ? unitEntityView.HandsEquipment.ActiveOffHandWeaponStyle : WeaponAnimationStyle.None);
				}
				manager.Orientation = unit.Orientation;
				manager.IsDead = unit.LifeState.IsDead;
				manager.IsProne = stateOptional?.IsProne ?? false;
				manager.IsSleeping = stateOptional?.HasCondition(UnitCondition.Sleeping) ?? false;
				manager.IsStunned = false;
				manager.IsAnimating = unit is LightweightUnitEntity || (stateOptional != null && stateOptional.IsAnimating) || manager.IsGoingProne;
				manager.IsUnconscious = unit.LifeState.IsUnconscious;
				manager.CoverType = ((manager.IsInCombat && unit is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.GetCoverType() : LosCalculations.CoverType.None);
				manager.ForceMoveDistance = (unit.GetOptional<UnitPartForceMove>()?.Active?.RemainingDistance).GetValueOrDefault();
				manager.ForceMoveTime = (manager.GetAction(UnitAnimationType.ForceMove) as UnitAnimationActionForceMove)?.GetClipsLength();
				manager.DodgeDistance = ((unit.Commands.Current is UnitJumpAsideDodge { ForcedPath: not null } unitJumpAsideDodge) ? unitJumpAsideDodge.ForcedPath.GetTotalLength() : 0f);
				manager.CombatMicroIdle = (unit.Commands.Empty ? CombatMicroIdle.Weapon : CombatMicroIdle.None);
			}
			float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;
			manager.Tick(gameDeltaTime);
		}
	}

	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.IsInFogOfWar)
		{
			return false;
		}
		return true;
	}
}
