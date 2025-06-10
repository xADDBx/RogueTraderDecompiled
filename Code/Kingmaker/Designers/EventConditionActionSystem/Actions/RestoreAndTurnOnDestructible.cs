using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/RestoreAndTurnOnDestructible")]
[AllowMultipleComponents]
[TypeId("65bed6f3e3774ca38d8bb85029c810da")]
public class RestoreAndTurnOnDestructible : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator Target;

	public override string GetCaption()
	{
		return $"Восстанавливает и включает {Target}";
	}

	protected override void RunAction()
	{
		MapObjectEntity value = Target.GetValue();
		if (!value.CanBeTurnedOn)
		{
			UberDebug.Log($"{value} cannot be turned on because its CanBeTurnedOn == false");
			return;
		}
		PartHealth optional = value.GetOptional<PartHealth>();
		if (optional == null)
		{
			UberDebug.Log($"{value} cannot be healed because it hasn't PartHealth");
			return;
		}
		int maxHitPoints = optional.MaxHitPoints;
		Rulebook.Trigger(new RuleHealDamage(value, value, maxHitPoints));
		value.IsInGame = true;
	}
}
