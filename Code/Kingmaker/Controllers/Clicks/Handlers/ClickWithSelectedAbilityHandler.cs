using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickWithSelectedAbilityHandler : IClickEventHandler
{
	public class ReturnToOriginPart : BaseUnitPart, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
	{
		[JsonProperty(IsReference = false)]
		public Vector3 Origin;

		public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
		{
			if (base.Owner == command.Executor && command.Executor.Commands.Queue.Empty())
			{
				base.Owner.Position = Origin;
				base.Owner.Remove<ReturnToOriginPart>();
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Origin);
			return result;
		}
	}

	public AbilityData RootAbility { get; private set; }

	public AbilityData Ability { get; private set; }

	public AbilityMultiTargetSelectionHandler MultiTargetHandler { get; } = new AbilityMultiTargetSelectionHandler();


	public PointerMode GetMode()
	{
		return PointerMode.Ability;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (Ability == null)
		{
			return 0f;
		}
		if (Input.GetMouseButtonUp(1))
		{
			return 1f;
		}
		bool flag = gameObject.IsLayerMask(Layers.WalkableMask);
		if (Ability.TargetAnchor == AbilityTargetAnchor.Point)
		{
			bool flag2 = KeyboardAccess.IsCtrlHold();
			if (!SettingsRoot.Controls.ConvertSnapLogic)
			{
				flag2 = !flag2;
			}
			if (flag2 && !flag)
			{
				return 0f;
			}
		}
		TargetWrapper targetForDesiredPosition = GetTargetForDesiredPosition(gameObject, worldPosition);
		UnitPartPersonalEnemy unitPartPersonalEnemy = targetForDesiredPosition?.Entity?.GetOptional<UnitPartPersonalEnemy>();
		if (unitPartPersonalEnemy != null && !unitPartPersonalEnemy.IsCurrentlyTargetable)
		{
			return 0f;
		}
		if (targetForDesiredPosition != null && Ability.CanTargetFromDesiredPosition(targetForDesiredPosition) && targetForDesiredPosition.Entity != null)
		{
			float result = (Ability.Blueprint.CanTargetPoint ? 1f : 0f);
			bool isDeadOrUnconscious = targetForDesiredPosition.Entity.IsDeadOrUnconscious;
			if (isDeadOrUnconscious && Ability.Blueprint.CanCastToAliveTarget())
			{
				return result;
			}
			if (!isDeadOrUnconscious && Ability.Blueprint.CanCastToDeadTarget)
			{
				return result;
			}
			if (targetForDesiredPosition.Entity.IsEnemy(Ability.Caster) && !Ability.Blueprint.CanTargetEnemies)
			{
				return result;
			}
			if (!targetForDesiredPosition.Entity.IsEnemy(Ability.Caster) && !Ability.Blueprint.CanTargetFriends && !Ability.CanRedirectFromTarget(targetForDesiredPosition.Entity))
			{
				return result;
			}
			AbilityCanTargetOnlyPetUnits canTargetOnlyPetUnitsComponent = Ability.Blueprint.CanTargetOnlyPetUnitsComponent;
			if (canTargetOnlyPetUnitsComponent != null)
			{
				if (canTargetOnlyPetUnitsComponent.Inverted)
				{
					if (targetForDesiredPosition.Entity is BaseUnitEntity { IsPet: not false })
					{
						return result;
					}
				}
				else
				{
					if (!(targetForDesiredPosition.Entity is BaseUnitEntity { IsPet: not false } baseUnitEntity2))
					{
						return result;
					}
					if (canTargetOnlyPetUnitsComponent.CanTargetOnlyOwnersPet && baseUnitEntity2.Master != Ability.Caster)
					{
						return result;
					}
				}
			}
			return 2f;
		}
		return 1f;
	}

	public TargetWrapper GetTargetForDesiredPosition(GameObject gameObject, Vector3 worldPosition)
	{
		return GetTarget(gameObject, worldPosition, Ability, Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster));
	}

	[CanBeNull]
	public TargetWrapper GetTarget(GameObject gameObject, Vector3 worldPosition, AbilityData ability, Vector3 casterPosition)
	{
		if (ability == null)
		{
			return null;
		}
		MechanicEntityView obj = ((gameObject != null) ? gameObject.GetComponentNonAlloc<MechanicEntityView>() : null);
		MechanicEntity mechanicEntity = obj.Or(null)?.Data;
		if (obj != null)
		{
			BaseUnitEntity baseUnitEntity = gameObject.GetComponentNonAlloc<MapObjectView>().Or(null)?.Data.GetOptional<InvisibleKittenHolderPart>()?.GetKitten();
			if (baseUnitEntity != null)
			{
				mechanicEntity = baseUnitEntity;
			}
		}
		if (mechanicEntity == null)
		{
			mechanicEntity = worldPosition.GetNearestNodeXZUnwalkable()?.GetUnit();
		}
		switch (ability.TargetAnchor)
		{
		case AbilityTargetAnchor.Owner:
			return ability.Caster;
		case AbilityTargetAnchor.Unit:
			if (mechanicEntity == null || !mechanicEntity.CanBeAttackedDirectly)
			{
				return null;
			}
			return new TargetWrapper(mechanicEntity);
		case AbilityTargetAnchor.Point:
		{
			CustomGridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
			Vector3 vector = worldPosition;
			vector = ((ability.GetPatternSettings() == null) ? AoEPatternHelper.GetGridAdjustedPosition(vector) : AoEPatternHelper.GetActualCastPosition(ability.Caster, nearestNodeXZUnwalkable, vector, ability.MinRangeCells, ability.RangeCells));
			Vector3 vector2 = vector - nearestNodeXZUnwalkable.Vector3Position;
			Quaternion quaternion = ((vector2 != Vector3.zero) ? Quaternion.LookRotation(vector2) : Quaternion.identity);
			MechanicEntity entity = ((mechanicEntity != null && mechanicEntity.CanBeAttackedDirectly && (ability.IsCharge || !ability.Blueprint.CanTargetPoint)) ? mechanicEntity : null);
			return new TargetWrapper(vector, quaternion.eulerAngles.y, entity);
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		if (Ability == null)
		{
			return false;
		}
		Vector3 desiredPosition = Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster);
		TargetWrapper target = GetTarget(gameObject, worldPosition, Ability, desiredPosition);
		if (ShouldHandleAbilityCastFail(target, out var unavailabilityReason))
		{
			if (unavailabilityReason.HasValue)
			{
				string restrictionText = Ability.GetUnavailabilityReasonString(unavailabilityReason.Value, desiredPosition, target);
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(restrictionText, addToLog: false, WarningNotificationFormat.Attention);
				});
				EventBus.RaiseEvent(delegate(IClickActionHandler h)
				{
					h.OnAbilityCastRefused(Ability, target, null);
				});
				UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
				return false;
			}
			IAbilityTargetRestriction failedRestriction = null;
			IAbilityTargetRestriction[] targetRestrictions = Ability.Blueprint.TargetRestrictions;
			foreach (IAbilityTargetRestriction abilityTargetRestriction in targetRestrictions)
			{
				if (!abilityTargetRestriction.IsTargetRestrictionPassed(Ability, target, Ability.Caster.Position))
				{
					failedRestriction = abilityTargetRestriction;
					string restrictionText = abilityTargetRestriction.GetAbilityTargetRestrictionUIText(Ability, target, desiredPosition);
					if (restrictionText.IsNullOrEmpty())
					{
						restrictionText = BlueprintRoot.Instance.LocalizedTexts.Reasons.UnavailableGeneric;
					}
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(restrictionText, addToLog: false, WarningNotificationFormat.Attention);
					});
					break;
				}
			}
			EventBus.RaiseEvent(delegate(IClickActionHandler h)
			{
				h.OnAbilityCastRefused(Ability, target, failedRestriction);
			});
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
			return false;
		}
		AbilityData abilityData = MultiTargetHandler.AddTarget(target);
		if (abilityData != null)
		{
			SetCurrentTargetAbility(abilityData);
			UISounds.Instance.Sounds.Combat.CombatGridConfirmActionClick.Play();
			return true;
		}
		bool shouldApproach = unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar;
		UnitCommandsRunner.TryUnitUseAbility(RootAbility, MultiTargetHandler.Targets[0], shouldApproach);
		UISounds.Instance.Sounds.Combat.CombatGridConfirmActionClick.Play();
		Game.Instance.GameCommandQueue.ClearPointerMode();
		return true;
	}

	private bool ShouldHandleAbilityCastFail(TargetWrapper target, out AbilityData.UnavailabilityReasonType? unavailabilityReason)
	{
		unavailabilityReason = null;
		if (target == null)
		{
			if (!Ability.Blueprint.CanTargetPoint)
			{
				unavailabilityReason = AbilityData.UnavailabilityReasonType.NullTarget;
				return true;
			}
			return true;
		}
		bool flag = Ability.CanTargetFromDesiredPosition(target, out unavailabilityReason);
		if (unavailabilityReason == AbilityData.UnavailabilityReasonType.TargetTooFar && !Game.Instance.Player.IsInCombat)
		{
			return false;
		}
		return !flag;
	}

	public void SetAbility([NotNull] AbilityData ability)
	{
		if (RootAbility != null)
		{
			if (ability == RootAbility)
			{
				Game.Instance.ClickEventsController.ClearPointerMode();
				return;
			}
			DropAbility();
		}
		Game.Instance.ClickEventsController.SetPointerMode(PointerMode.Ability);
		RootAbility = ability;
		MultiTargetHandler.OnRootAbilitySelected(ability);
		SetCurrentTargetAbility(MultiTargetHandler.GetAbilityForNextTarget());
	}

	private void SetCurrentTargetAbility(AbilityData abilityData)
	{
		Ability = abilityData;
		EventBus.RaiseEvent(delegate(IAbilityTargetSelectionUIHandler h)
		{
			h.HandleAbilityTargetSelectionStart(Ability);
		});
	}

	public void DropAbility()
	{
		if (!(RootAbility == null))
		{
			AbilityData rootAbility = (Ability = null);
			RootAbility = rootAbility;
			MultiTargetHandler.OnRootAbilitySelected(null);
			EventBus.RaiseEvent(delegate(IAbilityTargetSelectionUIHandler h)
			{
				h.HandleAbilityTargetSelectionEnd(Ability);
			});
		}
	}
}
