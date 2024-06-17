using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Common.UnitState;

public class UnitState : BaseDisposable, IUnitDirectHoverUIHandler, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, ICellAbilityHandler, IAbilityTargetSelectionUIHandler, IAbilityTargetHoverUIHandler, IPartyCombatHandler, IInteractionHighlightUIHandler, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IUnitFeaturesHandler<EntitySubscriber>, IUnitFeaturesHandler, IEventTag<IUnitFeaturesHandler, EntitySubscriber>, IEntitySubscriber, IGameModeHandler, IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, IUnitChangeAttackFactionsHandler, IUnitCommandStartHandler<EntitySubscriber>, IUnitCommandStartHandler, IEventTag<IUnitCommandStartHandler, EntitySubscriber>, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, IEventTag<IUnitCommandActHandler, EntitySubscriber>, INetRoleSetHandler, INetStopPlayingHandler, INetPingEntity, ILootDroppedAsAttachedHandler<EntitySubscriber>, ILootDroppedAsAttachedHandler, IEventTag<ILootDroppedAsAttachedHandler, EntitySubscriber>, IDestructibleEntityHandler
{
	public readonly MechanicEntityUIWrapper Unit;

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> IsEnemy = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDeadOrUnconsciousIsDead = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDead = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCover = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDestructible = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDestructibleNotCover = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTBM = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsInCombat = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsVisibleForPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCurrentUnitTurn = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<bool> IsMouseOverUnit = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsPingUnit = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ForceHotKeyPressed = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> NeedConsoleHint = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsAoETarget = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsStarshipAttack = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasHiddenCondition = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<AbilityData> Ability = new ReactiveProperty<AbilityData>(null);

	public readonly ReactiveProperty<bool> IsCaster = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsActing = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HoverSelfTargetAbility = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasLoot = new ReactiveProperty<bool>(initialValue: false);

	public Sprite HoverAbilityIcon;

	private Tween m_PingTween;

