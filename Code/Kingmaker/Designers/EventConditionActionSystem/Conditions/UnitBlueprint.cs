using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitBlueprint")]
[AllowMultipleComponents]
[TypeId("6d4c821bf43731342a33176b6d074a6c")]
public class UnitBlueprint : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	protected override string GetConditionCaption()
	{
		return $"Unit Checker ({Blueprint})";
	}

	protected override bool CheckCondition()
	{
		return Unit.GetValue().Blueprint.CheckEqualsWithPrototype(Blueprint);
	}
}
