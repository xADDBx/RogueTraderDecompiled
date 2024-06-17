using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;

public class SurfaceMomentumVM : SurfaceActionBarBasePartVM, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IGameModeHandler, IUnitCommandEndHandler, IPreparationTurnEndHandler
{
	public readonly AutoDisposingList<ActionBarSlotVM> HeroicActSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> DesperateMeasureSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly ReactiveProperty<MomentumEntityVM> MomentumEntityVM = new ReactiveProperty<MomentumEntityVM>();

	public readonly ReactiveProperty<bool> IsTurnBasedActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAppropriateGameMode = new ReactiveProperty<bool>();

	public readonly ReactiveCommand AbilitiesListUpdated = new ReactiveCommand();

	public SurfaceMomentumVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
		if (TurnController.IsInTurnBasedCombat())
		{
			DelayedInvoker.InvokeInFrames(HandleTurnBasedModeResumed, 1);
		}
	}

	protected override void OnUnitChanged()
	{
		MomentumEntityVM.Value = MomentumContextVM.Instance?.TryGetMomentumEntity(Unit);
		ClearSlots();
		if (Unit.Entity == null)
		{
			return;
		}
		int num = 0;
		foreach (MechanicActionBarSlot momentumSlot in Unit.Entity.UISettings.GetMomentumSlots())
		{
			ActionBarSlotVM item = new ActionBarSlotVM(momentumSlot, num);
			AbilityData abilityData = momentumSlot.GetContentData() as AbilityData;
			if (!(abilityData == null))
			{
				if (abilityData.Blueprint.IsHeroicAct)
				{
					HeroicActSlots.Add(item);
				}
				else
				{
					DesperateMeasureSlots.Add(item);
				}
				num++;
			}
		}
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
		AbilitiesListUpdated.Execute();
	}

	protected override void ClearSlots()
	{
		HeroicActSlots.Clear();
		DesperateMeasureSlots.Clear();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsAppropriateGameMode.Value = gameMode != GameModeType.Dialog && gameMode != GameModeType.Cutscene;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnBasedModeHandle(isTurnBased: true);
		UnitChanged.Execute();
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		IsTurnBasedActive.Value = isTurnBased;
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
		if (isTurnBased)
		{
			MomentumContextVM.Instance?.ForceUpdateContext();
			OnUnitChanged();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		DelayedInvoker.InvokeInFrames(UpdateSlots, 1);
	}

	private void UpdateSlots()
	{
		foreach (ActionBarSlotVM heroicActSlot in HeroicActSlots)
		{
			heroicActSlot.UpdateResources();
		}
		foreach (ActionBarSlotVM desperateMeasureSlot in DesperateMeasureSlots)
		{
			desperateMeasureSlot.UpdateResources();
		}
		MomentumEntityVM?.Value?.UpdateMomentum(null);
	}

	public void HandleEndPreparationTurn()
	{
		DelayedInvoker.InvokeInFrames(OnUnitChanged, 1);
	}
}