	public UnitState([NotNull] MechanicEntity unit)
	{
		Unit = new MechanicEntityUIWrapper(unit);
		IsDeadOrUnconsciousIsDead.Value = Unit.IsDead;
		UpdateProperties();
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			InternalUpdate();
		}));
		AddDisposable(EventBus.Subscribe(this));
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (!base.IsDisposed)
			{
				UpdateHiddenConditions();
				UpdateTBMUnit();
			}
		}, 1);
	}

	protected override void DisposeImplementation()
	{
	}

	private void InternalUpdate()
	{
		if (Unit.MechanicEntity != null)
		{
			UpdateIsVisibleForPlayer();
			UpdateIsSelected();
		}
	}

	public void UpdateProperties()
	{
		IsPlayer.Value = Unit.IsPlayer;
		IsEnemy.Value = Unit.IsPlayerEnemy;
		Name.Value = Unit.GetUnitNameWithPlayer();
		IsCover.Value = Unit.IsCover;
		IsDestructible.Value = Unit.IsDestructible;
		IsDestructibleNotCover.Value = Unit.IsDestructibleNotCover;
		IsDead.Value = Unit.IsDead;
		HasLoot.Value = Unit.IsDeadAndHasAttachedDroppedLoot || Unit.IsDeadAndHasLoot;
		UpdateGamepadHint();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		if (Unit.MechanicEntity is AbstractUnitEntity abstractUnitEntity && unitEntityView.Data == abstractUnitEntity)
		{
			IsMouseOverUnit.Value = isHover;
			UpdateGamepadHint();
		}
	}

	private void UpdateGamepadHint()
	{
		ReactiveProperty<bool> needConsoleHint = NeedConsoleHint;
		int value;
		if (IsMouseOverUnit.Value)
		{
			MechanicEntityUIWrapper unit = Unit;
			value = (((!unit.IsDirectlyControllable && !unit.IsPlayerEnemy && (bool)Unit.MechanicEntity?.GetOptional<UnitPartInteractions>()) || Unit.IsDeadAndHasAttachedDroppedLoot || Unit.IsDeadAndHasLoot) ? 1 : 0);
		}
		else
		{
			value = 0;
		}
		needConsoleHint.Value = (byte)value != 0;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateTBMUnit();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateTBMUnit();
	}

	public void HandleUnitStartInterruptTurn()
	{
		UpdateTBMUnit();
	}

	private void UpdateTBMUnit()
	{
		IsTBM.Value = (Game.Instance?.TurnController?.TurnBasedModeActive).GetValueOrDefault();
		IsInCombat.Value = ((!IsDestructible.Value) ? Unit.IsInCombat : IsTBM.Value);
		IsCurrentUnitTurn.Value = IsTBM.Value && Unit.MechanicEntity == Game.Instance.TurnController.CurrentUnit;
	}

	private void UpdateIsVisibleForPlayer()
	{
		IsVisibleForPlayer.Value = Unit.IsVisibleForPlayer && Unit.IsInCameraFrustum;
	}

	private void UpdateIsSelected()
	{
		IsSelected.Value = Game.Instance.SelectionCharacter.IsSelected(Unit.MechanicEntity as BaseUnitEntity);
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.TurnController.IsPreparationTurn)
		{
			IsCurrentUnitTurn.Value = IsSelected.Value;
		}
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		if (abilityTargets.Count == 0 || IsStarshipAttack.Value)
		{
			IsAoETarget.Value = false;
			return;
		}
		IsAoETarget.Value = abilityTargets.Any((AbilityTargetUIData n) => n.Target == Unit.MechanicEntity);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		IsAoETarget.Value = IsAoETarget.Value && ((ability.IsAOE && !ability.IsStarshipAttack) || ability.IsScatter || ability.IsCharge || ability.IsBurstAttack || ability.IsSingleShot || ability.IsChainLighting());
		Ability.Value = ability;
		IsStarshipAttack.Value = ability.IsStarshipAttack;
		IsCaster.Value = ability.Caster == Unit.MechanicEntity;
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		IsAoETarget.Value = false;
		IsStarshipAttack.Value = false;
		Ability.Value = null;
		IsCaster.Value = false;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateTBMUnit();
	}

	public void HandleHighlightChange(bool isOn)
	{
		ForceHotKeyPressed.Value = isOn;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!base.IsDisposed)
		{
			UpdateHiddenConditions();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!base.IsDisposed)
		{
			UpdateHiddenConditions();
		}
	}

	public void HandleFeatureAdded(FeatureCountableFlag feature)
	{
		HandleFeatureInternal(feature);
	}

	public void HandleFeatureRemoved(FeatureCountableFlag feature)
	{
		HandleFeatureInternal(feature);
	}

	private void HandleFeatureInternal(FeatureCountableFlag feature)
	{
		if (feature.Type == MechanicsFeatureType.IsUntargetable)
		{
			UpdateHiddenConditions();
		}
	}

	private void UpdateHiddenConditions()
	{
		bool flag = Game.Instance.CurrentMode == GameModeType.Cutscene;
		bool flag2 = Unit.Features != null && (bool)Unit.Features.IsUntargetable;
		HasHiddenCondition.Value = flag || flag2;
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		IsDeadOrUnconsciousIsDead.Value = Unit.IsDeadOrUnconscious;
	}

	public void HandleFactionChanged()
	{
		HandleFactionChangedInternal();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		HandleFactionChangedInternal();
	}

	private void HandleFactionChangedInternal()
	{
		if (EventInvokerExtensions.MechanicEntity == Unit.MechanicEntity)
		{
			DelayedInvoker.InvokeInFrames(UpdateProperties, 1);
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return Unit.MechanicEntity;
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateTBMUnit();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		IsActing.Value = true;
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		IsActing.Value = false;
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		IsActing.Value = false;
	}

	public void HandleRoleSet(string entityId)
	{
		MechanicEntityUIWrapper unit = Unit;
		if (unit.IsPlayer && unit.MechanicEntity != null && Unit.MechanicEntity.UniqueId == entityId)
		{
			Name.Value = Unit.GetUnitNameWithPlayer();
		}
	}

	public void HandleObjectHighlightChange()
	{
		if (EventInvokerExtensions.MapObjectEntity != null && EventInvokerExtensions.MapObjectEntity == Unit.MechanicEntity)
		{
			IsMouseOverUnit.Value = EventInvokerExtensions.MapObjectEntity.View.Highlighted;
		}
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		if (ability.Caster == Unit.MechanicEntity)
		{
			bool flag = Ability.Value == null && hover && ability.IsAvailable && ability.TargetAnchor == AbilityTargetAnchor.Owner && Game.Instance.SelectionCharacter.IsSelected(Unit.MechanicEntity as BaseUnitEntity);
			HoverAbilityIcon = (flag ? ability.Icon : null);
			HoverSelfTargetAbility.Value = flag;
		}
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		Name.Value = Unit.GetUnitNameWithPlayer();
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity == Unit.MechanicEntity)
		{
			IsPingUnit.Value = true;
			m_PingTween?.Kill();
			m_PingTween = DOTween.To(() => 1f, delegate
			{
			}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				IsPingUnit.Value = false;
				m_PingTween = null;
			});
		}
	}

	public void HandleLootDroppedAsAttached()
	{
		HasLoot.Value = true;
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		if (EventInvokerExtensions.MapObjectEntity != null && EventInvokerExtensions.MapObjectEntity == Unit.MechanicEntity)
		{
			IsDeadOrUnconsciousIsDead.Value = Unit.IsDeadOrUnconscious;
		}
	}
}
