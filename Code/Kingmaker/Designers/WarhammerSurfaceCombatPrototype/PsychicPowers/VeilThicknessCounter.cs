using Core.Cheats;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype.PsychicPowers;

public class VeilThicknessCounter : IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnBasedModeHandler, IRoundStartHandler
{
	private int m_Value => Game.Instance.LoadedAreaState.AreaVailPart?.Vail ?? 0;

	public int Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			int delta = value - m_Value;
			Game.Instance.LoadedAreaState.AreaVailPart.Vail = value;
			if (Game.Instance.CurrentMode != GameModeType.SpaceCombat)
			{
				EventBus.RaiseEvent(delegate(IPsychicPhenomenaUIHandler h)
				{
					h.HandleVeilThicknessValueChanged(delta, m_Value);
				});
			}
		}
	}

	public void OnEnable()
	{
		EventBus.RaiseEvent(delegate(IPsychicPhenomenaUIHandler h)
		{
			h.HandleVeilThicknessValueChanged(0, Game.Instance.TurnController.VeilThicknessCounter.Value);
		});
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor.IsInCombat && command is UnitUseAbility unitUseAbility && unitUseAbility.Ability.Blueprint.AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower && !unitUseAbility.IsInterruptible)
		{
			Value = Rulebook.Trigger(new RuleCalculateVeilCount(command.Executor, unitUseAbility.Ability)).Result;
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			ResetValueOutOfCombat();
		}
	}

	public void ResetValueOutOfCombat()
	{
		if (!Game.Instance.TurnController.TbActive)
		{
			Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, isTurnBaseSwitched: true, isNewRoundStart: false)).Result;
		}
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased)
		{
			Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, isTurnBaseSwitched: false, isNewRoundStart: true)).Result;
		}
	}

	[Cheat(Name = "veil_remove", Description = "Decreases veil thickness by specified value (default = 1)")]
	public static void DecreaseVeil(int value = 1)
	{
		Game.Instance.TurnController.VeilThicknessCounter.Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, -value)).Result;
	}

	[Cheat(Name = "veil_add", Description = "Increases veil thickness by specified value (default = 1)")]
	public static void IncreaseVeil(int value = 1)
	{
		Game.Instance.TurnController.VeilThicknessCounter.Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, value)).Result;
	}

	[Cheat(Name = "veil_clear", Description = "Clears all veil thickness")]
	public static void ClearVeil()
	{
		Game.Instance.TurnController.VeilThicknessCounter.Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, -Game.Instance.TurnController.VeilThicknessCounter.Value)).Result;
	}
}
