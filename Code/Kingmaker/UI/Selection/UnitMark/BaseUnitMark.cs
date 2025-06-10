using System;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public abstract class BaseUnitMark : RegisteredBehaviour, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitSizeHandler<EntitySubscriber>, IUnitSizeHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<IUnitSizeHandler, EntitySubscriber>, IDialogCueHandler, IUnitHighlightUIHandler, IInteractionHighlightUIHandler, IGameModeHandler, ITurnStartHandler, IContinueTurnHandler, IInterruptTurnStartHandler, IInterruptTurnContinueHandler, ITurnBasedModeResumeHandler, IUIVisibilityHandler, ILateUpdatable, IDialogFinishHandler, IAbilityTargetSelectionUIHandler, IAbilityExecutionProcessHandler
{
	private static readonly int _Color = Shader.PropertyToID("_BaseColor");

	public AbstractUnitEntity Unit { get; private set; }

	public UnitMarkState State { get; protected set; }

	protected static bool IsCutscene => Game.Instance.CurrentMode == GameModeType.Cutscene;

	protected static bool IsHideAllUI => UIVisibilityState.VisibilityPreset.Value == UIVisibilityFlags.None;

	public IEntity GetSubscribingEntity()
	{
		return Unit;
	}

	public virtual void Initialize(AbstractUnitEntity unit)
	{
		Unit = unit;
		if (Math.Abs(unit.Corpulence) < Mathf.Epsilon)
		{
			PFLog.UI.Log("Non initialized unit: " + unit, unit.View);
		}
		HandleStateChanged();
		UpdateUnitCurrentTurnState();
		HandleUnitSizeChanged();
		if (Unit != null)
		{
			if (base.isActiveAndEnabled)
			{
				EventBus.Subscribe(this);
			}
			UpdateCombatState();
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if (Unit != null)
		{
			EventBus.Subscribe(this);
			HandleUnitSizeChanged();
		}
		HandleStateChanged();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		EventBus.Unsubscribe(this);
	}

	public void HandleOnCueShow(CueShowData cueShowData)
	{
		BaseUnitEntity currentSpeaker = Game.Instance.DialogController.CurrentSpeaker;
		if (currentSpeaker != null)
		{
			bool active = Unit == currentSpeaker && Game.Instance.CurrentMode == GameModeType.Dialog;
			SetState(UnitMarkState.DialogCurrentSpeaker, active);
		}
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		SetState(UnitMarkState.DialogCurrentSpeaker, active: false);
	}

	public void HandleUnitJoinCombat()
	{
		UpdateCombatState();
	}

	private void UpdateCombatState()
	{
		SetState(UnitMarkState.IsInCombat, Unit.IsInCombat && !Unit.HasMechanicFeature(MechanicsFeatureType.Hidden));
		if (Game.Instance.TurnController.IsPreparationTurn && Unit is BaseUnitEntity unit)
		{
			SetState(UnitMarkState.CurrentTurn, Game.Instance.SelectionCharacter.IsSelected(unit) && !Unit.HasMechanicFeature(MechanicsFeatureType.Hidden));
		}
	}

	public void HandleUnitLeaveCombat()
	{
		UpdateCombatState();
	}

	public void HandleUnitSizeChanged()
	{
		HandleUnitSizeChangedImpl();
	}

	protected virtual void HandleUnitSizeChangedImpl()
	{
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Unit.Size);
		base.transform.localScale = new Vector3(rectForSize.Width, 1f, rectForSize.Height);
		base.transform.rotation = Quaternion.identity;
	}

	public virtual void Selected(bool isSelected)
	{
		SetState(UnitMarkState.Selected, isSelected);
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.TurnController.IsPreparationTurn)
		{
			SetState(UnitMarkState.CurrentTurn, isSelected);
		}
	}

	protected void SetState(UnitMarkState state, bool active)
	{
		UnitMarkState state2 = State;
		state2 = ((!active) ? (state2 & ~state) : (state2 | state));
		if (State != state2)
		{
			State = state2;
			HandleStateChanged();
		}
	}

	public abstract void HandleStateChanged();

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateUnitCurrentTurnState();
	}

	void IContinueTurnHandler.HandleUnitContinueTurn(bool isTurnBased)
	{
		UpdateUnitCurrentTurnState();
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateUnitCurrentTurnState();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		UpdateUnitCurrentTurnState();
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		UpdateUnitCurrentTurnState();
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		UpdateUnitCurrentTurnState();
	}

	private void UpdateUnitCurrentTurnState()
	{
		bool flag = TurnController.IsInTurnBasedCombat() && Unit is BaseUnitEntity baseUnitEntity && baseUnitEntity.Master == Game.Instance.TurnController.CurrentUnit;
		bool active = TurnController.IsInTurnBasedCombat() && (Unit == Game.Instance.TurnController.CurrentUnit || (flag && !Unit.HasMechanicFeature(MechanicsFeatureType.Hidden)));
		SetState(UnitMarkState.CurrentTurn, active);
		SetState(UnitMarkState.Selected, active);
	}

	void IUnitHighlightUIHandler.HandleHighlightChange(AbstractUnitEntityView unit)
	{
		SetState(UnitMarkState.MouseHovered, unit == Unit.View && unit.MouseHighlighted);
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		SetState(UnitMarkState.Highlighted, isOn);
	}

	void IGameModeHandler.OnGameModeStart(GameModeType gameMode)
	{
		HandleStateChanged();
	}

	void IGameModeHandler.OnGameModeStop(GameModeType gameMode)
	{
	}

	void IUIVisibilityHandler.HandleUIVisibilityChange(UIVisibilityFlags flags)
	{
		HandleStateChanged();
	}

	public abstract void HandleAbilityTargetSelectionStart(AbilityData ability);

	public abstract void HandleAbilityTargetSelectionEnd(AbilityData ability);

	public virtual void SetGamepadSelected(bool selected)
	{
	}

	public void DoLateUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}
}
