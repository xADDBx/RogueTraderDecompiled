using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class PsychicPhenomenaAvoidedLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPsychicPhenomenaAvoided>
{
	public void HandleEvent(GameLogEventPsychicPhenomenaAvoided evt)
	{
		if (!evt.Rule.ConcreteInitiator.IsDead)
		{
			MechanicEntity concreteInitiator = evt.Rule.ConcreteInitiator;
			TryAddCombatLogMessage(LogThreadBase.Strings.PsychicPhenomenaAvoided, concreteInitiator, evt.Rule.PsychicPhenomenaAvoid);
			TryAddCombatLogMessage(LogThreadBase.Strings.PerilsOfTheWarpAvoided, concreteInitiator, evt.Rule.PerilsOfTheWarpAvoid);
		}
	}

	public void TryAddCombatLogMessage(GameLogMessage gameLogMessage, MechanicEntity initiator, RuleCalculatePsychicPhenomenaEffect.PhenomenaAvoidResult avoidResult)
	{
		if (avoidResult.IsAvoided)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)initiator;
			CombatLogMessage combatLogMessage = gameLogMessage.CreateCombatLogMessage();
			if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
			{
				tooltipTemplateCombatLogMessage.ExtraInfoBricks = (tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(avoidResult).ToArray());
			}
			if (combatLogMessage != null)
			{
				AddMessage(combatLogMessage);
			}
		}
	}

	public static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleCalculatePsychicPhenomenaEffect.PhenomenaAvoidResult avoidResult)
	{
		yield return new TooltipBrickChance(UIStrings.Instance.CombatLog.PsychicPhenomenaAvoid, avoidResult.Chance, avoidResult.D100.Result, 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		yield return new TooltipBrickTextValue(LogThreadBase.Strings.TooltipBrickStrings.BaseModifier.Text, "0%", 2);
		IEnumerable<ITooltipBrick> enumerable = LogThreadBase.CreateBrickModifiers(avoidResult.ChanceModifiers.List, valueIsPercent: true, null, 2);
		foreach (ITooltipBrick item in enumerable)
		{
			yield return item;
		}
	}
}
