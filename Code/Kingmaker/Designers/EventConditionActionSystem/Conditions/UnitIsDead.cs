using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitIsDead")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("af04b660ed2c54f439b2dec09c84f5ad")]
public class UnitIsDead : Condition
{
	[InfoBox("This condition may fail if the unit has died and was later destroyed. Consider using SpawnerUnitIsDead or a Summon Pool.")]
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override string GetConditionCaption()
	{
		return $"({Target}) dead";
	}

	protected override bool CheckCondition()
	{
		return Target.GetValue().LifeState.IsDead;
	}
}
