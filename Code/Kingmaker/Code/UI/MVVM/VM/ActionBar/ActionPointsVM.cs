using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.UI.MVVM;
using Pathfinding;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar;

public class ActionPointsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAbilityTargetSelectionUIHandler, ISubscriber, IAbilityTargetHoverUIHandler, IAbilityTargetMarkerHoverUIHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IUnitCommandStartHandler, IUnitCommandActHandler, IUnitRunCommandHandler, IUnitPathManagerHandler, ITurnStartHandler, IContinueTurnHandler, IInterruptTurnStartHandler, IInterruptTurnContinueHandler, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, ITurnBasedModeHandler, IUnitSpentActionPoints, IUnitGainActionPoints, IUnitSpentMovementPoints, IUnitGainMovementPoints
{
	public readonly ReactiveProperty<float> MaxBlueAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> MaxYellowAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> BlueAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> YellowAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> SpentBlueAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> SpentYellowAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> CostBlueAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> CostYellowAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> PredictedBlueAP = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> PredictedYellowAP = new ReactiveProperty<float>();

	private readonly MechanicEntityUIWrapper m_UnitUIWrapper;

	private AbilityData m_SelectedAbility;

	private AbilityData m_HoveredAbility;

	private InteractionPart m_Interaction;

	private readonly BoolReactiveProperty m_AbilitySelected = new BoolReactiveProperty();

