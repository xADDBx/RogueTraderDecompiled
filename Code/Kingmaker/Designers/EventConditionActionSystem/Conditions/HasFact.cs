using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("f310985bf2724df4a97b165f74b806e8")]
[PlayerUpgraderAllowed(false)]
public class HasFact : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetConditionCaption()
	{
		return $"Check if {Unit} has {Fact}";
	}

	protected override bool CheckCondition()
	{
		return Unit.GetValue().Facts.Contains(Fact);
	}
}
