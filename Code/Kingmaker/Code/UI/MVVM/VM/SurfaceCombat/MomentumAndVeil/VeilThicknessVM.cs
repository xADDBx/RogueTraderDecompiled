using System;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;

public class VeilThicknessVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IPsychicPhenomenaUIHandler, ISubscriber, IAbilityTargetSelectionUIHandler, IAbilityTargetHoverUIHandler, IAbilityTargetMarkerHoverUIHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IGameModeHandler
{
	public IntReactiveProperty Value = new IntReactiveProperty(0);

	public IntReactiveProperty PredictedValue = new IntReactiveProperty(0);

	public readonly ReactiveProperty<bool> IsTurnBasedActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAppropriateGameMode = new ReactiveProperty<bool>();

	public TooltipTemplateVail Tooltip = new TooltipTemplateVail();

	public VeilThicknessVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(Value.Subscribe(Tooltip.ChangeValue));
		OnGameModeStart(Game.Instance.CurrentMode);
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleVeilThicknessValueChanged(int delta, int value)
	{
		IntReactiveProperty predictedValue = PredictedValue;
		int value2 = (Value.Value = value);
		predictedValue.Value = value2;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		UpdateValues(ability);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		UpdateValues(null);
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		UpdateValues((!hover) ? null : ability);
	}

	public void HandleAbilityTargetMarkerHover(AbilityData ability, bool hover)
	{
		UpdateValues((!hover) ? null : ability);
	}

	private void UpdateValues([CanBeNull] AbilityData ability)
	{
		if (ability == null)
		{
			PredictedValue.Value = Value.Value;
		}
		else
		{
			PredictedValue.Value = Value.Value + ability.GetVeilThicknessPointsToAdd(isPrediction: true);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		IsTurnBasedActive.Value = isTurnBased;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
	}

	public void HandleTurnBasedModeResumed()
	{
		IsTurnBasedActive.Value = true;
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsAppropriateGameMode.Value = gameMode != GameModeType.Dialog && gameMode != GameModeType.Cutscene;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void Update()
	{
		OnGameModeStart(Game.Instance.CurrentMode);
		IsTurnBasedActive.Value = TurnController.IsInTurnBasedCombat();
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
	}
}