	private readonly string m_APColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.ActionPoints) + ">";

	private readonly string m_MPColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.MovePoints) + ">";

	private readonly string m_NotEnoughColor = "<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.TooltipColors.NotEnoughPoints) + ">";

	private string m_ColorEnd = "</color>";

	public ActionPointsVM(MechanicEntity entity)
	{
		m_UnitUIWrapper = new MechanicEntityUIWrapper(entity);
		UpdateActionPointsFromUnit();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(CostYellowAP.CombineLatest(CostBlueAP, m_AbilitySelected, (float ap, float mp, bool _) => new { ap, mp }).Subscribe(value =>
		{
			if (!(Game.Instance.CurrentMode == GameModeType.SpaceCombat))
			{
				float mp2 = value.mp;
				int ap2 = Mathf.RoundToInt(value.ap);
				bool noMove = false;
				bool setForce = false;
				AbilityData abilityData = m_SelectedAbility ?? m_HoveredAbility;
				if (abilityData != null)
				{
					mp2 = UnitPathManager.Instance.MemorizedPathCost;
					noMove = abilityData.ClearMPAfterUse;
					setForce = abilityData.TargetAnchor == AbilityTargetAnchor.Owner || m_SelectedAbility != null;
				}
				SetCursorTexts(mp2, ap2, noMove, setForce);
			}
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetCursorTexts(float mp, int ap, bool noMove, bool setForce)
	{
		string upperText = null;
		string lowerText = null;
		bool flag = Game.Instance.SelectionCharacter.SelectedUnit.Value.IsMyNetRole();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		if (mp > 0f && flag)
		{
			string text = ((BlueAP.Value >= mp) ? m_MPColor : m_NotEnoughColor);
			upperText = $"{text}<size=150%>{mp}</size>{m_ColorEnd} {tooltips.MP.Text}";
		}
		if (ap > 0 && flag)
		{
			lowerText = $"{m_APColor}<size=150%>{ap}</size>{m_ColorEnd} {tooltips.AP.Text}";
		}
		Game.Instance.CursorController.SetTexts_APMP(upperText, lowerText, setForce);
		Game.Instance.CursorController.SetNoMoveIcon(noMove, setForce);
	}

	private void UpdateActionPointsFromUnit()
	{
		if (m_UnitUIWrapper.MechanicEntity != null && m_UnitUIWrapper.CombatState != null)
		{
			MaxBlueAP.Value = ((m_UnitUIWrapper.CombatState.ActionPointsBlue <= (float)m_UnitUIWrapper.CombatState.WarhammerInitialAPBlue.ModifiedValue) ? ((float)m_UnitUIWrapper.CombatState.WarhammerInitialAPBlue.ModifiedValue) : m_UnitUIWrapper.CombatState.ActionPointsBlue);
			BlueAP.Value = GetBlueAP();
			PredictedBlueAP.Value = BlueAP.Value;
			SpentBlueAP.Value = m_UnitUIWrapper.CombatState.ActionPointsBlueSpentThisTurn;
			MaxYellowAP.Value = ((m_UnitUIWrapper.CombatState.ActionPointsYellow <= m_UnitUIWrapper.CombatState.WarhammerInitialAPYellow.ModifiedValue) ? m_UnitUIWrapper.CombatState.WarhammerInitialAPYellow.ModifiedValue : m_UnitUIWrapper.CombatState.ActionPointsYellow);
			YellowAP.Value = m_UnitUIWrapper.CombatState.ActionPointsYellow;
			PredictedYellowAP.Value = YellowAP.Value;
			SpentYellowAP.Value = (MaxYellowAP.Value - (float)m_UnitUIWrapper.CombatState.ActionPointsYellow) / MaxYellowAP.Value * 6f;
		}
	}

	private void UpdateActionPointsFromAction()
	{
		if (m_UnitUIWrapper.MechanicEntity != null && m_UnitUIWrapper.CombatState != null)
		{
			BlueAP.Value = GetBlueAP();
			SpentBlueAP.Value = m_UnitUIWrapper.CombatState.ActionPointsBlueSpentThisTurn;
			if (PredictedBlueAP.Value > BlueAP.Value)
			{
				PredictedBlueAP.Value = BlueAP.Value;
			}
			YellowAP.Value = m_UnitUIWrapper.CombatState.ActionPointsYellow;
			if (PredictedYellowAP.Value > YellowAP.Value)
			{
				PredictedYellowAP.Value = YellowAP.Value;
			}
			SpentYellowAP.Value = (MaxYellowAP.Value - (float)m_UnitUIWrapper.CombatState.ActionPointsYellow) / MaxYellowAP.Value * 6f;
		}
	}

	private float GetBlueAP()
	{
		if (m_UnitUIWrapper.CombatState == null)
		{
			return 0f;
		}
		if (!m_UnitUIWrapper.CantMove)
		{
			return m_UnitUIWrapper.CombatState.ActionPointsBlue;
		}
		return 0f;
	}

	private void SetBlueAPCost(float cost)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && !(m_SelectedAbility != null) && !(m_HoveredAbility != null))
		{
			if (m_Interaction != null)
			{
				ClearBlueAPCost();
				return;
			}
			CostBlueAP.Value = cost;
			PredictedBlueAP.Value = Mathf.Max(BlueAP.Value - CostBlueAP.Value, 0f);
		}
	}

	private void CalculateYellowAPCost()
	{
		CalculateYellowAPCost(m_SelectedAbility ?? m_HoveredAbility);
		CalculateYellowAPCost(m_Interaction);
		if (m_SelectedAbility == null && m_HoveredAbility == null && m_Interaction == null)
		{
			ClearYellowAPCost();
		}
	}

	private void CalculateYellowAPCost(AbilityData ability)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && !(ability == null))
		{
			CostBlueAP.Value = (ability.ClearMPAfterUse ? BlueAP.Value : 0f);
			PredictedBlueAP.Value = BlueAP.Value - CostBlueAP.Value;
			CostYellowAP.Value = ability.CalculateActionPointCost();
			PredictedYellowAP.Value = YellowAP.Value - CostYellowAP.Value;
		}
	}

	private void CalculateYellowAPCost(InteractionPart interaction)
	{
		if (m_UnitUIWrapper.MechanicEntity != null && interaction != null)
		{
			CostYellowAP.Value = interaction.ActionPointsCost;
			PredictedYellowAP.Value = YellowAP.Value - CostYellowAP.Value;
		}
	}

	private void ClearYellowAPCost()
	{
		CostYellowAP.Value = 0f;
		PredictedYellowAP.SetValueAndForceNotify(YellowAP.Value);
	}

	private void ClearBlueAPCost()
	{
		CostBlueAP.Value = 0f;
		PredictedBlueAP.SetValueAndForceNotify(BlueAP.Value);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_SelectedAbility = ability;
		m_AbilitySelected.Value = true;
		CalculateYellowAPCost();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_SelectedAbility = null;
		m_AbilitySelected.Value = false;
		ClearYellowAPCost();
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	public void HandleAbilityTargetMarkerHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	private void HandleAbilityTargetHoverInternal(AbilityData ability, bool hover)
	{
		m_HoveredAbility = (hover ? ability : null);
		CalculateYellowAPCost();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
			ClearBlueAPCost();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (ShouldHandle(command))
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandlePathAdded(Path path, float cost)
	{
		HandleCurrentNodeChanged(cost);
	}

	public void HandlePathRemoved()
	{
	}

	public void HandleCurrentNodeChanged(float cost)
	{
		if (Game.Instance.TurnController.IsPlayerTurn)
		{
			SetBlueAPCost(cost);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		HandleUnitStartTurnInternal();
	}

	private void HandleUnitStartTurnInternal()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			UpdateActionPointsFromUnit();
		}
	}

	private bool ShouldHandle(AbstractUnitCommand command)
	{
		return command.Executor == m_UnitUIWrapper.MechanicEntity;
	}

	private bool ShouldHandle()
	{
		return EventInvokerExtensions.MechanicEntity == m_UnitUIWrapper.MechanicEntity;
	}

	public void HandleObjectHighlightChange()
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		if (entity == null || !entity.View.Highlighted)
		{
			m_Interaction = null;
		}
		else
		{
			m_Interaction = entity.Interactions.FirstOrDefault();
		}
		CalculateYellowAPCost();
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		ClearBlueAPCost();
		ClearYellowAPCost();
	}

	public void HandleUnitSpentActionPoints(int actionPointsSpent)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitGainActionPoints(int actionPoints, MechanicsContext context)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}

	public void HandleUnitGainMovementPoints(float movementPoints, MechanicsContext context)
	{
		if (ShouldHandle())
		{
			UpdateActionPointsFromAction();
		}
	}
}
