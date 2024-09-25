using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventAbility : GameLogEventBrackets<GameLogEventAbility>
{
	[UsedImplicitly]
	private class EventHandle : GameLogController.GameEventsHandler, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleExecutionProcessStart(AbilityExecutionContext context)
		{
			AddEvent(new GameLogEventAbility(context));
		}

		public void HandleExecutionProcessEnd(AbilityExecutionContext context)
		{
			GetEventFromQueue((GameLogEventAbility i) => i.Context == context)?.MarkReady();
		}
	}

	public readonly AbilityExecutionContext Context;

	public readonly List<GameLogRuleEvent<RuleRollScatterShotHitDirection>> ScatterShots = new List<GameLogRuleEvent<RuleRollScatterShotHitDirection>>();

	public readonly List<GameLogEventAttack> ScatterAttacks = new List<GameLogEventAttack>();

	public AbilityData Ability => Context.Ability;

	public bool IsScatter => Ability.IsScatter;

	public bool IsAoe => Ability.IsAOE;

	public GameLogEventAbility(AbilityExecutionContext context)
	{
		Context = context;
	}

	protected override bool TryHandleInnerEventInternal(GameLogEvent @event)
	{
		IRulebookEvent first = Rulebook.CurrentContext.First;
		if (first == null)
		{
			return false;
		}
		if (((RulebookEvent)first).Reason.Context != Context)
		{
			return false;
		}
		GameLogRuleEvent<RuleRollScatterShotHitDirection> gameLogRuleEvent = @event.AsRuleEvent<RuleRollScatterShotHitDirection>();
		if (gameLogRuleEvent != null)
		{
			ScatterShots.Add(gameLogRuleEvent);
		}
		if (@event is GameLogEventAttack gameLogEventAttack)
		{
			for (int num = ScatterAttacks.Count - 1; num >= 0; num--)
			{
				GameLogEventAttack gameLogEventAttack2 = ScatterAttacks[num];
				if (gameLogEventAttack2.RollPerformAttackRule.ResultIsHit && !gameLogEventAttack.RollPerformAttackRule.IsMelee && gameLogEventAttack.RollPerformAttackRule.BurstIndex == gameLogEventAttack2.RollPerformAttackRule.BurstIndex)
				{
					if (gameLogEventAttack.RollPerformAttackRule.Ability.Blueprint.AbilityTag != AbilityTag.ThrowingGrenade)
					{
						gameLogEventAttack2.SetOverpenetrationTrigger(value: true);
					}
					break;
				}
			}
			ScatterAttacks.Add(gameLogEventAttack);
		}
		return true;
	}
}
