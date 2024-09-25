using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookCanApplyBuffLogThread : LogThreadBase, IGameLogRuleHandler<RuleCalculateCanApplyBuff>
{
	public void HandleEvent(RuleCalculateCanApplyBuff rule)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(rule);
		if (combatLogMessage != null)
		{
			AddMessage(combatLogMessage);
		}
	}

	public static CombatLogMessage GetCombatLogMessage(RuleCalculateCanApplyBuff rule)
	{
		if (rule.ConcreteInitiator.IsDisposed || rule.ConcreteInitiator.IsDead)
		{
			return null;
		}
		if (!rule.AppliedBuff.IsTraumas && (rule.Blueprint.IsHiddenInUI || ShowInLogOnlyOnYourself(rule)))
		{
			return null;
		}
		MechanicEntity concreteInitiator = rule.ConcreteInitiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)concreteInitiator;
		GameLogContext.Text = rule.Context.Name;
		GameLogMessage gameLogMessage = GetGameLogMessage(rule);
		bool flag = rule.AppliedBuff.IsProne && rule.AppliedBuff.Context.MaybeCaster != rule.Reason.Caster;
		return gameLogMessage?.CreateCombatLogMessage(new TooltipTemplateBuff(rule.AppliedBuff, flag ? rule.Reason.Caster : null), null, isPerformAttackMessage: false, concreteInitiator);
	}

	private static GameLogMessage GetGameLogMessage(RuleCalculateCanApplyBuff rule)
	{
		if (rule.Immunity)
		{
			return LogThreadBase.Strings.SpellImmunity;
		}
		if (rule.AppliedBuff.IsAttached)
		{
			if (rule.AppliedBuff.IsTraumas)
			{
				return LogThreadBase.Strings.BuffTrauma;
			}
			if (rule.AppliedBuff.IsWounds)
			{
				return LogThreadBase.Strings.BuffWound;
			}
			return LogThreadBase.Strings.StatusEffect;
		}
		return null;
	}

	private static bool ShowInLogOnlyOnYourself(RuleCalculateCanApplyBuff rule)
	{
		MechanicsContext maybeContext = rule.AppliedBuff.MaybeContext;
		if (rule.Blueprint.ShowInLogOnlyOnYourself && maybeContext != null && maybeContext.MaybeOwner != null && maybeContext.MaybeCaster != null)
		{
			return maybeContext.MaybeOwner != maybeContext.MaybeCaster;
		}
		return false;
	}
}
