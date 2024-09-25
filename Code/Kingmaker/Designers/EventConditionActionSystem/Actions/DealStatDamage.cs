using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("283363841716498e96195afbb42278de")]
public class DealStatDamage : GameAction
{
	public bool NoSource;

	[HideIf("NoSource")]
	[SerializeReference]
	public AbstractUnitEvaluator Source;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public StatType Stat;

	public bool IsDrain;

	public DiceFormula DamageDice;

	public int DamageBonus;

	public bool DisableBattleLog;

	public override string GetDescription()
	{
		string text = (IsDrain ? "Drain stat" : "Deal damage to");
		return $"{text} {Stat} of {Target} with source {(NoSource ? Target : Source)}\n" + (DisableBattleLog ? "Log disabled" : "Log enabled") + "\n";
	}

	protected override void RunAction()
	{
		using (ContextData<GameLogDisabled>.RequestIf(DisableBattleLog))
		{
			if (!(Target.GetValue() is UnitEntity target))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
			}
			else
			{
				Rulebook.Trigger(new RuleDealStatDamage((NoSource || !Source) ? Target.GetValue() : Source.GetValue(), target, Stat, DamageDice, DamageBonus)
				{
					IsDrain = IsDrain,
					DisableGameLog = DisableBattleLog
				});
			}
		}
	}

	public override string GetCaption()
	{
		if (!IsDrain)
		{
			return "Deal stat Damage";
		}
		return "Drain stat";
	}

	public void Validate(ValidationContext context, int parentIndex)
	{
		if (!NoSource && !Source)
		{
			context.AddError("source is missing");
		}
		if (!Stat.IsAttribute())
		{
			context.AddError("attribute is missing");
		}
	}
}
