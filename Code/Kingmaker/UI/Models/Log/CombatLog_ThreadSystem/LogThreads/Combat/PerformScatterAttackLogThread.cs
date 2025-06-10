using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class PerformScatterAttackLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAbility>
{
	public void HandleEvent(GameLogEventAbility evt)
	{
		if (!evt.IsScatter || evt.Context.DisableLog)
		{
			return;
		}
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Context.Caster;
		GameLogContext.Text = evt.Ability.Weapon?.Name ?? evt.Ability.Name;
		GameLogContext.AttacksCount = evt.ScatterShots.Count;
		GameLogEventAttack gameLogEventAttack = evt.ScatterAttacks.FindOrDefault((GameLogEventAttack o) => o != null && o.RollRuleDamage != null);
		if (gameLogEventAttack != null)
		{
			GameLogContext.DamageType = UIUtilityTexts.GetTextByKey(gameLogEventAttack.RollRuleDamage.Damage.Type);
		}
		int num = 0;
		int num2 = 0;
		foreach (GameLogEventAttack scatterAttack in evt.ScatterAttacks)
		{
			if (!scatterAttack.Rule.IsOverpenetration)
			{
				switch (scatterAttack.RollPerformAttackRule.Result)
				{
				case AttackResult.Hit:
				case AttackResult.RighteousFury:
					num++;
					break;
				case AttackResult.CoverHit:
					num2++;
					break;
				}
			}
		}
		int num3 = evt.ScatterShots.Count - num - num2;
		StringBuilder stringBuilder = GameLogUtility.StringBuilder;
		if (num > 0)
		{
			stringBuilder.Append($"<b>{num}</b> {UIStrings.Instance.CombatLog.ScatterShotHits.Text}");
		}
		if (num2 > 0)
		{
			if (num > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append($"<b>{num2}</b> {UIStrings.Instance.CombatLog.ScatterShotCoverHits.Text}");
		}
		if (num3 > 0)
		{
			if (num > 0 || num2 > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append($"<b>{num3}</b> {UIStrings.Instance.CombatLog.ScatterShotMiss.Text}");
		}
		GameLogContext.Description = stringBuilder.ToString();
		string text = "{source} " + LogThreadBase.Strings.WarhammerScatterHitFull.Message.Text;
		string tooltipHeader = TextTemplateEngineProxy.Instance.Process(text);
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.WarhammerScatterHitFull.CreateCombatLogMessage(null, tooltipHeader, isPerformAttackMessage: false, evt.Context.Caster);
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(evt).ToArray();
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = CollectExtraBricks(evt).ToArray();
		}
		AddMessage(combatLogMessage);
		AddMessage(new CombatLogMessage(null, isSeparator: true, GameLogEventAddSeparator.States.Break));
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(GameLogEventAbility evt)
	{
		int countAttack = 0;
		foreach (GameLogRuleEvent<RuleRollScatterShotHitDirection> scatterShot in evt.ScatterShots)
		{
			int index = scatterShot.Rule.ShotIndex + 1;
			RuleRollD100 resultD = scatterShot.Rule.ResultD100;
			yield return new TooltipBrickShotDirectionWithName(index, scatterShot.Rule.Result, scatterShot.Rule.ResultMainLineChance, scatterShot.Rule.ResultFarLineChance, resultD);
			bool isAttack = false;
			for (; countAttack < evt.ScatterAttacks.Count; countAttack++)
			{
				GameLogEventAttack gameLogEventAttack = evt.ScatterAttacks[countAttack];
				if (gameLogEventAttack.RollPerformAttackRule.BurstIndex + 1 != index)
				{
					break;
				}
				isAttack = true;
				GameLogContext.ResultDamage = gameLogEventAttack.Rule.ResultDamageValue;
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(MechanicEntity)gameLogEventAttack.Rule.Target;
				GameLogContext.Description = (gameLogEventAttack.IsOverpenetrationTrigger ? LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text : null);
				AttackResult result = gameLogEventAttack.Rule.Result;
				string text = ((result == AttackResult.Hit || result == AttackResult.CoverHit || result == AttackResult.RighteousFury) ? LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackHit.Text : LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackMiss.Text) + " " + GetAttackResultText(gameLogEventAttack.Rule.Result);
				yield return new TooltipBrickIconText(text);
			}
			if (!isAttack)
			{
				yield return new TooltipBrickIconText(LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackMiss.Text + " " + LogThreadBase.Strings.TooltipBrickStrings.ScatterAttackNoTarget.Text, isShowIcon: false);
			}
		}
	}

	private static string GetAttackResultText(AttackResult result)
	{
		return result switch
		{
			AttackResult.Unknown => GameLogStrings.Instance.AttackResultStrings.AttackResultUnknown.Text, 
			AttackResult.Hit => GameLogStrings.Instance.WarhammerDealDamage.Message.Text, 
			AttackResult.CoverHit => GameLogStrings.Instance.WarhammerCoverHit.Message.Text, 
			AttackResult.Miss => GameLogStrings.Instance.WarhammerMiss.Message.Text, 
			AttackResult.Dodge => GameLogStrings.Instance.WarhammerDodge.Message.Text, 
			AttackResult.RighteousFury => GameLogStrings.Instance.WarhammerRFHit.Message.Text, 
			AttackResult.Parried => GameLogStrings.Instance.WarhammerParry.Message.Text, 
			AttackResult.Blocked => "%BLOCKED%", 
			_ => GameLogStrings.Instance.AttackResultStrings.AttackResultUnknown.Text, 
		};
	}
}
