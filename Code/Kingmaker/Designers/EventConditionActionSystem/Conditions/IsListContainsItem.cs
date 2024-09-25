using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("34b0aecf92fe44f9a3c2fae68364983f")]
public class IsListContainsItem : Condition
{
	[SerializeReference]
	public ItemEvaluator Item;

	[ValidateNotNull]
	public BlueprintItemsList.Reference List;

	protected override string GetConditionCaption()
	{
		return $"Is {List?.Get().NameSafe()} contains [{Item}]";
	}

	protected override bool CheckCondition()
	{
		return (List?.Get()?.Contains(Item.GetValue().Blueprint)).GetValueOrDefault();
	}
}
