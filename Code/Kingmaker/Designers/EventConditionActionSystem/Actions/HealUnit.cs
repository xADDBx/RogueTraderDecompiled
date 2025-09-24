using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/HealUnit")]
[AllowMultipleComponents]
[TypeId("3f63ecc3968426246bd07eec57d34cb4")]
public class HealUnit : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Source;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool ToFullHP;

	[HideIf("ToFullHP")]
	[SerializeReference]
	public IntEvaluator HealAmount;

	public override string GetDescription()
	{
		return $"Лечит цель {Target}";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		int maxHitPoints = value.Health.MaxHitPoints;
		int num = (ToFullHP ? maxHitPoints : HealAmount.GetValue());
		if (value.LifeState.IsUnconscious)
		{
			value.LifeState.Resurrect(num);
		}
		else
		{
			Rulebook.Trigger(new RuleHealDamage((Source != null) ? Source.GetValue() : value, value, num));
		}
	}

	public override string GetCaption()
	{
		return $"Heal {Target}";
	}
}
