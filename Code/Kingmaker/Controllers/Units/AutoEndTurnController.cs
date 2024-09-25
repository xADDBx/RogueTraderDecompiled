using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Units;

public class AutoEndTurnController : IControllerTick, IController
{
	private const int DelayInTicks = 9;

	private int m_LockedUntilTick = int.MinValue;

	private static int CurrentTick => Game.Instance.RealTimeController.CurrentNetworkTick;

	private bool IsLocked => CurrentTick <= m_LockedUntilTick;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController == null || !turnController.TurnBasedModeActive)
		{
			return;
		}
		MechanicEntity currentUnit = turnController.CurrentUnit;
		if (currentUnit == null || !currentUnit.IsMyNetRole())
		{
			return;
		}
		if (currentUnit.IsPlayerFaction)
		{
			PartUnitState stateOptional = currentUnit.GetStateOptional();
			if (stateOptional != null && stateOptional.IsHelpless)
			{
				TryEndTurn();
				return;
			}
		}
		if (currentUnit.IsPlayerFaction && !turnController.IsSpaceCombat && (!turnController.IsPlayerTurn || currentUnit.GetCommandsOptional()?.Current == null) && (bool)SettingsRoot.Game.TurnBased.AutoEndTurn)
		{
			PartUnitCombatState combatStateOptional = currentUnit.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.ActionPointsBlue == 0f && combatStateOptional.ActionPointsYellow == 0 && !currentUnit.HasAnyAvailableBonusAbility())
			{
				TryEndTurn();
			}
		}
	}

	private void TryEndTurn()
	{
		if (!IsLocked && !Game.Instance.TurnController.EndTurnRequested)
		{
			Game.Instance.GameCommandQueue.EndTurnManually(Game.Instance.TurnController.CurrentUnit);
			m_LockedUntilTick = CurrentTick + 9;
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.TurnBasedTexts.AutoEndTurn.Text);
			});
		}
	}
}
