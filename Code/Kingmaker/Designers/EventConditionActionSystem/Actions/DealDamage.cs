using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5242e40ba3d06fb469bcf2cc11ed020d")]
public class DealDamage : GameAction
{
	public bool NoSource;

	[InfoBox("Note: If Target is peaceful NPC, damage wont be dealt. Use `NoSource = true` for such cases.")]
	[SerializeReference]
	[HideIf("NoSource")]
	public MechanicEntityEvaluator Source;

	[SerializeReference]
	[ValidateNotNull]
	public MechanicEntityEvaluator Target;

	public DamageDescription Damage;

	public bool DisableBattleLog;

	public bool DisableFxAndSound;

	public bool IgnorePeacefulZone;

	public override string GetDescription()
	{
		return $"Deal damage to {Target} with source {(NoSource ? Target : Source)}\n" + (DisableBattleLog ? "Log disabled" : "Log enabled") + "\n";
	}

	protected override void RunAction()
	{
		using (ContextData<GameLogDisabled>.RequestIf(DisableBattleLog))
		{
			Rulebook.Trigger(new RuleDealDamage((NoSource || !Source) ? Target.GetValue() : Source.GetValue(), Target.GetValue(), Damage.CreateDamage())
			{
				DisableGameLog = DisableBattleLog,
				DisableFxAndSound = DisableFxAndSound,
				IsIgnorePeacefulZone = IgnorePeacefulZone
			});
		}
	}

	public override string GetCaption()
	{
		return "Deal " + Damage.GetReadableFormula() + " Damage";
	}
}
