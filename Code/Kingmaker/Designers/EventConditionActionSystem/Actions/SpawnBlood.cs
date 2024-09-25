using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Visual.HitSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("9e9202ec6aca44728f3dfff49ce23e20")]
public class SpawnBlood : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override void RunAction()
	{
		if ((bool)Target)
		{
			DamageValue damage = new DamageValue(BlueprintRoot.Instance.Cheats.TestDamage.CreateDamage(), 5, 0, 0);
			AbstractUnitEntity value = Target.GetValue();
			HitFXPlayer.PlayDamageHit(value, value, null, null, null, damage, isCritical: false, isDot: false);
		}
	}

	public override string GetCaption()
	{
		return $"Spawn FX blood on ({Target})";
	}
}
